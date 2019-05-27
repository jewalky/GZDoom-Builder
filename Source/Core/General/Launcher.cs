
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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Actions;
using CodeImp.DoomBuilder.Data;
using CodeImp.DoomBuilder.Editing;
using CodeImp.DoomBuilder.IO;
using CodeImp.DoomBuilder.Windows;

#endregion

namespace CodeImp.DoomBuilder
{
	internal class Launcher : IDisposable
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		private string tempwad;
		private Dictionary<Process, string> processes; //mxd
		private bool isdisposed;

		delegate void EngineExitedCallback(Process p); //mxd
		
		#endregion

		#region ================== Properties

		public string TempWAD { get { return tempwad; } }

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public Launcher(MapManager manager)
		{
			// Initialize
			CleanTempFile(manager);
			processes = new Dictionary<Process, string>(); //mxd

			// Bind actions
			General.Actions.BindMethods(this);
		}

		// Disposer
		public void Dispose()
		{
			// Not yet disposed?
			if(!isdisposed)
			{
				// Unbind actions
				General.Actions.UnbindMethods(this);

				//mxd. Terminate running processes?
				if(processes != null) 
				{
					foreach(KeyValuePair<Process, string> group in processes)
					{
						// Close engine
						group.Key.CloseMainWindow();
						group.Key.Close();

						// Remove temporary file
						if(File.Exists(group.Value))
						{
							try { File.Delete(group.Value); }
							catch { }
						}
					}
				}
				
				// Remove temporary file
				if(File.Exists(tempwad))
				{
					try { File.Delete(tempwad); }
					catch { }
				}
				
				// Done
				isdisposed = true;
			}
		}

		#endregion

		#region ================== Parameters

		// This takes the unconverted parameters (with placeholders) and converts it
		// to parameters with full paths, names and numbers where placeholders were put.
		// The tempfile must be the full path and filename to the PWAD file to test.
		public string ConvertParameters(string parameters, int skill, bool shortpaths, bool linuxpaths)
		{
			string outp = parameters;
			DataLocation iwadloc;
			string p_wp = "", p_wf = "";
			string p_ap = "", p_apq = "";
			string p_l1 = "", p_l2 = "";
			string p_nm = "";
			string f = tempwad;
			
			// Make short path if needed
			if(shortpaths) f = General.GetShortFilePath(f);
			else if (linuxpaths) f = General.GetLinuxFilePath(f);
			// Find the first IWAD file
			if(General.Map.Data.FindFirstIWAD(out iwadloc))
			{
				// %WP and %WF result in IWAD file
				p_wp = iwadloc.location;
				p_wf = Path.GetFileName(p_wp);
				if(shortpaths)
				{
					p_wp = General.GetShortFilePath(p_wp);
					p_wf = General.GetShortFilePath(p_wf);
				}
				else if (linuxpaths)
				{
					p_wp = General.GetLinuxFilePath(p_wp);
					p_wf = General.GetLinuxFilePath(p_wf);
				}
			}
			
			// Make a list of all data locations, including map location
			DataLocationList locations = DataLocationList.Combined(General.Map.ConfigSettings.Resources, General.Map.Options.Resources);

			//mxd. General.Map.FilePathName will be empty when a newly created map was not saved yet.
			if(!string.IsNullOrEmpty(General.Map.FilePathName))
			{
				DataLocation maplocation = new DataLocation(DataLocation.RESOURCE_WAD, General.Map.FilePathName, false, false, false);
				locations.Remove(maplocation); //If maplocation was already added as a resource, make sure it's singular and is last in the list
				locations.Add(maplocation); 
			}

			// Go for all data locations
			foreach(DataLocation dl in locations)
			{
				// Location not the IWAD file?
				if((dl.type != DataLocation.RESOURCE_WAD) || (dl.location != iwadloc.location))
				{
					// Location not included?
					if(!dl.notfortesting)
					{
						// Add to string of files
						if(shortpaths)
						{
							p_ap += General.GetShortFilePath(dl.location) + " ";
							p_apq += "\"" + General.GetShortFilePath(dl.location) + "\" ";
						}
						else if (linuxpaths)
						{
							p_ap += General.GetLinuxFilePath(dl.location) + " ";
							p_apq += "\"" + General.GetLinuxFilePath(dl.location) + "\" ";
						}
						else
						{
							p_ap += dl.location + " ";
							p_apq += "\"" + dl.location + "\" ";
						}
					}
				}
			}

			// Trim last space from resource file locations
			p_ap = p_ap.TrimEnd(' ');
			p_apq = p_apq.TrimEnd(' ');

			// Try finding the L1 and L2 numbers from the map name
			string numstr = "";
			bool first = true;
			foreach(char c in General.Map.Options.CurrentName)
			{
				// Character is a number?
				if(Configuration.NUMBERS.IndexOf(c) > -1)
				{
					// Include it
					numstr += c;
				}
				else
				{
					// Store the number if we found one
					if(numstr.Length > 0)
					{
						int num;
						int.TryParse(numstr, out num);
						if(first) p_l1 = num.ToString(); else p_l2 = num.ToString();
						numstr = "";
						first = false;
					}
				}
			}
			
			// Store the number if we found one
			if(numstr.Length > 0)
			{
				int num;
				int.TryParse(numstr, out num);
				if(first) p_l1 = num.ToString(); else p_l2 = num.ToString();
			}

			// No monsters?
			if(!General.Settings.TestMonsters) p_nm = "-nomonsters";
			
			// Make sure all our placeholders are in uppercase
			outp = outp.Replace("%f", "%F");
			outp = outp.Replace("%wp", "%WP");
			outp = outp.Replace("%wf", "%WF");
			outp = outp.Replace("%wP", "%WP");
			outp = outp.Replace("%wF", "%WF");
			outp = outp.Replace("%Wp", "%WP");
			outp = outp.Replace("%Wf", "%WF");
			outp = outp.Replace("%l1", "%L1");
			outp = outp.Replace("%l2", "%L2");
			outp = outp.Replace("%l", "%L");
			outp = outp.Replace("%ap", "%AP");
			outp = outp.Replace("%aP", "%AP");
			outp = outp.Replace("%Ap", "%AP");
			outp = outp.Replace("%s", "%S");
			outp = outp.Replace("%nM", "%NM");
			outp = outp.Replace("%Nm", "%NM");
			outp = outp.Replace("%nm", "%NM");
			
			// Replace placeholders with actual values
			outp = outp.Replace("%F", f);
			outp = outp.Replace("%WP", p_wp);
			outp = outp.Replace("%WF", p_wf);
			outp = outp.Replace("%L1", p_l1);
			outp = outp.Replace("%L2", p_l2);
			outp = outp.Replace("%L", General.Map.Options.CurrentName);
			outp = outp.Replace("\"%AP\"", p_apq);
			outp = outp.Replace("%AP", p_ap);
			outp = outp.Replace("%S", skill.ToString());
			outp = outp.Replace("%NM", p_nm);
			
			// Return result
			return outp;
		}

		#endregion

		#region ================== Test

		// This saves the map to a temporary file and launches a test
		[BeginAction("testmap")]
		public void Test()
		{
			TestAtSkill(General.Map.ConfigSettings.TestSkill, false);
		}

		//mxd
		[BeginAction("testmapfromview")]
		public void TestFromView() 
		{
			TestAtSkill(General.Map.ConfigSettings.TestSkill, true);
		}
		
		// This saves the map to a temporary file and launches a test with the given skill
		public void TestAtSkill(int skill) { TestAtSkill(skill, false); }
		public void TestAtSkill(int skill, bool testfromcurrentposition)
		{
			if(!General.Editing.Mode.OnMapTestBegin(testfromcurrentposition)) return; //mxd
			
			Cursor oldcursor = Cursor.Current;

			// Check if configuration is OK
			if(string.IsNullOrEmpty(General.Map.ConfigSettings.TestProgram) || !File.Exists(General.Map.ConfigSettings.TestProgram))
			{
				//mxd. Let's be more precise
				string message;
				if(General.Map.ConfigSettings.TestProgram == "")
					message = "Your test program is not set for the current game configuration";
				else
					message = "Current test program has invalid path";
				
				// Show message
				Cursor.Current = Cursors.Default;
				DialogResult result = General.ShowWarningMessage(message + ". Would you like to set up your test program now?", MessageBoxButtons.YesNo);
				if(result == DialogResult.Yes)
				{
					// Show game configuration on the right page
					General.MainWindow.ShowConfigurationPage(2);
				}
				return;
			}

			// No custom parameters?
			if(!General.Map.ConfigSettings.CustomParameters)
			{
				// Set parameters to the default ones
				General.Map.ConfigSettings.TestParameters = General.Map.Config.TestParameters;
				General.Map.ConfigSettings.TestShortPaths = General.Map.Config.TestShortPaths;
				General.Map.ConfigSettings.TestLinuxPaths = General.Map.Config.TestLinuxPaths;
			}
			
			// Remove temporary file
			if(File.Exists(tempwad) && !processes.ContainsValue(tempwad))
			{
				try { File.Delete(tempwad); }
				catch { }
			}
			
			// Save map to temporary file
			Cursor.Current = Cursors.WaitCursor;
			tempwad = General.MakeTempFilename(General.Map.TempPath, "wad");
			General.Plugins.OnMapSaveBegin(SavePurpose.Testing);
			if(General.Map.SaveMap(tempwad, SavePurpose.Testing))
			{
				// No compiler errors?
				if(General.Map.Errors.Count == 0)
				{
					// Make arguments
					string args = ConvertParameters(General.Map.ConfigSettings.TestParameters, skill, General.Map.ConfigSettings.TestShortPaths, General.Map.ConfigSettings.TestLinuxPaths);

					// Setup process info
					ProcessStartInfo processinfo = new ProcessStartInfo();
					processinfo.Arguments = args;
					processinfo.FileName = General.Map.ConfigSettings.TestProgram;
					processinfo.CreateNoWindow = false;
					processinfo.ErrorDialog = false;
					processinfo.UseShellExecute = true;
					processinfo.WindowStyle = ProcessWindowStyle.Normal;
					processinfo.WorkingDirectory = Path.GetDirectoryName(processinfo.FileName);

					// Output info
					General.WriteLogLine("Running test program: " + processinfo.FileName);
					General.WriteLogLine("Program parameters:  " + processinfo.Arguments);
					General.MainWindow.DisplayStatus(StatusType.Info, "Launching " + processinfo.FileName + "...");

					try
					{
						// Start the program
						Process process = Process.Start(processinfo);
						process.EnableRaisingEvents = true; //mxd
						process.Exited += ProcessOnExited; //mxd
						processes.Add(process, tempwad); //mxd
						Cursor.Current = oldcursor; //mxd
					}
					catch(Exception e)
					{
						// Unable to start the program
						General.ShowErrorMessage("Unable to start the test program, " + e.GetType().Name + ": " + e.Message, MessageBoxButtons.OK);
					}
				}
				else
				{
					General.MainWindow.DisplayStatus(StatusType.Warning, "Unable to test the map due to script errors.");
				}
			}
			General.Plugins.OnMapSaveEnd(SavePurpose.Testing);
			General.Editing.Mode.OnMapTestEnd(testfromcurrentposition); //mxd
		}

		//mxd
		private void TestingFinished(Process process) 
		{
			// Done
			TimeSpan deltatime = TimeSpan.FromTicks(process.ExitTime.Ticks - process.StartTime.Ticks);
			General.WriteLogLine("Testing with \"" + process.StartInfo.FileName + "\" has finished.");
			General.WriteLogLine("Run time: " + deltatime.TotalSeconds.ToString("###########0.00") + " seconds");

			//mxd. Remove from active processes list
			string closedtempfile = processes[process];
			processes.Remove(process);

			//mxd. Still have running engines?..
			if(processes.Count > 0)
			{
				// Remove temp file
				if(File.Exists(closedtempfile))
				{
					try { File.Delete(closedtempfile); }
					catch { }
				}
				return; 
			}
			
			General.MainWindow.DisplayReady();

			// Clean up temp file
			CleanTempFile(General.Map);

			if(General.Map != null)
			{
				// Device reset may be needed...
				if(General.Editing.Mode is ClassicMode)
				{
					General.Map.Graphics.Reset();
					General.MainWindow.RedrawDisplay();
				}
			}

			General.MainWindow.FocusDisplay();
		}

		//mxd
		private void ProcessOnExited(object sender, EventArgs e)
		{
			General.MainWindow.Invoke(new EngineExitedCallback(TestingFinished), new[] { sender });
		}

		// This deletes the previous temp file and creates a new, empty temp file
		private void CleanTempFile(MapManager manager)
		{
			// Remove temporary file
			try { File.Delete(tempwad); }
			catch { }
			
			// Make new empty temp file
			tempwad = General.MakeTempFilename(manager.TempPath, "wad");
			File.WriteAllText(tempwad, "");
		}

		#endregion
	}
}
