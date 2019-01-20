
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

using CodeImp.DoomBuilder.Geometry;

#endregion

namespace CodeImp.DoomBuilder.VisualModes
{
	public interface IVisualPickable
	{
		bool Selected { get; set; }
		bool PickFastReject(Vector3D from, Vector3D to, Vector3D dir);
		bool PickAccurate(Vector3D from, Vector3D to, Vector3D dir, ref float u_ray);
	}
}
