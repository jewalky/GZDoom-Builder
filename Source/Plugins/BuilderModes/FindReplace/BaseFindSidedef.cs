#region ================== Namespaces

using System.Collections.Generic;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	//mxd. Encapsulates boring stuff
	internal class BaseFindSidedef : FindReplaceType
	{
		#region ================== Methods

		// This is called when a specific object is selected from the list
		public override void ObjectSelected(FindReplaceObject[] selection) 
		{
			if(selection.Length == 1)
			{
				ZoomToSelection(selection);
				General.Interface.ShowLinedefInfo(selection[0].Sidedef.Line);
			}
			else
			{
				General.Interface.HideInfo();
			}

			General.Map.Map.ClearAllSelected();
			foreach(FindReplaceObject obj in selection) obj.Sidedef.Line.Selected = true;
		}

		// Render selection
		public override void PlotSelection(IRenderer2D renderer, FindReplaceObject[] selection) 
		{
			foreach(FindReplaceObject o in selection)
				renderer.PlotLinedef(o.Sidedef.Line, General.Colors.Selection);
		}

		// Edit objects
		public override void EditObjects(FindReplaceObject[] selection) 
		{
			HashSet<Linedef> linedefs = new HashSet<Linedef>();
			foreach(FindReplaceObject o in selection)
				if(!linedefs.Contains(o.Sidedef.Line)) linedefs.Add(o.Sidedef.Line);
			General.Interface.ShowEditLinedefs(linedefs);
		}

		#endregion
	}
}
