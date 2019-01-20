
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
using CodeImp.DoomBuilder.Geometry;
using System.Drawing;
using CodeImp.DoomBuilder.Editing;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	internal class FindReplaceType
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		protected readonly FindReplaceAttribute attribs;
		
		#endregion

		#region ================== Properties

		public FindReplaceAttribute Attributes { get { return attribs; } }
		public virtual Image BrowseImage { get { return null; } }
		public bool AllowDelete { get { return false; } }
		public virtual string UsageHint { get { return string.Empty; } } //mxd
		public virtual Presentation RenderPresentation { get { return Presentation.Standard; } }

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public FindReplaceType()
		{
			// Initialize
			object[] attrs = this.GetType().GetCustomAttributes(typeof(FindReplaceAttribute), true);
			attribs = (FindReplaceAttribute)attrs[0];
		}

		#endregion

		#region ================== Methods
		
		// This is called to test if the item should be displayed
		public virtual bool DetermineVisiblity()
		{
			return true;
		}

		// This is called to test if replacing is supported (mxd)
		public virtual bool CanReplace() 
		{
			return true;
		}
		
		// This is called when the browse button is pressed
		public virtual string Browse(string initialvalue)
		{
			return string.Empty;
		}

		//mxd. This is called when the replace browse button is pressed
		public virtual string BrowseReplace(string initialvalue)
		{
			return Browse(initialvalue);
		}
		
		// This is called to perform a search (and replace)
		// Must return a list of items to show in the results list
		// replacewith is null when not replacing
		public virtual FindReplaceObject[] Find(string value, bool withinselection, bool replace, string replacewith, bool keepselection)
		{
			return new FindReplaceObject[0];
		}

		// String representation
		public override string ToString()
		{
			return attribs.DisplayName;
		}

		// This is called when a specific object is selected from the list
		public virtual void ObjectSelected(FindReplaceObject[] selection)
		{
		}
		
		// This is called for rendering
		public virtual void PlotSelection(IRenderer2D renderer, FindReplaceObject[] selection)
		{
		}

		// This is called for rendering
		public virtual void RenderThingsSelection(IRenderer2D renderer, FindReplaceObject[] selection)
		{
		}

		// This is called for rendering
		public virtual void RenderOverlaySelection(IRenderer2D renderer, FindReplaceObject[] selection)
		{
		}
		
		// This is called when the objects are to be edited
		public virtual void EditObjects(FindReplaceObject[] selection)
		{
		}

		// This is called when the objects are to be deleted
		public virtual void DeleteObjects(FindReplaceObject[] selection)
		{
		}
		
		// Call this to zoom in on the given selection
		public virtual void ZoomToSelection(ICollection<FindReplaceObject> selection)
		{
			List<Vector2D> points = new List<Vector2D>();
			RectangleF area = MapSet.CreateEmptyArea();
			
			// Add all points to a list
			foreach(FindReplaceObject o in selection) o.AddViewPoints(points);
			
			// Make a view area from the points
			foreach(Vector2D p in points) area = MapSet.IncreaseArea(area, p);
			
			// Make the area square, using the largest side
			if(area.Width > area.Height)
			{
				float delta = area.Width - area.Height;
				area.Y -= delta * 0.5f;
				area.Height += delta;
			}
			else
			{
				float delta = area.Height - area.Width;
				area.X -= delta * 0.5f;
				area.Width += delta;
			}
			
			// Add padding
			area.Inflate(100f, 100f);
			
			// Zoom to area
			ClassicMode editmode = (General.Editing.Mode as ClassicMode);
			editmode.CenterOnArea(area, 0.6f);
		}
		
		#endregion
	}
}
