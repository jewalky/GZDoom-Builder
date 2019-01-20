
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
using System.Windows.Forms;
using CodeImp.DoomBuilder.Windows;
using CodeImp.DoomBuilder.Actions;
using CodeImp.DoomBuilder.Plugins;
using CodeImp.DoomBuilder.VisualModes;

#endregion

namespace CodeImp.DoomBuilder.Editing
{
	public sealed class EditingManager
	{
		#region ================== Constants
		
		#endregion
		
		#region ================== Variables
		
		// All editing mode groups, sorted alphabetically
		private List<string> groups;
		
		// All editing modes available
		private List<EditModeInfo> allmodes;
		
		// Editing modes selected through configuration
		private List<EditModeInfo> usedmodes;
		
		// Status
		private EditMode mode;
		private EditMode newmode;
		private Type prevmode;
		private Type prevstablemode;
		private Type prevclassicmode;
		private bool disengaging;

		// Disposing
		private bool isdisposed;
		
		#endregion
		
		#region ================== Properties
		
		internal List<EditModeInfo> ModesInfo { get { return allmodes; } }
		public EditMode Mode { get { return mode; } }
		public EditMode NewMode { get { return newmode; } }
		public Type PreviousMode { get { return prevmode; } }
		public Type PreviousStableMode { get { return prevstablemode; } }
		public Type PreviousClassicMode { get { return prevclassicmode; } }
		public bool IsDisposed { get { return isdisposed; } }
		
		#endregion
		
		#region ================== Constructor / Disposer
		
		// Constructor
		internal EditingManager()
		{
			// Initialize
			allmodes = new List<EditModeInfo>();
			usedmodes = new List<EditModeInfo>();
			groups = new List<string>();
			
			// Bind any methods
			General.Actions.BindMethods(this);
			
			// Make list of all editing modes we can find
			foreach(Plugin p in General.Plugins.Plugins)
			{
				// For all classes that inherit from EditMode
				Type[] editclasses = p.FindClasses(typeof(EditMode));
				foreach(Type t in editclasses)
				{
					// For all defined EditMode attributes
					EditModeAttribute[]  emattrs = (EditModeAttribute[])t.GetCustomAttributes(typeof(EditModeAttribute), false);
					foreach(EditModeAttribute a in emattrs)
					{
						// Make edit mode information
						EditModeInfo modeinfo = new EditModeInfo(p, t, a);
						allmodes.Add(modeinfo);
						
						// Add group if not added yet
						if(!groups.Contains(modeinfo.Attributes.ButtonGroup))
							groups.Add(modeinfo.Attributes.ButtonGroup);
					}
				}
			}
			
			// Sort the lists
			allmodes.Sort();
			groups.Sort();
			
			// Update modes
			UpdateCurrentEditModes();
			
			// We have no destructor
			GC.SuppressFinalize(this);
		}
		
		// Disposer
		internal void Dispose()
		{
			// Not already disposed?
			if(!isdisposed)
			{
				// Unbind any methods
				General.Actions.UnbindMethods(this);
				
				// Clean up
				foreach(EditModeInfo i in allmodes) i.Dispose(); //mxd
				
				// Done
				isdisposed = true;
			}
		}
		
		#endregion

		#region ================== Switch Actions
		
		// This unbinds all editing mode switch actions
		private void UnbindSwitchActions()
		{
			foreach(EditModeInfo emi in allmodes)
			{
				emi.UnbindSwitchAction();
			}
		}
		
		// This binds all editing mode switch actions for the available modes only
		private void BindAvailableSwitchActions()
		{
			// In case of VisualMode, we only bind the switch action
			// of the VisualMode to switch back to the previous mode
			if(mode is VisualMode)
			{
				// Bind only the switch action for this mode
				EditModeInfo info = GetEditModeInfo(mode.GetType());
				info.BindSwitchAction();
			}
			else
			{
				// Bind all available mode swtich actions
				foreach(EditModeInfo emi in usedmodes)
				{
					emi.BindSwitchAction();
				}
			}
		}
		
		#endregion
		
		#region ================== Methods
		
		// This cancels a volatile mode, as if the user presses cancel
		public bool CancelVolatileMode()
		{
			// Volatile mode?
			if((General.Map != null) & (mode != null) && mode.Attributes.Volatile && !disengaging)
			{
				// Cancel
				disengaging = true;
				General.Plugins.OnEditCancel();
				mode.OnCancel();
				return true;
			}
			else
			{
				// Mode is not volatile
				return false;
			}
		}
		
		// This disengages a volatile mode, leaving the choice to cancel or accept to the editing mode
		public bool DisengageVolatileMode()
		{
			// Volatile mode?
			if((General.Map != null) && (mode != null) && mode.Attributes.Volatile && !disengaging)
			{
				// Change back to normal mode
				disengaging = true;
				ChangeMode(prevstablemode.Name);
				return true;
			}
			else
			{
				// Mode is not volatile
				return false;
			}
		}
		
		// This returns specific editing mode info by name
		internal EditModeInfo GetEditModeInfo(string editmodename)
		{
			// Find the edit mode
			foreach(EditModeInfo emi in usedmodes)
			{
				// Mode matches class name?
				if(emi.Type.Name == editmodename) return emi;
			}
			
			// No such mode found
			return null;
		}
		
		// This returns specific editing mode info by name
		internal EditModeInfo GetEditModeInfo(Type modetype)
		{
			// Find the edit mode
			foreach(EditModeInfo emi in usedmodes)
			{
				// Mode matches class name?
				if(emi.Type == modetype) return emi;
			}
			
			// No such mode found
			return null;
		}
		
		// This is called when the editing modes must update
		internal void UpdateCurrentEditModes()
		{
			// Unbind editing mode switch actions
			UnbindSwitchActions();
			
			// Rebuild list of used modes
			usedmodes.Clear();
			if(General.Map != null)
			{
				foreach(EditModeInfo emi in allmodes)
				{
					// Include the mode if it supports current map format (mxd)
					// Also include the mode when it is listed and enabled or when it's not optional
					if( (emi.Attributes.SupportedMapFormats == null || Array.IndexOf(emi.Attributes.SupportedMapFormats, General.Map.Config.FormatInterface) != -1) &&
						((General.Map.ConfigSettings.EditModes.ContainsKey(emi.Type.FullName) && 
						General.Map.ConfigSettings.EditModes[emi.Type.FullName] ) 
						 || !emi.IsOptional) )
					{
						// Add the mode to be used and bind switch action
						usedmodes.Add(emi);
					}
				}
			}
			
			// Bind switch action for used modes
			BindAvailableSwitchActions();
			
			// Remove editing mode buttons from interface
			General.MainWindow.RemoveEditModeButtons();
			
			// Go for all the editing mode groups
			foreach(string grp in groups)
			{
				General.MainWindow.AddEditModeSeperator(grp);
				
				// Go for all used edit modes to add buttons
				foreach(EditModeInfo emi in usedmodes)
				{
					if((emi.ButtonImage != null) && (emi.ButtonDesc != null) &&
					   (emi.Attributes.ButtonGroup == grp))
						General.MainWindow.AddEditModeButton(emi);
				}
			}
		}
		
		//
		// This changes the editing mode.
		// Order in which events occur for the old and new modes:
		// 
		// - Constructor of new mode is called
		// - Disengage of old mode is called
		// ----- Mode switches -----
		// - Engage of new mode is called
		// - Dispose of old mode is called
		//
		// Returns false when cancelled
		public bool ChangeMode(EditMode nextmode)
		{
			EditMode oldmode = mode;
			
			if(nextmode != null)
			{
				// Verify that this mode is usable
				bool allowuse = false;
				foreach(EditModeInfo emi in usedmodes)
				{
					if(emi.Type.FullName == nextmode.GetType().FullName)
					{
						allowuse = true;
						break;
					}
				}

				if(!allowuse)
				{
					General.Interface.MessageBeep(MessageBeepType.Error);
					General.WriteLogLine("Attempt to switch to an invalid edit mode " + nextmode.GetType().Name + "!");
					return false;
				}
				else
				{
					General.WriteLogLine("Preparing to change editing mode to " + nextmode.GetType().Name + "...");
				}
			}
			else
			{
				General.WriteLogLine("Stopping editing mode...");
			}
			
			// Remember previous mode
			newmode = nextmode;
			if(mode != null)
			{
				prevmode = mode.GetType();
				if(!mode.Attributes.Volatile)
				{
					prevstablemode = prevmode;
					if(mode is ClassicMode) prevclassicmode = prevmode;
				}
			}
			else
			{
				prevmode = null;
				prevstablemode = null;
				prevclassicmode = null;
			}
			
			// Let the plugins know beforehand and check if not cancelled
			if(General.Plugins.ModeChanges(oldmode, newmode))
			{
				// Disenagage old mode
				disengaging = true;
				if(oldmode != null)
				{
					General.Plugins.OnEditDisengage(oldmode, newmode);
					oldmode.OnDisengage();
				}
				
				// Reset cursor
				General.Interface.SetCursor(Cursors.Default);
				
				// Apply new mode
				General.WriteLogLine("Editing mode changes from " + TypeNameOrNull(oldmode) + " to " + TypeNameOrNull(nextmode));
				General.WriteLogLine("Previous stable mode is " + TypeNameOrNull(prevstablemode) + ", previous classic mode is " + TypeNameOrNull(prevclassicmode));
				mode = newmode;
				disengaging = false;
				
				// Engage new mode
				if(newmode != null)
				{
					newmode.OnEngage();
					General.Plugins.OnEditEngage(oldmode, newmode);
				}
				
				// Bind new switch actions
				UnbindSwitchActions();
				BindAvailableSwitchActions();
				
				// Update the interface
				General.MainWindow.EditModeChanged();
				
				// Dispose old mode
				if(oldmode != null) oldmode.Dispose();
				
				// Done switching
				General.WriteLogLine("Editing mode change complete.");
				newmode = null;
				
				// Redraw the display
				General.MainWindow.RedrawDisplay();
				return true;
			}
			else
			{
				// Cancelled
				General.WriteLogLine("Editing mode change cancelled.");
				return false;
			}
		}
		
		// This changes mode by class name and optionally with arguments
		public void ChangeMode(string classname, params object[] args)
		{
			EditModeInfo emi = GetEditModeInfo(classname);
			if(emi != null) emi.SwitchToMode(args);
		}

		// This returns the type name as string
		private static string TypeNameOrNull(Type type)
		{
			return (type != null) ? type.Name : "NULL";
		}

		// This returns the type name as string
		private static string TypeNameOrNull(object obj)
		{
			return (obj != null) ? obj.GetType().Name : "NULL";
		}
		
		#endregion
		
		#region ================== Actions
		
		/// <summary>
		/// This cancels the current mode.
		/// </summary>
		[BeginAction("cancelmode")]
		public void CancelMode()
		{
			// Let the mode know
			if(mode != null)
			{
				General.Plugins.OnEditCancel();
				mode.OnCancel();
			}
		}
		
		/// <summary>
		/// This accepts the changes in the current mode.
		/// </summary>
		[BeginAction("acceptmode")]
		public void AcceptMode()
		{
			// Let the mode know
			if(mode != null)
			{
				General.Plugins.OnEditAccept();
				mode.OnAccept();
			}
		}
		
		#endregion
	}
}
