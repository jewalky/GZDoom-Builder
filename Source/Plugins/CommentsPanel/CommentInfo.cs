
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

using System.Collections.Generic;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Map;

#endregion

namespace CodeImp.DoomBuilder.CommentsPanel
{
	public class CommentInfo
	{
		private readonly string comment;
		private List<MapElement> elements;
		private DataGridViewRow row;
		
		// Properties
		public string Comment { get { return comment; } }
		public List<MapElement> Elements { get { return elements; } }
		public DataGridViewRow Row { get { return row; } set { row = value; } }
		
		// Constructor
		public CommentInfo(string comment, MapElement e)
		{
			this.comment = comment;
			this.elements = new List<MapElement>();
			this.AddElement(e);
			this.row = null;
		}
		
		// This adds an element
		public void AddElement(MapElement e)
		{
			this.elements.Add(e);
		}
		
		// This replaces the elements with those from another
		public void ReplaceElements(CommentInfo other)
		{
			this.elements = other.elements;
		}
	}
}
