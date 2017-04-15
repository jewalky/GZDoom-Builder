
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

using System.Collections.Generic;
using System.Windows.Forms;
using CodeImp.DoomBuilder.IO;

#endregion

namespace CodeImp.DoomBuilder.Actions
{
	public class Action
	{
		#region ================== Variables

		// Description
		private readonly string name;
		private readonly string shortname;
		private readonly string title;
		private readonly string description;
		private readonly string category;

		// Shortcut key
		private int key;
		private readonly int keymask;
		private readonly int defaultkey;
		
		// Shortcut options
		private readonly bool allowkeys;
		private readonly bool allowmouse;
		private readonly bool allowscroll;
		private readonly bool disregardshift;
		private readonly bool disregardcontrol;
		private readonly bool disregardalt; //mxd
		private readonly bool repeat;
		
		// Delegate
		private readonly List<ActionDelegate> begindelegates;
		private readonly List<ActionDelegate> enddelegates;
		
		#endregion

		#region ================== Properties

		public string Name { get { return name; } }
		public string ShortName { get { return shortname; } }
		public string Category { get { return category; } }
		public string Title { get { return title; } }
		public string Description { get { return description; } }
		public int ShortcutKey { get { return key; } }
		public int ShortcutMask { get { return keymask; } }
		public int DefaultShortcutKey { get { return defaultkey; } }
		public bool AllowKeys { get { return allowkeys; } }
		public bool AllowMouse { get { return allowmouse; } }
		public bool AllowScroll { get { return allowscroll; } }
		public bool DisregardShift { get { return disregardshift; } }
		public bool DisregardControl { get { return disregardcontrol; } }
		public bool DisregardAlt { get { return disregardalt; } } //mxd
		public bool Repeat { get { return repeat; } }
		public bool BeginBound { get { return (begindelegates.Count > 0); } }
		public bool EndBound { get { return (enddelegates.Count > 0); } }
		
		#endregion

		#region ================== Constructor / Disposer

		// Constructor
		internal Action(Configuration cfg, string name, string shortname, int key)
		{
			// Initialize
			this.name = name;
			this.shortname = shortname;
			this.title = cfg.ReadSetting(shortname + ".title", "[" + name + "]");
			this.category = cfg.ReadSetting(shortname + ".category", "");
			this.description = cfg.ReadSetting(shortname + ".description", "");
			this.allowkeys = cfg.ReadSetting(shortname + ".allowkeys", true);
			this.allowmouse = cfg.ReadSetting(shortname + ".allowmouse", true);
			this.allowscroll = cfg.ReadSetting(shortname + ".allowscroll", false);
			this.disregardshift = cfg.ReadSetting(shortname + ".disregardshift", false);
			this.disregardcontrol = cfg.ReadSetting(shortname + ".disregardcontrol", false);
			this.disregardalt = cfg.ReadSetting(shortname + ".disregardalt", false); //mxd
			this.repeat = cfg.ReadSetting(shortname + ".repeat", false);
			this.defaultkey = cfg.ReadSetting(shortname + ".default", 0);
			this.begindelegates = new List<ActionDelegate>();
			this.enddelegates = new List<ActionDelegate>();

			keymask = disregardshift ? (int)Keys.Shift : 0;
			if(disregardcontrol) keymask |= (int)Keys.Control;
			if(disregardalt) keymask |= (int)Keys.Alt; //mxd
			
			keymask = ~keymask;

			if(key == -1)
			{
				this.key = -1;
			}
			else
			{
				this.key = key & keymask;
			}
		}
		
		#endregion

		#region ================== Static Methods

		// This returns the shortcut key description for a key
		public static string GetShortcutKeyDesc(int key)
		{
			KeysConverter conv = new KeysConverter();
			string ctrlprefix = "";
			
			// When key is 0, then return an empty string
			if(key == 0) return "";

			// Split the key in Control and Button
			int ctrl = key & ((int)Keys.Control | (int)Keys.Shift | (int)Keys.Alt);
			int button = key & ~((int)Keys.Control | (int)Keys.Shift | (int)Keys.Alt);

			// When the button is a control key, then remove the control itsself
			if((button == (int)Keys.ControlKey) || (button == (int)Keys.ShiftKey) || (button == (int)Keys.Alt))
			{
				ctrl = 0;
				key = key & ~((int)Keys.Control | (int)Keys.Shift | (int)Keys.Alt);
			}
			
			//mxd. Determine control prefix
			if(ctrl != 0)
			{
				if((key & (int)Keys.Control) != 0) ctrlprefix += "Ctrl+";
				if((key & (int)Keys.Alt) != 0) ctrlprefix += "Alt+";
				if((key & (int)Keys.Shift) != 0) ctrlprefix += "Shift+";
			}
			
			// Check if button is special
			switch(button)
			{
				// Scroll down
				case (int)SpecialKeys.MScrollDown:
					
					// Make string representation
					return ctrlprefix + "ScrollDown";

				// Scroll up
				case (int)SpecialKeys.MScrollUp:

					// Make string representation
					return ctrlprefix + "ScrollUp";

                case (int)SpecialKeys.MScrollLeft:

                    //
                    return ctrlprefix + "ScrollLeft";

                case (int)SpecialKeys.MScrollRight:

                    //
                    return ctrlprefix + "ScrollRight";

				// Keys that would otherwise have odd names
				case (int)Keys.Oemtilde: return ctrlprefix + "~";
				case (int)Keys.OemMinus: return ctrlprefix + "-";
				case (int)Keys.Oemplus: return ctrlprefix + "+";
				case (int)Keys.Subtract: return ctrlprefix + "NumPad-";
				case (int)Keys.Add: return ctrlprefix + "NumPad+";
				case (int)Keys.Decimal: return ctrlprefix + "NumPad.";
				case (int)Keys.Multiply: return ctrlprefix + "NumPad*";
				case (int)Keys.Divide: return ctrlprefix + "NumPad/";
				case (int)Keys.OemOpenBrackets: return ctrlprefix + "[";
				case (int)Keys.OemCloseBrackets: return ctrlprefix + "]";
				case (int)Keys.Oem1: return ctrlprefix + ";";
				case (int)Keys.Oem7: return ctrlprefix + "'";
				case (int)Keys.Oemcomma: return ctrlprefix + ",";
				case (int)Keys.OemPeriod: return ctrlprefix + ".";
				case (int)Keys.OemQuestion: return ctrlprefix + "?";
				case (int)Keys.Oem5: return ctrlprefix + "\\";
				case (int)Keys.Capital: return ctrlprefix + "CapsLock";
				case (int)Keys.Back: return ctrlprefix + "Backspace";
				
				default:
					
					// Use standard key-string conversion
					return conv.ConvertToString(key);
			}
		}

		//mxd. This returns the shortcut key description for an action name
		public static string GetShortcutKeyDesc(string actionName) 
		{
			Action a = General.Actions.GetActionByName(actionName);
			if(a.ShortcutKey == 0) return a.Title + " (not bound to a key)";
			return GetShortcutKeyDesc(a.ShortcutKey);
		}

		#endregion

		#region ================== Methods

		// This invokes the action
		public void Invoke()
		{
			this.Begin();
			this.End();
		}
		
		// This sets a new key for the action
		internal void SetShortcutKey(int key)
		{
			// Make it so.
			this.key = key & keymask;
		}
		
		// This binds a delegate to this action
		internal void BindBegin(ActionDelegate method)
		{
			begindelegates.Add(method);
		}

		// This removes a delegate from this action
		internal void UnbindBegin(ActionDelegate method)
		{
			begindelegates.Remove(method);
		}

		// This binds a delegate to this action
		internal void BindEnd(ActionDelegate method)
		{
			enddelegates.Add(method);
		}

		// This removes a delegate from this action
		internal void UnbindEnd(ActionDelegate method)
		{
			enddelegates.Remove(method);
		}

		// This raises events for this action
		internal void Begin()
		{
			General.Plugins.OnActionBegin(this);

			// Method bound?
			if(begindelegates.Count > 0)
			{
				// Copy delegates list
				List<ActionDelegate> delegateslist = new List<ActionDelegate>(begindelegates);
				
				// Invoke all the delegates
				General.Actions.Current = this;
				General.Actions.ResetExclusiveRequest();
				foreach(ActionDelegate ad in delegateslist) ad.Invoke();
				General.Actions.ResetExclusiveRequest();
				General.Actions.Current = null;
			}
		}

		// This raises events for this action
		internal void End()
		{
			// Method bound?
			if(enddelegates.Count > 0)
			{
				// Copy delegates list
				List<ActionDelegate> delegateslist = new List<ActionDelegate>(enddelegates);

				// Invoke all the delegates
				General.Actions.Current = this;
				General.Actions.ResetExclusiveRequest();
				foreach(ActionDelegate ad in delegateslist) ad.Invoke();
				General.Actions.ResetExclusiveRequest();
				General.Actions.Current = null;
			}
			
			General.Plugins.OnActionEnd(this);
		}

		// This checks if the action qualifies for a key combination
		public bool KeyMatches(int pressedkey)
		{
			return (key == (pressedkey & keymask));
		}
		
		#endregion
	}
}
