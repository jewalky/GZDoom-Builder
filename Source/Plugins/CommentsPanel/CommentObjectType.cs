
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

#endregion

namespace CodeImp.DoomBuilder.CommentsPanel
{
	public enum CommentObjectType : int
	{
		Unknown = -1,
		
		Vertex = 0,
		Linedef = 1,
		Sector = 2,
		Thing = 3,
		
		// last
		NumTypes = 4,
	}
}
