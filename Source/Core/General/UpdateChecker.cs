#region ================== Namespaces

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using SharpCompress.Archives.SevenZip;
//using SharpCompress.Common;
using SharpCompress.Readers;

#endregion

namespace CodeImp.DoomBuilder
{
	internal static class UpdateChecker
	{
		private delegate DialogResult ShowWarningMessageDelegate(string message, MessageBoxButtons buttons);
		
		private const string NO_UPDATE_REQUIRED = "Your version is up to date.";
		
		private static BackgroundWorker worker;
		private static bool verbose;

		internal static void PerformCheck(bool verbosemode)
		{
			// Update check already runing?
			if(worker != null && worker.IsBusy)
			{
				if(verbosemode) General.ShowWarningMessage("Update check is already running!", MessageBoxButtons.OK);
				return;
			}

			// Check if we have write access...
			if(!General.CheckWritePremissions(General.AppPath))
			{
				string msg = "Cannot perform update: your user account does not have write access to the destination folder \"" + General.AppPath + "\"\n" 
					+ "Move the editor to a folder with write access, or run it as Administrator.";

				if(verbosemode) General.ShowWarningMessage(msg, MessageBoxButtons.OK);
				else General.ErrorLogger.Add(ErrorType.Error, msg);
				return;
			}

			// Start checking
			verbose = verbosemode;
			worker = new BackgroundWorker();
			worker.DoWork += DoWork;
			worker.RunWorkerCompleted += RunWorkerCompleted;
			worker.WorkerSupportsCancellation = true;
			worker.RunWorkerAsync();
		}

		private static void DoWork(object sender, DoWorkEventArgs e)
		{
			string updaterpath = Path.Combine(General.AppPath, "Updater.exe");
			if(!File.Exists(updaterpath))
			{
				ShowResult("Update check failed: \"" + updaterpath + "\" does not exist!");
				e.Cancel = true;
				return;
			} 
			
			string inipath = Path.Combine(General.AppPath, "Updater.ini");
			if(!File.Exists(inipath))
			{
				ShowResult("Update check failed: \"" + inipath + "\" does not exist!");
				e.Cancel = true;
				return;
			}

			// Get some ini values...
			string url = string.Empty;
			string updaterpackname = string.Empty;
			string[] inilines = File.ReadAllLines(inipath);
			foreach(string line in inilines)
			{
				if(line.StartsWith("URL")) url = line.Substring(3).Trim();
				else if(line.StartsWith("UpdaterName")) updaterpackname = line.Substring(11).Trim();
			}

			if(string.IsNullOrEmpty(url))
			{
				ShowResult("Update check failed: failed to get update url from Updater.ini!");
				e.Cancel = true;
				return;
			}

			if(string.IsNullOrEmpty(updaterpackname))
			{
				ShowResult("Update check failed: failed to get updater pack name from Updater.ini!");
				e.Cancel = true;
				return;
			}

			// Get local revision number
			int localrev = General.ThisAssembly.GetName().Version.Revision;
			int actuallocalrev = localrev;
			if(!verbose) localrev = Math.Max(localrev, General.Settings.IgnoredRemoteRevision);

			// Get remote revision numbers
			int remoterev, remoteupdaterrev;
			MemoryStream stream = DownloadWebFile(Path.Combine(url, "Versions.txt"));
			if(stream == null)
			{
				ShowResult("Update check failed: failed to retrieve remote revision info.\nCheck your Internet connection and try again.");
				e.Cancel = true;
				return;
			}

			List<string> lines = new List<string>(2);
			using(StreamReader reader = new StreamReader(stream))
			{
				while(!reader.EndOfStream) lines.Add(reader.ReadLine());
			}

			if(lines.Count != 2)
			{
				ShowResult("Update check failed: invalid remote revision number.");
				e.Cancel = true;
				return;
			}

			if(!int.TryParse(lines[0], out remoterev))
			{
				ShowResult("Update check failed: failed to retrieve remote revision number.");
				e.Cancel = true;
				return;
			}

			if(!int.TryParse(lines[1], out remoteupdaterrev))
			{
				ShowResult("Update check failed: failed to retrieve remote updater revision number.");
				e.Cancel = true;
				return;
			}

			// Update the updater!
			string result = UpdateUpdater(url, updaterpackname, remoteupdaterrev);
			if(!string.IsNullOrEmpty(result))
			{
				ShowResult("Update check failed: " + result);
				e.Cancel = true;
				return;
			}

			if(remoterev > localrev)
			{
				// Get changelog info
				string changelog = GetChangelog(url, actuallocalrev);

				if(string.IsNullOrEmpty(changelog))
				{
					ShowResult("Update check failed: failed to retrieve changelog.\nCheck your Internet connection and try again.");
					e.Cancel = true;
					return;
				}

				// Pass data to MainForm
				General.MainWindow.UpdateAvailable(remoterev, changelog);
			}
			else if(verbose)
			{
				ShowResult(NO_UPDATE_REQUIRED);
			}
		}

		private static string UpdateUpdater(string url, string updaterpackname, int remoterev)
		{
			// Check if updater is running...
			try
			{
				Process[] processes = Process.GetProcesses();

				// Abort if it's running...
				foreach(Process process in processes)
				{
					if(process.ProcessName == "Updater" &&
					   Path.GetDirectoryName(process.MainModule.FileName) == Application.StartupPath)
					{
						return "Updater.exe is already running.";
					}
				}
			}
			catch(Exception e)
			{
				return "failed to check Updater process: " + e.Message;
			}

			// Check local revision
			int localrev = FileVersionInfo.GetVersionInfo("Updater.exe").ProductPrivatePart;
			if(localrev < remoterev)
			{
				// Download update
				MemoryStream stream = DownloadWebFile(Path.Combine(url, updaterpackname));
				if(stream == null)
				{
					return "failed to download Updater package.";
				}

				// Unpack update
				try
				{
					using(SevenZipArchive arc = SevenZipArchive.Open(stream))
					{
						if(!arc.IsComplete) return "downloaded Updater package is not complete.";
						IReader reader = arc.ExtractAllEntries();

						// Unpack all
						while(reader.MoveToNextEntry())
						{
							if(reader.Entry.IsDirectory) continue; // Shouldn't be there, but who knows...
							reader.WriteEntryToDirectory(General.AppPath, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
						}
					}
				}
				catch(Exception e)
				{
					return "failed to unpack the Updater: " + e.Message;
				}
			}

			return string.Empty;
		}

		private static void ShowResult(string message)
		{
			if(!string.IsNullOrEmpty(message))
			{
				if(verbose)
				{
					if(General.MainWindow.InvokeRequired)
						General.MainWindow.Invoke(new ShowWarningMessageDelegate(General.ShowWarningMessage), new object[] { message, MessageBoxButtons.OK });
					else
						General.ShowWarningMessage(message, MessageBoxButtons.OK);
				}
				else if(message != NO_UPDATE_REQUIRED)
				{
					General.ErrorLogger.Add(ErrorType.Error, message);
				}
			}
		}

		private static void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			worker = null;
		}

		private static string GetChangelog(string url, int localrev)
		{
			StringBuilder sb = new StringBuilder(@"{\rtf1");
			
			using(MemoryStream stream = DownloadWebFile(Path.Combine(url, "Changelog.xml")))
			{
				if(stream == null) return string.Empty;
				
				XmlDocument doc = new XmlDocument();
				doc.Load(stream);

				// Revision infos go in descending order
				if(doc.ChildNodes.Count == 0) return string.Empty;
				foreach(XmlNode log in doc.ChildNodes)
				{
					if(log.ChildNodes.Count == 0) continue;
					foreach(XmlNode logentry in log.ChildNodes)
					{
						if(logentry.Attributes == null) continue;
						var revnode = logentry.Attributes.GetNamedItem("revision");
						var comnode = logentry.Attributes.GetNamedItem("commit");
						if(revnode == null || comnode == null) continue;

						int noderev;
						if(!int.TryParse(revnode.Value, out noderev)) continue;
						if(noderev <= localrev) break;

						string commit = comnode.Value;
						string message = string.Empty;
						XmlNode msgnode = logentry["msg"];
						if(msgnode != null) message = msgnode.InnerText.Trim().Replace(Environment.NewLine, @"\par ");

						// Add info
						sb.Append(@"{\b R")
							.Append(noderev)
							.Append(" | ")
							.Append(commit)
							.Append(@":}\par ")
							.Append(message)
							.Append(@"\par\par ");
					}
				}
			}

			sb.Append("}");
			return sb.ToString();
		}

		private static MemoryStream DownloadWebFile(string url)
		{
			// Open a data stream from the supplied URL
			WebRequest request = WebRequest.Create(url);
			WebResponse response;

			try
			{
				response = request.GetResponse();
			}
			catch(WebException e)
			{
                //General.ErrorLogger.Add(ErrorType.Warning, e.ToString());
				return null;
			}
			
			Stream source = response.GetResponseStream();
			if(source == null) return null;

			// Download the data in chuncks
			byte[] buffer = new byte[1024];

			// Download the data
			MemoryStream result = new MemoryStream();
			while(!General.MainWindow.IsDisposed)
			{
				// Let's try and read the data
				int numbytes = source.Read(buffer, 0, buffer.Length);
				if(numbytes == 0) break; // Download complete

				// Write the downloaded data
				result.Write(buffer, 0, numbytes);
			}

			// Release resources
			source.Close();

			// Rewind and return the stream
			result.Position = 0;
			return result;
		}
	}
}
