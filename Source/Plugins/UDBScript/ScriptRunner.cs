#region ================== Copyright (c) 2020 Boris Iwanski

/*
 * This program is free software: you can redistribute it and/or modify
 *
 * it under the terms of the GNU General Public License as published by
 * 
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 * 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
 * 
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.If not, see<http://www.gnu.org/licenses/>.
 */

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Windows;
using CodeImp.DoomBuilder.UDBScript.Wrapper;
using CodeImp.DoomBuilder.UDBScript.API;
using Jint;
using Jint.Runtime;
using Jint.Runtime.Interop;
using Jint.Native;
using Jint.Native.Error;
using Esprima;

namespace CodeImp.DoomBuilder.UDBScript
{
	class ScriptRunner
	{
		#region ================== Variables

		private ScriptInfo scriptinfo;
		Engine engine;
		Stopwatch stopwatch;

		#endregion

		#region ================== Constructor

		public ScriptRunner(ScriptInfo scriptoption)
		{
			this.scriptinfo = scriptoption;
			stopwatch = new Stopwatch();
		}

		#endregion

		#region ================== Methods

		/// <summary>
		/// Stops the timer, pausing the script's runtime constraint
		/// </summary>
		public void StopTimer()
		{
			stopwatch.Stop();
		}

		/// <summary>
		/// Resumes the timer, resuming the script's runtime constraint
		/// </summary>
		public void ResumeTimer()
		{
			stopwatch.Start();
		}

		/// <summary>
		/// Shows a message box with an "OK" button
		/// </summary>
		/// <param name="s">Message to show</param>
		public void ShowMessage(string s)
		{
			stopwatch.Stop();
			MessageForm mf = new MessageForm("OK", null, s);
			DialogResult result = mf.ShowDialog();
			stopwatch.Start();

			if (result == DialogResult.Abort)
				throw new UserScriptAbortException();
		}

		/// <summary>
		/// Shows a message box with an "Yes" and "No" button
		/// </summary>
		/// <param name="s">Message to show</param>
		/// <returns>true if "Yes" was clicked, false if "No" was clicked</returns>
		public bool ShowMessageYesNo(string s)
		{
			stopwatch.Stop();
			MessageForm mf = new MessageForm("Yes", "No", s);
			DialogResult result = mf.ShowDialog();
			stopwatch.Start();

			if (result == DialogResult.Abort)
				throw new UserScriptAbortException();

			return result == DialogResult.OK ? true : false;
		}

		public JavaScriptException CreateRuntimeException(string message)
		{
			return new JavaScriptException(ErrorConstructor.CreateErrorConstructor(engine, new JsString("UDBScriptRuntimeException")), message);
		}

		/// <summary>
		/// Imports the code of all script library files in a single string
		/// </summary>
		/// <param name="engine">Scripting engine to load the code into</param>
		/// <param name="errortext">Errors that occured while loading the library code</param>
		/// <returns>true if there were no errors, false if there were errors</returns>
		private bool ImportLibraryCode(Engine engine, out string errortext)
		{
			string path = Path.Combine(General.AppPath, "UDBScript", "Libraries");
			string[] files = Directory.GetFiles(path, "*.js", SearchOption.AllDirectories);

			errortext = string.Empty;

			foreach (string file in files)
			{
				try
				{
					ParserOptions po = new ParserOptions(file.Remove(0, General.AppPath.Length));
					engine.Execute(File.ReadAllText(file), po);
				}
				catch (Esprima.ParserException e)
				{
					MessageBox.Show("There was an error while loading the library " + file + ":\n\n" + e.Message, "Script error", MessageBoxButtons.OK, MessageBoxIcon.Error);

					return false;
				}
				catch (Jint.Runtime.JavaScriptException e)
				{
					if (e.Error.Type != Jint.Runtime.Types.String)
					{
						//MessageBox.Show("There is an error in the script in line " + e.LineNumber + ":\n\n" + e.Message + "\n\n" + e.StackTrace, "Script error", MessageBoxButtons.OK, MessageBoxIcon.Error);
						UDBScriptErrorForm sef = new UDBScriptErrorForm(e.Message, e.StackTrace);
						sef.ShowDialog();
					}
					else
						General.Interface.DisplayStatus(StatusType.Warning, e.Message); // We get here if "throw" is used in a script

					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Runs the script
		/// </summary>
		public void Run()
		{
			string importlibraryerrors;
			bool abort = false;

			// Read the current script file
			string script = File.ReadAllText(scriptinfo.ScriptFile);

			// Make sure the option value gets saved if an option is currently being edited
			BuilderPlug.Me.EndOptionEdit();
			General.Interface.Focus();

			// Get the script assemblies (and the one from Builder) to make them available to the script
			//List<Assembly> assemblies = General.GetPluginAssemblies();
			//assemblies.Add(General.ThisAssembly);

			// Create the script engine
			engine = new Engine(cfg => {
				//cfg.AllowClr(assemblies.ToArray());
				cfg.Constraint(new RuntimeConstraint(stopwatch));
			});
			engine.SetValue("log", new Action<object>(Console.WriteLine));
			engine.SetValue("showMessage", new Action<string>(ShowMessage));
			engine.SetValue("showMessageYesNo", new Func<string, bool>(ShowMessageYesNo));
			engine.SetValue("QueryOptions", TypeReference.CreateTypeReference(engine, typeof(QueryOptions)));
			engine.SetValue("ScriptOptions", scriptinfo.GetScriptOptionsObject());
			engine.SetValue("Map", new MapWrapper());
			engine.SetValue("GameConfiguration", new GameConfigurationWrapper());
			engine.SetValue("Angle2D", TypeReference.CreateTypeReference(engine, typeof(Angle2DWrapper)));
			engine.SetValue("Vector3D", TypeReference.CreateTypeReference(engine, typeof(Vector3DWrapper)));
			engine.SetValue("Vector2D", TypeReference.CreateTypeReference(engine, typeof(Vector2DWrapper)));
			engine.SetValue("UniValue", TypeReference.CreateTypeReference(engine, typeof(UniValue)));
			engine.SetValue("Data", TypeReference.CreateTypeReference(engine, typeof(DataWrapper)));

			// We'll always need to import the UDB namespace anyway, so do it here instead in every single script
			//engine.Execute("var UDB = importNamespace('CodeImp.DoomBuilder');");

			// Import all library files into the current engine
			if(ImportLibraryCode(engine, out importlibraryerrors) == false)
				return;

			// Tell the mode that a script is about to be run
			General.Editing.Mode.OnScriptRunBegin();

			// Run the script file
			try
			{
				General.Map.UndoRedo.CreateUndo("Run script " + scriptinfo.Name);

				ParserOptions po = new ParserOptions(scriptinfo.ScriptFile.Remove(0, General.AppPath.Length));

				stopwatch.Start();
				engine.Execute(script, po);
				stopwatch.Stop();
			}
			catch (UserScriptAbortException)
			{
				General.Interface.DisplayStatus(StatusType.Warning, "Script aborted");
				abort = true;
			}
			catch (ParserException e)
			{
				MessageBox.Show("There is an error while parsing the script:\n\n" + e.Message, "Script error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				abort = true;
			}
			catch (Jint.Runtime.JavaScriptException e)
			{
				if (e.Error.Type != Jint.Runtime.Types.String)
				{
					//MessageBox.Show("There is an error in the script in line " + e.LineNumber + ":\n\n" + e.Message + "\n\n" + e.StackTrace, "Script error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					UDBScriptErrorForm sef = new UDBScriptErrorForm(e.Message, e.StackTrace);
					sef.ShowDialog();
				}
				else
					General.Interface.DisplayStatus(StatusType.Warning, e.Message); // We get here if "throw" is used in a script

				abort = true;
			}
			catch(Exception e) // Catch anything else we didn't think about
			{
				UDBScriptErrorForm sef = new UDBScriptErrorForm(e.Message, e.StackTrace);
				sef.ShowDialog();

				abort = true;
			}

			if (abort)
			{
				General.Map.UndoRedo.WithdrawUndo();
			}

			// Do some updates
			General.Map.Map.Update();
			General.Map.ThingsFilter.Update();
			//General.Interface.RedrawDisplay();

			// Tell the mode that running the script ended
			General.Editing.Mode.OnScriptRunEnd();
		}

		#endregion
	}
}
