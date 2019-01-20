#region === Copyright (c) 2010 Pascal van der Heiden ===

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

#endregion

namespace CodeImp.DoomBuilder.Plugins.VisplaneExplorer
{
	internal class VPOManager : IDisposable
	{
		#region ================== Constants

		public const int POINTS_PER_ITERATION = 100;
		private const int EXPECTED_RESULTS_BUFFER = 200000;

		private readonly int[] TEST_ANGLES = new[] { 0, 90, 180, 270, 45, 135, 225, 315 /*, 22, 67, 112, 157, 202, 247, 292, 337 */ };
		private const int TEST_HEIGHT = 41 + 8;
		
		#endregion

		#region ================== APIs

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr LoadLibrary(string filename);

		[DllImport("kernel32.dll")]
		private static extern IntPtr GetProcAddress(IntPtr modulehandle, string procedurename);

		[DllImport("kernel32.dll")]
		private static extern bool FreeLibrary(IntPtr modulehandle);
		
		#endregion

		#region ================== Delegates

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate string VPO_GetError();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate int VPO_LoadWAD(string filename);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate int VPO_OpenMap(string mapname, ref bool isHexen);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate void VPO_FreeWAD();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate void VPO_CloseMap();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] //mxd
		private delegate void VPO_OpenDoorSectors(int dir);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate int VPO_TestSpot(int x, int y, int dz, int angle,
			ref int visplanes, ref int drawsegs, ref int openings, ref int solidsegs);

		#endregion

		#region ================== Variables

		// Main objects
		private readonly string[] tempfiles;
		private IntPtr[] dlls;
		private Thread[] threads;

		// Map to load
		private string filename;
		private string mapname;

		// Input and output queue (both require a lock on 'points' !)
		private readonly Queue<TilePoint> points = new Queue<TilePoint>(EXPECTED_RESULTS_BUFFER);
		private readonly Queue<PointData> results = new Queue<PointData>(EXPECTED_RESULTS_BUFFER);
		
		#endregion

		#region ================== Properties

		public int NumThreads { get { return Environment.ProcessorCount; } }

		#endregion

		#region ================== Constructor / Destructor
		
		// Constructor
		public VPOManager()
		{
			// Load a DLL for each thread
			dlls = new IntPtr[NumThreads];
			tempfiles = new string[NumThreads];
			
			// We must write the DLL file with a unique name for every thread,
			// because LoadLibrary will share loaded libraries with the same
			// names and LoadLibraryEx does not have a flag to force loading
			// it multiple times.
			Assembly thisasm = Assembly.GetExecutingAssembly();
			Stream dllstream = thisasm.GetManifestResourceStream("CodeImp.DoomBuilder.Plugins.VisplaneExplorer.Resources.vpo.dll");
			dllstream.Seek(0, SeekOrigin.Begin);
			byte[] dllbytes = new byte[dllstream.Length];
			dllstream.Read(dllbytes, 0, dllbytes.Length);
			for(int i = 0; i < dlls.Length; i++)
			{
				// Write file with unique filename
				tempfiles[i] = BuilderPlug.MakeTempFilename(".dll");
				File.WriteAllBytes(tempfiles[i], dllbytes);
				
				// Load it
				dlls[i] = LoadLibrary(tempfiles[i]);
				if(dlls[i] == IntPtr.Zero)
				{
					int error = Marshal.GetLastWin32Error(); //mxd
					throw new Exception("Error " + error + " while loading vpo.dll: " + new Win32Exception(error).Message);
				}
			}
		}

		// Disposer
		public void Dispose()
		{
			if(threads != null) Stop();
			
			if(dlls != null)
			{
				for(int i = 0; i < dlls.Length; i++)
				{
					FreeLibrary(dlls[i]);
					File.Delete(tempfiles[i]);
				}
				dlls = null;
			}
		}
		
		#endregion

		#region ================== Processing

		// The thread!
		private void ProcessingThread(object index)
		{
			// Get function pointers
			//VPO_GetError GetError = (VPO_GetError)Marshal.GetDelegateForFunctionPointer(GetProcAddress(dlls[(int)index], "VPO_GetError"), typeof(VPO_GetError));
			VPO_LoadWAD LoadWAD = (VPO_LoadWAD)Marshal.GetDelegateForFunctionPointer(GetProcAddress(dlls[(int)index], "VPO_LoadWAD"), typeof(VPO_LoadWAD));
			VPO_OpenMap OpenMap = (VPO_OpenMap)Marshal.GetDelegateForFunctionPointer(GetProcAddress(dlls[(int)index], "VPO_OpenMap"), typeof(VPO_OpenMap));
			VPO_FreeWAD FreeWAD = (VPO_FreeWAD)Marshal.GetDelegateForFunctionPointer(GetProcAddress(dlls[(int)index], "VPO_FreeWAD"), typeof(VPO_FreeWAD));
			VPO_CloseMap CloseMap = (VPO_CloseMap)Marshal.GetDelegateForFunctionPointer(GetProcAddress(dlls[(int)index], "VPO_CloseMap"), typeof(VPO_CloseMap));
			VPO_OpenDoorSectors OpenDoors = (VPO_OpenDoorSectors)Marshal.GetDelegateForFunctionPointer(GetProcAddress(dlls[(int)index], "VPO_OpenDoorSectors"), typeof(VPO_OpenDoorSectors)); //mxd
			VPO_TestSpot TestSpot = (VPO_TestSpot)Marshal.GetDelegateForFunctionPointer(GetProcAddress(dlls[(int)index], "VPO_TestSpot"), typeof(VPO_TestSpot));

			try
			{
				// Load the map
				bool isHexen = General.Map.HEXEN;
				if(LoadWAD(filename) != 0) throw new Exception("VPO is unable to read this file.");
				if(OpenMap(mapname, ref isHexen) != 0) throw new Exception("VPO is unable to open this map.");
				OpenDoors(BuilderPlug.InterfaceForm.OpenDoors ? 1 : -1); //mxd

				// Processing
				Queue<TilePoint> todo = new Queue<TilePoint>(POINTS_PER_ITERATION);
				Queue<PointData> done = new Queue<PointData>(POINTS_PER_ITERATION);
				while(true)
				{
					lock(points)
					{
						// Flush done points to the results
						int numdone = done.Count;
						for(int i = 0; i < numdone; i++)
							results.Enqueue(done.Dequeue());
						
						// Get points from the waiting queue into my todo queue for processing
						int numtodo = Math.Min(POINTS_PER_ITERATION, points.Count);
						for(int i = 0; i < numtodo; i++)
							todo.Enqueue(points.Dequeue());
					}

					// Don't keep locking!
					if(todo.Count == 0)
						Thread.Sleep(31);
					
					// Process the points
					while(todo.Count > 0)
					{
						TilePoint p = todo.Dequeue();
						PointData pd = new PointData();
						pd.point = p;

						for(int i = 0; i < TEST_ANGLES.Length; i++)
						{
							pd.result = (PointResult)TestSpot(p.x, p.y, TEST_HEIGHT, TEST_ANGLES[i],
								ref pd.visplanes, ref pd.drawsegs, ref pd.openings, ref pd.solidsegs);
						}

						done.Enqueue(pd);
					}
				}
			}
			catch(ThreadInterruptedException)
			{
			}
			finally
			{
				CloseMap();
				FreeWAD();
			}
		}

		#endregion

		#region ================== Public Methods

		// This loads a map
		public void Start(string filename, string mapname)
		{
			this.filename = filename;
			this.mapname = mapname;
			results.Clear();
			
			// Start a thread on each core
			threads = new Thread[dlls.Length];
			for(int i = 0; i < threads.Length; i++)
			{
				threads[i] = new Thread(ProcessingThread);
				threads[i].Priority = ThreadPriority.BelowNormal;
				threads[i].Name = "Visplane Explorer " + i;
				threads[i].Start(i);
			}
		}

		// This frees the map
		public void Stop()
		{
			if(threads != null)
			{
				lock(points)
				{
					// Stop all threads
					for(int i = 0; i < threads.Length; i++)
					{
						threads[i].Interrupt();
						threads[i].Join();
					}
					threads = null;
					points.Clear();
					results.Clear();
				}
			}
		}

		// This clears the list of enqueued points
		/*public void ClearPoints()
		{
			lock(points)
			{
				points.Clear();
			}
		}*/

		// This gives points to process and returns the total points left in the buffer
		public int EnqueuePoints(IEnumerable<TilePoint> newpoints)
		{
			lock(points)
			{
				foreach(TilePoint p in newpoints)
					points.Enqueue(p);
				return points.Count;
			}
		}

		// This fetches results (in 'data') and returns the number of points
		// remaining to be processed.
		public int DequeueResults(List<PointData> data)
		{
			lock(points)
			{
				int numresults = results.Count;
				if(data.Capacity - data.Count < numresults)
					data.Capacity = data.Count + numresults;
				for(int i = 0; i < numresults; i++)
					data.Add(results.Dequeue());
				return points.Count;
			}
		}

		// This returns the number of points left in the buffer
		public int GetRemainingPoints()
		{
			lock(points)
			{
				return points.Count;
			}
		}

		#endregion
	}
}
