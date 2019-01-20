
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

using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms.Design.Behavior;
using System.Windows.Forms.Design;

#endregion

namespace CodeImp.DoomBuilder.Controls
{
	// Designer for ButtonsNumericTextbox
	internal class ButtonsNumericTextboxDesigner : ControlDesigner
	{
		public override IList SnapLines
		{
			get
			{
				IList snaplines = base.SnapLines;

				ButtonsNumericTextbox control = base.Control as ButtonsNumericTextbox;
				if(control == null) { return snaplines; }

				IDesigner designer = TypeDescriptor.CreateDesigner(control.Textbox, typeof(IDesigner));
				if(designer == null) { return snaplines; }
				designer.Initialize(control.Textbox);

				using(designer)
				{
					ControlDesigner boxdesigner = designer as ControlDesigner;
					if(boxdesigner == null) { return snaplines; }
					
					// Add the baseline and right snap lines from the textbox
					foreach(SnapLine line in boxdesigner.SnapLines)
					{
						if(line.SnapLineType == SnapLineType.Baseline)
							snaplines.Add(new SnapLine(line.SnapLineType, line.Offset + control.Textbox.Top, line.Filter, line.Priority));
						
						if(line.SnapLineType == SnapLineType.Right)
							snaplines.Add(new SnapLine(line.SnapLineType, line.Offset + control.Textbox.Left, line.Filter, line.Priority));
					}
				}

				return snaplines;
			}
		}
	}
}
