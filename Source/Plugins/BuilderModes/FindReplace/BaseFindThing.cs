#region ================== Namespaces

using System.Collections.Generic;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	//mxd. Encapsulates boring stuff
	internal class BaseFindThing : FindReplaceType
	{
		#region ================== Methods

		// This is called when a specific object is selected from the list
		public override void ObjectSelected(FindReplaceObject[] selection) 
		{
			if(selection.Length == 1) 
			{
				ZoomToSelection(selection);
				General.Interface.ShowThingInfo(selection[0].Thing);
			} 
			else 
			{
				General.Interface.HideInfo();
			}

			General.Map.Map.ClearAllSelected();
			foreach(FindReplaceObject obj in selection) obj.Thing.Selected = true;
		}

		// Render selection
		public override void RenderThingsSelection(IRenderer2D renderer, FindReplaceObject[] selection) 
		{
			foreach(FindReplaceObject o in selection)
				renderer.RenderThing(o.Thing, General.Colors.Selection, General.Settings.ActiveThingsAlpha);
		}

		// Edit objects
		public override void EditObjects(FindReplaceObject[] selection) 
		{
			HashSet<Thing> things = new HashSet<Thing>();
			foreach(FindReplaceObject o in selection) 
				if(!things.Contains(o.Thing)) things.Add(o.Thing);
			General.Interface.ShowEditThings(things);
		}

		#endregion
	}
}
