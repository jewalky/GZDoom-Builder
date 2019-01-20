
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

#endregion

namespace CodeImp.DoomBuilder.Windows
{
	public struct StatusInfo
	{
		public const string NO_SELECTION = "Nothing selected."; //mxd
		public const string LOADING_TEXT = "Loading resources...";
		public const string READY_TEXT = "Ready.";
		
		public readonly StatusType type;
		public readonly string message;
		public readonly string selectioninfo; //mxd
		internal bool displayed;
		
		internal StatusInfo(StatusType type, string message)
		{
			this.type = type;

			switch(type)
			{
				case StatusType.Selection:
					this.selectioninfo = (string.IsNullOrEmpty(message) ? NO_SELECTION : message);
					this.message = General.MainWindow.Status.message;
					break;

				case StatusType.Ready:
					bool mapopened = (General.Map != null) && (General.Map.Data != null);
					bool mapisloading = mapopened && General.Map.Data.IsLoading;
					this.selectioninfo = ((string.IsNullOrEmpty(message) && mapopened) ? (string.IsNullOrEmpty(General.MainWindow.Status.selectioninfo) ? NO_SELECTION : General.MainWindow.Status.selectioninfo) : message);
					this.message = (mapisloading ? LOADING_TEXT : (mapopened ? string.Empty : READY_TEXT));
					break;

				default:
					this.selectioninfo = (string.IsNullOrEmpty(message) ? NO_SELECTION : General.MainWindow.Status.selectioninfo);
					this.message = message;
					break;
			}

			this.displayed = false;
		}

		//mxd
		public override string ToString()
		{
			if(string.IsNullOrEmpty(selectioninfo)) return message;
			if(string.IsNullOrEmpty(message)) return selectioninfo;
			return selectioninfo + " " + message;
		}
	}
	
	public enum StatusType
	{
		/// <summary>
		/// When no particular information is to be displayed. The messages displayed depends on running background processes.
		/// </summary>
		Ready,

		/// <summary>
		/// mxd. Displays information about current selection.
		/// </summary>
		Selection,
		
		/// <summary>
		/// Shows action information and flashes up the status icon once.
		/// </summary>
		Action,
		
		/// <summary>
		/// Shows information without flashing the icon.
		/// </summary>
		Info,
		
		/// <summary>
		/// Shows information with the busy icon.
		/// </summary>
		Busy,
		
		/// <summary>
		/// Shows a warning, makes a warning sound and flashes a warning icon.
		/// </summary>
		Warning
	}

	//mxd. StatusInfo used by Script Editor
	public struct ScriptStatusInfo
	{
		public const string READY_TEXT = "Ready.";

		public readonly ScriptStatusType type;
		public readonly string message;
		internal bool displayed;

		internal ScriptStatusInfo(ScriptStatusType type, string message)
		{
			this.type = type;

			switch(type)
			{
				case ScriptStatusType.Ready:
					this.message = READY_TEXT;
					break;

				case ScriptStatusType.Info:
				case ScriptStatusType.Warning:
				case ScriptStatusType.Busy:
					this.message = message;
					break;

				default:
					throw new NotImplementedException("Unsupported Script Status Type!");
			}

			this.displayed = false;
		}
	}

	//mxd. StatusType used by Script Editor
	public enum ScriptStatusType
	{
		/// <summary>
		/// When no particular information is to be displayed.
		/// </summary>
		Ready,

		/// <summary>
		/// Shows information without flashing the icon.
		/// </summary>
		Info,

		/// <summary>
		/// Shows information with the busy icon.
		/// </summary>
		Busy,

		/// <summary>
		/// Shows a warning, makes a warning sound and flashes a warning icon.
		/// </summary>
		Warning
	}
}
