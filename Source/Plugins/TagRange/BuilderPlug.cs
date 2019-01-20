
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

using CodeImp.DoomBuilder.Windows;
using CodeImp.DoomBuilder.Editing;
using CodeImp.DoomBuilder.Plugins;
using CodeImp.DoomBuilder.Actions;

#endregion

namespace CodeImp.DoomBuilder.TagRange
{
	public class BuilderPlug : Plug
	{
		// Static instance. We can't use a real static class, because BuilderPlug must
		// be instantiated by the core, so we keep a static reference. (this technique
		// should be familiar to object-oriented programmers)
		private static BuilderPlug me;
		
		// Tools form
		private ToolsForm toolsform;
		
		// Static property to access the BuilderPlug
		public static BuilderPlug Me { get { return me; } }
		
		// This event is called when the plugin is initialized
		public override void OnInitialize()
		{
			base.OnInitialize();
			
			// Keep a static reference
            me = this;
			
			// Load form
            toolsform = new ToolsForm();
			
			General.Actions.BindMethods(this);
		}
		
		// This is called when the plugin is terminated
		public override void Dispose()
		{
			base.Dispose();
			
			General.Actions.UnbindMethods(this);
			
			// Dispose form
			if(toolsform != null)
				toolsform.Dispose();
			toolsform = null;
		}
		
		// This updates the button on the toolbar
		public override void OnEditEngage(EditMode oldmode, EditMode newmode)
		{
			base.OnEditEngage(oldmode, newmode);
			toolsform.UpdateButton();
		}

		[BeginAction("rangetagselection")]
		private void RangeTagSelection()
		{
			TagRangeForm f = new TagRangeForm();
			f.Setup();
			if(f.SelectionCount > 0)
				f.ShowDialog(General.Interface);
			else
				General.Interface.DisplayStatus(StatusType.Warning, "This action requires a selection!"); //mxd
			f.Dispose();
		}
	}
}
