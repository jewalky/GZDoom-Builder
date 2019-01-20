
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
using System.Collections.Generic;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;
using CodeImp.DoomBuilder.Geometry;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	public class ResultLineMissingFront : ErrorResult
	{
		#region ================== Variables
		
		private readonly Linedef line;
		private readonly int buttons;
		private readonly Sidedef copysidedef;
		
		#endregion
		
		#region ================== Properties

		public override int Buttons { get { return buttons; } }
		public override string Button1Text { get { return "Flip Linedef"; } }
		public override string Button2Text { get { return "Create Sidedef"; } }
		
		#endregion
		
		#region ================== Constructor / Destructor
		
		// Constructor
		public ResultLineMissingFront(Linedef l)
		{
			// Initialize
			line = l;
			viewobjects.Add(l);
			hidden = l.IgnoredErrorChecks.Contains(this.GetType()); //mxd
			description = "This linedef has a back sidedef, but is missing a front sidedef. " +
						  "A line must have at least a front side and optionally a back side! " +
						  "Click 'Flip Linedef' button if the line is supposed to be single-sided.";
			
			// One solution is to flip the sidedefs
			buttons = 1;
			
			// Check if the linedef can join a sector on the side where it is missing a sidedef
			bool fixable = false;
			List<LinedefSide> sides = Tools.FindPotentialSectorAt(l, true);
			if(sides != null)
			{
				foreach(LinedefSide sd in sides)
				{
					// If any of the sides lies along a sidedef, then we can copy
					// that sidedef to fix the missing sidedef on this line.
					if(sd.Front && (sd.Line.Front != null))
					{
						copysidedef = sd.Line.Front;
						fixable = true;
						break;
					}
					
					if(!sd.Front && (sd.Line.Back != null))
					{
						copysidedef = sd.Line.Back;
						fixable = true;
						break;
					}
				}
			}
			
			// Fixable?
			if(fixable)
			{
				buttons++;
				this.description += " Or click Create Sidedef to rebuild the missing sidedef (making the line double-sided).";
			}
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
			return "Linedef " + line.Index + " is missing front side";
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
			line.FlipVertices(); //mxd. Otherwise FlipSidedefs() will destroy the sector back side belongs to
			line.FlipSidedefs();
			General.Map.Map.Update();
			return true;
		}
		
		// Fix by creating a sidedef
		public override bool Button2Click(bool batchMode)
		{
			if(!batchMode) General.Map.UndoRedo.CreateUndo("Create front sidedef");
			Sidedef newside = General.Map.Map.CreateSidedef(line, true, copysidedef.Sector);
			if(newside == null) return false;
			copysidedef.CopyPropertiesTo(newside);
			line.ApplySidedFlags();
			General.Map.Map.Update();
			return true;
		}
		
		#endregion
	}
}
