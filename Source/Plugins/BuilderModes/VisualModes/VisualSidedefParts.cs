
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

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	internal struct VisualSidedefParts
	{
		// Members
		public readonly VisualUpper upper;
		public readonly VisualLower lower;
		public readonly VisualMiddleDouble middledouble;
		public readonly VisualMiddleSingle middlesingle;
		public readonly List<VisualMiddle3D> middle3d;
		public readonly List<VisualMiddleBack> middleback; //mxd
		public readonly VisualFogBoundary fogboundary; //mxd
		
		// Constructor
		public VisualSidedefParts(VisualUpper u, VisualLower l, VisualMiddleDouble m, VisualFogBoundary f, List<VisualMiddle3D> e, List<VisualMiddleBack> eb)
		{
			this.upper = u;
			this.lower = l;
			this.middledouble = m;
			this.middlesingle = null;
			this.fogboundary = f;
			this.middle3d = e;
			this.middleback = eb; //mxd
		}
		
		// Constructor
		public VisualSidedefParts(VisualMiddleSingle m)
		{
			this.upper = null;
			this.lower = null;
			this.middledouble = null;
			this.middlesingle = m;
			this.middle3d = null;
			this.middleback = null; //mxd
			this.fogboundary = null; //mxd
		}
		
		// This calls Setup() on all parts
		public void SetupAllParts()
		{
			if(lower != null) lower.Setup();
			if(middledouble != null) middledouble.Setup();
			if(middlesingle != null) middlesingle.Setup();
			if(fogboundary != null) fogboundary.Setup(); //mxd
			if(upper != null) upper.Setup();
			if(middle3d != null)
			{
				foreach(VisualMiddle3D m in middle3d) m.Setup();
			}
			if(middleback != null) //mxd
			{
				foreach(VisualMiddleBack m in middleback) m.Setup();
			}
		}

		//mxd
		public void DeselectAllParts() 
		{
			if(lower != null) lower.Selected = false;
			if(middledouble != null) middledouble.Selected = false;
			if(middlesingle != null) middlesingle.Selected = false;
			if(upper != null) upper.Selected = false;
			if(middle3d != null) 
			{
				foreach(VisualMiddle3D m in middle3d) m.Selected = false;
			}
			if(middleback != null) 
			{
				foreach(VisualMiddleBack m in middleback) m.Selected = false;
			}
		}
	}
}
