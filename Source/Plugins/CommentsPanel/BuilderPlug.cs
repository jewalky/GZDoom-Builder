
#region ================== Copyright (c) 2010 Pascal vd Heiden

/*
 * Copyright (c) 2010 Pascal vd Heiden
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

using CodeImp.DoomBuilder.Controls;
using CodeImp.DoomBuilder.Editing;
using CodeImp.DoomBuilder.Plugins;
using CodeImp.DoomBuilder.Config;

#endregion

namespace CodeImp.DoomBuilder.CommentsPanel
{
	//
	// MANDATORY: The plug!
	// This is an important class to the Doom Builder core. Every plugin must
	// have exactly 1 class that inherits from Plug. When the plugin is loaded,
	// this class is instantiated and used to receive events from the core.
	// Make sure the class is public, because only public classes can be seen
	// by the core.
	//

	public class BuilderPlug : Plug
	{
		// Static instance. We can't use a real static class, because BuilderPlug must
		// be instantiated by the core, so we keep a static reference. (this technique
		// should be familiar to object-oriented programmers)
		private static BuilderPlug me;
		
		// Docker
		private CommentsDocker dockerpanel;
		private Docker commentsdocker;
		
		// Static property to access the BuilderPlug
		public static BuilderPlug Me { get { return me; } }
		
		// This event is called when the plugin is initialized
		public override void OnInitialize()
		{
			base.OnInitialize();

			// Keep a static reference
            me = this;
		}

		// When a map is created
		public override void OnMapNewEnd()
		{
			OnMapOpenEnd();
		}
		
		// This is called after a map has been successfully opened
		public override void OnMapOpenEnd()
		{
			// If we just opened a UDMF format map, we want to create the Comments panel!
			if(General.Map.Config.FormatInterface == "UniversalMapSetIO")
			{
				dockerpanel = new CommentsDocker();
				commentsdocker = new Docker("commentsdockerpanel", "Comments", dockerpanel);
				General.Interface.AddDocker(commentsdocker);
				dockerpanel.Setup();
				dockerpanel.UpdateListSoon();
			}
		}

		// Occurs before the map is closed
		public override void OnMapCloseBegin()
		{
			// If we have a Comments panel, remove it
			if(dockerpanel != null)
			{
				dockerpanel.Terminate();
				General.Interface.RemoveDocker(commentsdocker);
				commentsdocker = null;
				dockerpanel.Dispose();
				dockerpanel = null;
			}
		}

		// Geometry pasted
		public override void OnPasteEnd(PasteOptions options)
		{
			if(dockerpanel != null)
				dockerpanel.UpdateListSoon();
		}

		// Undo performed
		public override void OnUndoEnd()
		{
			if(dockerpanel != null)
				dockerpanel.UpdateListSoon();
		}

		// Redo performed
		public override void OnRedoEnd()
		{
			if(dockerpanel != null)
				dockerpanel.UpdateListSoon();
		}
		
		// Mode changes
		public override bool OnModeChange(EditMode oldmode, EditMode newmode)
		{
			if(dockerpanel != null)
				dockerpanel.UpdateListSoon();
				
			return base.OnModeChange(oldmode, newmode);
		}
    }
}
