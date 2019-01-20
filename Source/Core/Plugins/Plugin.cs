
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
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

#endregion

namespace CodeImp.DoomBuilder.Plugins
{
	internal class Plugin : IDisposable
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		// The plugin assembly
		private Assembly asm;
		
		// The plug
		private Plug plug;
		
		// Unique name used to refer to this assembly
		private readonly string name;
		
		// Disposing
		private bool isdisposed;

		#endregion

		#region ================== Properties

		public Assembly Assembly { get { return asm; } }
		public Plug Plug { get { return plug; } }
		public string Name { get { return name; } }
		public bool IsDisposed { get { return isdisposed; } }

		#endregion

		#region ================== Constructor / Disposer

		// Constructor
		public Plugin(string filename)
		{
			// Initialize
			string shortfilename = Path.GetFileName(filename);
			name = Path.GetFileNameWithoutExtension(filename);
			General.WriteLogLine("Loading plugin \"" + name + "\" from \"" + shortfilename + "\"...");

			try
			{
				// Load assembly
				asm = Assembly.LoadFrom(filename);
			}
			catch(Exception e)
			{
				General.ErrorLogger.Add(ErrorType.Error, "Could not load plugin \"" + shortfilename + "\", the DLL file could not be read. This file is not supposed to be in the Plugins subdirectory." + Environment.NewLine + Environment.NewLine + "Exception details: " + Environment.NewLine + e.ToString());
				throw new InvalidProgramException();
			}
			
			// Find the class that inherits from Plugin
			Type t = FindSingleClass(typeof(Plug));
			if(t != null)
			{
				// Are the multiple plug classes?
				if(FindClasses(typeof(Plug)).Length > 1)
				{
					// Show a warning
					General.ErrorLogger.Add(ErrorType.Warning, "Plugin \"" + shortfilename + "\" has more than one Plug class. The following class is used to create in instance: " + t.FullName);
				}
				
				// Make plug instance
				plug = CreateObject<Plug>(t);
				plug.Plugin = this;

				// Verify revision numbers
				int thisrevision = General.ThisAssembly.GetName().Version.Revision;

				//mxd. Revision numbers should match?
				if(plug.StrictRevisionMatching && plug.MinimumRevision != thisrevision)
				{
					string message = shortfilename + " plugin's assembly version (" + plug.MinimumRevision + ") doesn't match main module version (" + thisrevision + ").";
					if(General.ShowWarningMessage(message + Environment.NewLine +
												  "It's strongly recommended to update the editor." + Environment.NewLine + 
												  "Program stability is not guaranteed." + Environment.NewLine + Environment.NewLine +
					                              "Continue anyway?", MessageBoxButtons.YesNo, MessageBoxDefaultButton.Button2, false) == DialogResult.No)
					{
						General.WriteLogLine("Quiting on " + shortfilename + " module version mismatch");
						General.Exit(General.Map != null);
						return;
					}
					else
					{
						General.ErrorLogger.Add(ErrorType.Warning, message);
						throw new InvalidProgramException();
					}
				}

				// Verify minimum revision number
				if((thisrevision != 0) && (plug.MinimumRevision > thisrevision))
				{
					// Can't load this plugin because it is meant for a newer version
					General.ErrorLogger.Add(ErrorType.Error, "Could not load plugin \"" + shortfilename + "\", the Plugin is made for GZDoom Builder R" + plug.MinimumRevision + " or newer and you are running R" + thisrevision + ".");
					throw new InvalidProgramException();
				}
			}
			else
			{
				// How can we plug something in without a plug?
				General.ErrorLogger.Add(ErrorType.Error, "Could not load plugin \"" + shortfilename + "\", plugin is missing the Plug class. This file is not supposed to be in the Plugins subdirectory.");
				throw new InvalidProgramException();
			}
			
			// We have no destructor
			GC.SuppressFinalize(this);
		}

		// Disposer
		public void Dispose()
		{
			// Not already disposed?
			if(!isdisposed)
			{
				// Clean up
				plug.Dispose(); //mxd
				plug = null; //mxd
				asm = null;
				
				// Done
				isdisposed = true;
			}
		}

		#endregion

		#region ================== Methods
		
		// This creates a stream to read a resource or returns null when not found
		public Stream GetResourceStream(string resourcename)
		{
			// Find a resource
			resourcename = "." + resourcename; //mxd. Otherwise, we can get Properties.Resources.SuperCoolMode.png while searching for CoolMode.png
			string[] resnames = asm.GetManifestResourceNames();
			foreach(string rn in resnames)
			{
				// Found it?
				if(rn.EndsWith(resourcename, StringComparison.OrdinalIgnoreCase))
				{
					// Get a stream from the resource
					return asm.GetManifestResourceStream(rn);
				}
			}

			// Nothing found
			return null;
		}
		
		// This finds all class types that inherits from the given type
		public Type[] FindClasses(Type t)
		{
			List<Type> found = new List<Type>();

			// Get all exported types
			Type[] types = asm.GetExportedTypes();
			foreach(Type it in types)
			{
				// Compare types
				if(t.IsAssignableFrom(it)) found.Add(it);
			}

			// Return list
			return found.ToArray();
		}

		// This finds a single class type that inherits from the given type
		// Returns null when no valid type was found
		public Type FindSingleClass(Type t)
		{
			Type[] types = FindClasses(t);
			return (types.Length > 0 ? types[0] : null);
		}
		
		// This creates an instance of a class
		public T CreateObject<T>(Type t, params object[] args)
		{
			return CreateObjectA<T>(t, args);
		}

		// This creates an instance of a class
		public T CreateObjectA<T>(Type t, object[] args)
		{
			try
			{
				// Create instance
				return (T)asm.CreateInstance(t.FullName, false, BindingFlags.Default, null, args, CultureInfo.CurrentCulture, new object[0]);
			}
			catch(TargetInvocationException e)
			{
				// Error!
				string error = "Failed to create class instance \"" + t.Name + "\" from plugin \"" + name + "\".";
				General.ShowErrorMessage(error + Environment.NewLine + Environment.NewLine + "See the error log for more details", MessageBoxButtons.OK, false);
				General.WriteLogLine(error + " " + e.InnerException.GetType().Name + " at target: " 
					+ e.InnerException.Message + Environment.NewLine + "Stacktrace: " + e.InnerException.StackTrace.Trim());
				return default(T);
			}
			catch(Exception e)
			{
				// Error!
				string error = "Failed to create class instance \"" + t.Name + "\" from plugin \"" + name + "\".";
				General.ShowErrorMessage(error + Environment.NewLine + Environment.NewLine + "See the error log for more details", MessageBoxButtons.OK, false);
				General.WriteLogLine(error + " " + e.GetType().Name + ": " + e.Message + Environment.NewLine
					+ "Stacktrace: " + e.StackTrace.Trim());
				return default(T);
			}
		}

		#endregion
	}
}
