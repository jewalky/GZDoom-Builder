#region ================== Namespaces

using System.Collections.Generic;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	//mxd. Encapsulates boring stuff
	internal class BaseFindLinedef : FindReplaceType
	{
		#region ================== Methods

		// This is called when a specific object is selected from the list
		public override void ObjectSelected(FindReplaceObject[] selection) 
		{
			if(selection.Length == 1) 
			{
				ZoomToSelection(selection);
				General.Interface.ShowLinedefInfo(selection[0].Linedef);
			} 
			else 
			{
				General.Interface.HideInfo();
			}

			General.Map.Map.ClearAllSelected();
			foreach(FindReplaceObject obj in selection) obj.Linedef.Selected = true;
		}

		// Render selection
		public override void PlotSelection(IRenderer2D renderer, FindReplaceObject[] selection) 
		{
			foreach(FindReplaceObject o in selection) 
				renderer.PlotLinedef(o.Linedef, General.Colors.Selection);
		}

		// Edit objects
		public override void EditObjects(FindReplaceObject[] selection) 
		{
			HashSet<Linedef> linedefs = new HashSet<Linedef>();
			foreach(FindReplaceObject o in selection)
				if(!linedefs.Contains(o.Linedef)) linedefs.Add(o.Linedef);
			General.Interface.ShowEditLinedefs(linedefs);
		}

		#endregion
	}
}
