
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
using System.Windows.Forms;
using System.Collections.Generic;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	public class ResultMissingActivation : ErrorResult
	{
		#region ================== Variables

		private readonly Linedef line;

		#endregion

		#region ================== Properties

		public override int Buttons { get { return 1; } }
		public override string Button1Text { get { return "Edit Linedef"; } }

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public ResultMissingActivation(Linedef l)
		{
			// Initialize
			this.line = l;
			this.viewobjects.Add(l);
			this.hidden = l.IgnoredErrorChecks.Contains(this.GetType());
			this.description = "This linedef has an assigned action, but no way to activate it has been set.";
		}

		#endregion

		#region ================== Methods

		// This sets if this result is displayed in ErrorCheckForm (mxd)
		internal override void Hide(bool hide) 
		{
			hidden = hide;
			Type t = this.GetType();
            if (hide)
            {
                line.IgnoredErrorChecks.Add(t);
            }
            else if (line.IgnoredErrorChecks.Contains(t))
            {
                line.IgnoredErrorChecks.Remove(t);
            }
		}

		// This must return the string that is displayed in the listbox
		public override string ToString()
		{
			return "Linedef " + line.Index + " has an action with no activation";
		}

		// Rendering
		public override void PlotSelection(IRenderer2D renderer)
		{
			renderer.PlotLinedef(line, General.Colors.Selection);
			renderer.PlotVertex(line.Start, ColorCollection.VERTICES);
			renderer.PlotVertex(line.End, ColorCollection.VERTICES);
		}

		// Fix by prompting to edit the linedef
		public override bool Button1Click(bool batchMode)
		{
            if (!batchMode) General.Map.UndoRedo.CreateUndo("Edit linedef");

            if (General.Interface.ShowEditLinedefs(new List<Linedef> { line }) == DialogResult.OK)
            {
                General.Map.Map.Update();
                return true;
            }

            return false;
        }
		#endregion
	}
}
