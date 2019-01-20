
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
	public class ResultLineNotSingleSided : ErrorResult
	{
		#region ================== Variables
		
		private readonly Linedef line;
		
		#endregion
		
		#region ================== Properties

		public override int Buttons { get { return 2; } }
		public override string Button1Text { get { return "Make Double-Sided"; } }
		public override string Button2Text { get { return "Remove Sidedef"; } }
		
		#endregion
		
		#region ================== Constructor / Destructor
		
		// Constructor
		public ResultLineNotSingleSided(Linedef l)
		{
			// Initialize
			line = l;
			viewobjects.Add(l);
			hidden = l.IgnoredErrorChecks.Contains(this.GetType()); //mxd
			description = "This linedef is marked as single-sided, but has both a front and a back sidedef. Click 'Make Double-Sided' button to flag the line as double-sided." +
							   " Or click 'Remove Sidedef' button to remove the sidedef on the back side (making the line really single-sided).";
		}
		
		#endregion
		
		#region ================== Methods

		// This sets if this result is displayed in ErrorCheckForm (mxd)
		internal override void Hide(bool hide) 
		{
			hidden = hide;
			Type t = this.GetType();
			if(hide) line.IgnoredErrorChecks.Add(t);
			else if(line.IgnoredErrorChecks.Contains(t)) line.IgnoredErrorChecks.Remove(t);
		}
		
		// This must return the string that is displayed in the listbox
		public override string ToString()
		{
			return "Linedef " + line.Index + " is marked single-sided but has two sides";
		}
		
		// Rendering
		public override void PlotSelection(IRenderer2D renderer)
		{
			renderer.PlotLinedef(line, General.Colors.Selection);
			renderer.PlotVertex(line.Start, ColorCollection.VERTICES);
			renderer.PlotVertex(line.End, ColorCollection.VERTICES);
		}
		
		// Fix by flipping linedefs
		public override bool Button1Click(bool batchMode)
		{
			if(!batchMode) General.Map.UndoRedo.CreateUndo("Linedef flags change");
			line.ApplySidedFlags();
			General.Map.Map.Update();
			return true;
		}
		
		// Fix by creating a sidedef
		public override bool Button2Click(bool batchMode)
		{
			if(!batchMode) General.Map.UndoRedo.CreateUndo("Remove back sidedef");
			line.Back.Dispose();
			line.ApplySidedFlags();
			General.Map.Map.Update();
			return true;
		}
		
		#endregion
	}
}
