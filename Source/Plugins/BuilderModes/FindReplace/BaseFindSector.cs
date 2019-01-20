#region ================== Namespaces

using System.Collections.Generic;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	//mxd. Encapsulates boring stuff
	internal class BaseFindSector : FindReplaceType
	{
		#region ================== Methods

		// This is called when a specific object is selected from the list
		public override void ObjectSelected(FindReplaceObject[] selection) 
		{
			if(selection.Length == 1) 
			{
				ZoomToSelection(selection);
				General.Interface.ShowSectorInfo(selection[0].Sector);
			} 
			else 
			{
				General.Interface.HideInfo();
			}

			General.Map.Map.ClearAllSelected();
			foreach(FindReplaceObject obj in selection) obj.Sector.Selected = true;
		}

		// Render selection
		public override void PlotSelection(IRenderer2D renderer, FindReplaceObject[] selection) 
		{
			foreach(FindReplaceObject o in selection) 
			{
				foreach(Sidedef sd in o.Sector.Sidedefs)
					renderer.PlotLinedef(sd.Line, General.Colors.Selection);
			}
		}

		//mxd. Render selection highlight
		public override void RenderOverlaySelection(IRenderer2D renderer, FindReplaceObject[] selection) 
		{
			if(!General.Settings.UseHighlight) return;

			int color = General.Colors.Selection.WithAlpha(64).ToInt();
			foreach(FindReplaceObject o in selection)
				renderer.RenderHighlight(o.Sector.FlatVertices, color);
		}

		// Edit objects
		public override void EditObjects(FindReplaceObject[] selection) 
		{
			HashSet<Sector> sectors = new HashSet<Sector>();
			foreach(FindReplaceObject o in selection) 
				if(!sectors.Contains(o.Sector)) sectors.Add(o.Sector);
			General.Interface.ShowEditSectors(sectors);
		}

		#endregion
	}
}
