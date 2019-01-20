
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
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	[FindReplace("Vertex Index", BrowseButton = false)]
	internal class FindVertexNumber : FindReplaceType
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		#endregion

		#region ================== Properties

		#endregion

		#region ================== Constructor / Destructor

		#endregion

		#region ================== Methods

		//mxd
		public override bool CanReplace() 
		{
			return false;
		}

		// This is called to perform a search (and replace)
		// Returns a list of items to show in the results list
		// replacewith is null when not replacing
		public override FindReplaceObject[] Find(string value, bool withinselection, bool replace, string replacewith, bool keepselection)
		{
			List<FindReplaceObject> objs = new List<FindReplaceObject>();

			// Interpret the number given
			int index;
			if(int.TryParse(value, out index))
			{
				Vertex v = General.Map.Map.GetVertexByIndex(index);
				if(v != null) objs.Add(new FindReplaceObject(v, "Vertex " + index));
			}
			
			return objs.ToArray();
		}

		// This is called when a specific object is selected from the list
		public override void ObjectSelected(FindReplaceObject[] selection)
		{
			if(selection.Length == 1)
			{
				ZoomToSelection(selection);
				General.Interface.ShowVertexInfo(selection[0].Vertex);
			}
			else
				General.Interface.HideInfo();

			General.Map.Map.ClearAllSelected();
			foreach(FindReplaceObject obj in selection) obj.Vertex.Selected = true;
		}

		// Render selection
		public override void PlotSelection(IRenderer2D renderer, FindReplaceObject[] selection)
		{
			foreach(FindReplaceObject o in selection)
			{
				renderer.PlotVertex(o.Vertex, ColorCollection.SELECTION);
			}
		}

		// Edit objects
		public override void EditObjects(FindReplaceObject[] selection)
		{
			HashSet<Vertex> vertices = new HashSet<Vertex>();
			foreach(FindReplaceObject o in selection)
				if(!vertices.Contains(o.Vertex)) vertices.Add(o.Vertex);
			General.Interface.ShowEditVertices(vertices);
			General.Map.Map.Update();
		}

		#endregion
	}
}
