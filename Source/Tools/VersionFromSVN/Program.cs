#region ======================== Namespaces

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;

#endregion

namespace mxd.VersionFromGIT
{
	public static class Program
	{
		#region ======================== Constants

		private const string GIT_INFO = "@echo off\r\ngit rev-list --count origin/master\r\ngit rev-parse --short=7 origin/master";

		#endregion

		#region ======================== Main

		[STAThread]
		private static int Main(string[] args)
		{
			bool showusageinfo = true;
			bool dorevisionlookup = true;
			bool reverttargets = false;
			string revision = "";
			string shorthash = "";
			string apppath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			string revisionoutputfile = "";

			List<string> targetfiles = new List<string>();
			Queue<string> queue = new Queue<string>(args);
			
			if(string.IsNullOrEmpty(apppath))
			{
				Console.WriteLine("Invalid working directory: '" + apppath + "'");
				return 2;
			}

			while(queue.Count > 0)
			{
				string arg = queue.Dequeue();
				if(string.Compare(arg, "-F", true) == 0)
				{
					if(queue.Count > 0)
					{
						revision = queue.Dequeue();
						dorevisionlookup = false;
					}
					else
					{
						showusageinfo = true;
						break;
					}
				}
				else if(string.Compare(arg, "-O", true) == 0)
				{
					if(queue.Count > 0) revisionoutputfile = queue.Dequeue();
				}
				else if(string.Compare(arg, "-R", true) == 0)
				{
					dorevisionlookup = false;
					reverttargets = true;
				}
				else
				{
					targetfiles.Add(arg);
					showusageinfo = false;
				}
			}

			if(showusageinfo)
			{
				ShowUsageInfo();
				Console.ReadKey();
				return 0;
			}

			if(dorevisionlookup)
			{
				Console.WriteLine("Looking up latest git revision...");

				string batpath = Path.Combine(apppath, "versionfromgit.bat");
				File.WriteAllText(batpath, GIT_INFO);

				// Redirect the output stream of the child process.
				Process p = new Process
				{
					StartInfo =
					{
						UseShellExecute = false, 
						RedirectStandardOutput = true,
						WindowStyle = ProcessWindowStyle.Hidden,
						FileName = batpath,
						WorkingDirectory = apppath
					}
				};
				// Redirect the output stream of the child process.
				p.Start();

				// Read the output stream first and then wait.
				string output = p.StandardOutput.ReadToEnd().Trim(); //mxd. first line is revision, second - short hash
				p.WaitForExit();
				File.Delete(batpath);

				string[] parts = output.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries); 
				if(parts.Length != 2)
				{
					Console.WriteLine("Unable to get Git info from string \"" + output + "\". You must install Git from https://git-scm.com");
					return 3;
				}

				//mxd. Check hash
				shorthash = parts[1];
				if(shorthash.Length != 7)
				{
					Console.WriteLine("Unable to get Git hash from string \"" + shorthash + "\". You must install Git from https://git-scm.com");
					return 4;
				}
				
				//mxd. Check revision
				int unused;
				revision = parts[0];
				if(string.IsNullOrEmpty(revision) || !int.TryParse(revision, out unused) || unused < 1)
				{
					Console.WriteLine("Unable to get Git commits count from string \"" + revision + "\". You must install Git from https://git-scm.com");
					return 1;
				}
			}

			if(!string.IsNullOrEmpty(revisionoutputfile))
				File.AppendAllText(revisionoutputfile, "SET REVISIONNUMBER=" + revision + "\nSET REVISIONHASH=" + shorthash);
					
			if(reverttargets)
			{
				Console.WriteLine("Reverting changes in version files...");

				string contents = "";
				foreach(string str in targetfiles)
					contents += "git checkout \"" + str + "\"\r\n";
						
				string path = Path.Combine(apppath, "versionfromgit.bat");
				File.WriteAllText(path, contents);

				ProcessStartInfo info = new ProcessStartInfo
				                             {
					                             FileName = path,
												 CreateNoWindow = false,
												 ErrorDialog = false,
												 UseShellExecute = true,
												 WindowStyle = ProcessWindowStyle.Hidden,
												 WorkingDirectory = apppath
				                             };

				Process.Start(info).WaitForExit();
				File.Delete(path);
			}
			else
			{
				Console.WriteLine("Writing revision " + revision + ", hash " + shorthash + " to target files...");
				foreach(string file in targetfiles)
				{
					bool changed = false;
					string[] contents = File.ReadAllLines(file);

					for(int i = 0; i < contents.Length; ++i)
					{
						string line = contents[i];
						if(line.Trim().StartsWith("[assembly: AssemblyVersion", true, CultureInfo.InvariantCulture))
						{
							int startbracepos = line.IndexOf("\"", 0);
							int endbracepos = line.IndexOf("\"", startbracepos + 1);
							int revisiondotpos = line.LastIndexOf(".", endbracepos, endbracepos - startbracepos);

							string linestart = line.Substring(0, revisiondotpos + 1);
							string lineend = line.Substring(endbracepos);
							string result = linestart + revision + lineend;

							if(string.Compare(contents[i], result, true) != 0)
							{
								contents[i] = result;
								changed = true;
							}
						}
						//mxd. Apply hash
						else if(line.Trim().StartsWith("[assembly: AssemblyHash", true, CultureInfo.InvariantCulture))
						{
							int startbracepos = line.IndexOf("\"", 0);
							int endbracepos = line.IndexOf("\"", startbracepos + 1);

							string linestart = line.Substring(0, startbracepos + 1);
							string lineend = line.Substring(endbracepos);
							string result = linestart + shorthash + lineend;

							if(string.Compare(contents[i], result, true) != 0)
							{
								contents[i] = result;
								changed = true;
							}
						}
					}

					if(changed)
					{
						File.Copy(file, file + ".backup");
						File.WriteAllLines(file, contents);
						File.Delete(file + ".backup");
					}
				}
			}

			Console.WriteLine("Done.");
			return 0;
		}

		#endregion

		#region ======================== Usage info

		private static void ShowUsageInfo()
		{
			Console.WriteLine("Version From GIT is a tool programmed by MaxED.\r\nThis tool requires that Git commandline client is installed.\r\nSee https://git-scm.com\r\n");
			Console.WriteLine("Usage: versionfromgit.exe targetfile [targetfile ...] [options]");
			Console.WriteLine("Where targetfile is AssemblyInfo.cs file in which AssemblyVersion property value must be replaced with the commits count of the GIT master branch.\r\n");

			Console.WriteLine("Options:\r\n");
			Console.WriteLine("-F number");
			Console.WriteLine("When this is used, the given number will be used as commits count and this program will not use Git to look for the real commits count (faster).\r\n");

			Console.WriteLine("-R");
			Console.WriteLine("This will revert all changes in the specified target files (same as GIT checkout function). This will not apply commits count to the target files.\r\n");

			Console.WriteLine("-O filename");
			Console.WriteLine("Creates a bath file, which sets REVISIONNUMBER environment variable to the GIT revision number and REVISIONHASH environment variable to the GIT revision short hash.\r\n");

			Console.WriteLine("Press any key to quit.");
		}

		#endregion
	}
}
