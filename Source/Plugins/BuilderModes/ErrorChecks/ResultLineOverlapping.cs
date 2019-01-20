
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

using System;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	public class ResultLineOverlapping : ErrorResult
	{
		#region ================== Variables
		
		private readonly Linedef line1;
		private readonly Linedef line2;
		
		#endregion
		
		#region ================== Properties

		public override int Buttons { get { return 0; } }
		
		#endregion
		
		#region ================== Constructor / Destructor
		
		// Constructor
		public ResultLineOverlapping(Linedef l1, Linedef l2)
		{
			// Initialize
			line1 = l1;
			line2 = l2;
			viewobjects.Add(l1);
			viewobjects.Add(l2);
			hidden = (l1.IgnoredErrorChecks.Contains(this.GetType()) && l2.IgnoredErrorChecks.Contains(this.GetType())); //mxd
			description = "These linedefs are overlapping and they do not reference the same sector on all sides. Overlapping lines is only allowed when they reference the same sector on all sides.";
		}
		
		#endregion
		
		#region ================== Methods

		// This sets if this result is displayed in ErrorCheckForm (mxd)
		internal override void Hide(bool hide) 
		{
			hidden = hide;
			Type t = this.GetType();
			if(hide) 
			{
				line1.IgnoredErrorChecks.Add(t);
				line2.IgnoredErrorChecks.Add(t);
			}
			else 
			{
				if(line1.IgnoredErrorChecks.Contains(t)) line1.IgnoredErrorChecks.Remove(t);
				if(line2.IgnoredErrorChecks.Contains(t)) line2.IgnoredErrorChecks.Remove(t);
			}
		}
		
		// This must return the string that is displayed in the listbox
		public override string ToString()
		{
			return "Linedefs " + line1.Index + " and " + line2.Index + " are overlapping and reference different sectors";
		}
		
		// Rendering
		public override void PlotSelection(IRenderer2D renderer)
		{
			renderer.PlotLinedef(line1, General.Colors.Selection);
			renderer.PlotLinedef(line2, General.Colors.Selection);
			renderer.PlotVertex(line1.Start, ColorCollection.VERTICES);
			renderer.PlotVertex(line1.End, ColorCollection.VERTICES);
			renderer.PlotVertex(line2.Start, ColorCollection.VERTICES);
			renderer.PlotVertex(line2.End, ColorCollection.VERTICES);
		}
		
		#endregion
	}
}
