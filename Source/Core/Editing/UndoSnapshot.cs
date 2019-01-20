
#region ================== Copyright (c) 2007 Pascal vd Heiden

/*
 * Copyright (c) 2007 Pascal vd Heiden, www.codeimp.com
 * This program is released under GNU General Public License
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 */

#endregion

#region ================== Namespaces

using System;
using System.IO;
using CodeImp.DoomBuilder.GZBuilder.Data; //mxd

#endregion

namespace CodeImp.DoomBuilder.Editing
{
	public class UndoSnapshot : IDisposable
	{
		#region ================== Variables

		private MemoryStream recstream;
		private string filename;
		private string description;
		private readonly int ticketid;			// For safe withdrawing
		private volatile bool storeondisk;
		private volatile bool isondisk;
		private bool isdisposed;
		//private Dictionary<string, MemoryStream> customdata;
		
		#endregion

		#region ================== Properties

		public string Description { get { return description; } set { description = value; } }
		public int TicketID { get { return ticketid; } }
		internal bool StoreOnDisk { get { return storeondisk; } set { storeondisk = value; } }
		public bool IsOnDisk { get { return isondisk; } }
		
		#endregion

		#region ================== Constructor / Disposer

		// Constructor
		internal UndoSnapshot(string description, MemoryStream recstream, int ticketid)
		{
			if(recstream == null) General.Fail("Argument cannot be null!");
			this.ticketid = ticketid;
			this.description = description;
			this.recstream = recstream;
			this.filename = null;
		}

		// Constructor
		internal UndoSnapshot(UndoSnapshot info, MemoryStream recstream)
		{
			if(recstream == null) General.Fail("Argument cannot be null!");
			this.ticketid = info.ticketid;
			this.description = info.description;
			this.recstream = recstream;
			this.filename = null;
		}

		// Disposer
		public void Dispose()
		{
			lock(this)
			{
				isdisposed = true;
				if(recstream != null) recstream.Dispose();
				recstream = null;
				if(isondisk) File.Delete(filename);
				isondisk = false;
			}
		}

		#endregion
		
		#region ================== Methods

		// This returns the map data
		internal MemoryStream GetStream()
		{
			lock(this)
			{
				// Restore into memory if needed
				if(isondisk) RestoreFromFile();
				
				// Return the buffer
				return recstream;
			}
		}
		
		// This moves the snapshot from memory to harddisk
		internal void WriteToFile()
		{
			lock(this)
			{
				if(isdisposed) return;
				if(isondisk) return;
				isondisk = true;
				
				// Compress data
				recstream.Seek(0, SeekOrigin.Begin);
				MemoryStream outstream = SharpCompressHelper.CompressStream(recstream); //mxd

				// Make temporary file
				filename = General.MakeTempFilename(General.Map.TempPath, "snapshot");

				// Write data to file
				File.WriteAllBytes(filename, outstream.ToArray());

				// Remove data from memory
				recstream.Dispose();
				recstream = null;
				outstream.Dispose();
			}
		}

		// This loads the snapshot from harddisk into memory
		internal void RestoreFromFile()
		{
			lock(this)
			{
				if(isdisposed) return;
				if(!isondisk) return;
				isondisk = false;

				// Read the file data
				MemoryStream instream = new MemoryStream(File.ReadAllBytes(filename));
				
				// Decompress data
				MemoryStream outstream = SharpCompressHelper.DecompressStream(instream); //mxd
				recstream = new MemoryStream(outstream.ToArray());
				
				// Clean up
				instream.Dispose();
				File.Delete(filename);
				filename = null;
				outstream.Dispose();
			}
		}
		
		#endregion
	}
}
