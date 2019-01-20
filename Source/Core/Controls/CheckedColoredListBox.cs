using System;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Forms.VisualStyles;

namespace CodeImp.DoomBuilder.Controls
{
	public class CheckedColoredListBox : CheckedListBox
	{
		public Image WarningIcon { get; set; }
		
		public CheckedColoredListBox() 
		{
			this.DrawMode = DrawMode.OwnerDrawVariable;
		}

		protected override void OnMeasureItem(MeasureItemEventArgs e) 
		{
			e.ItemHeight = (int)Math.Ceiling(this.Font.Height * (e.Graphics.DpiY / 96.0f));
		}

		protected override void OnDrawItem(DrawItemEventArgs e) 
		{
			if(Items.Count == 0) return; //sanity check
			IColoredListBoxItem item = (IColoredListBoxItem)Items[e.Index];

			// Draw background
			e.DrawBackground();

			// Draw checkbox
			CheckBoxRenderer.DrawCheckBox(e.Graphics, new Point(e.Bounds.Left + 1, e.Bounds.Top + 1), CheckedIndices.Contains(e.Index) ? CheckBoxState.CheckedNormal : CheckBoxState.UncheckedNormal);

			// Draw warning icon
			if(item.ShowWarning && WarningIcon != null)
			{
				e.Graphics.DrawImage(WarningIcon, e.Bounds.Left + e.Bounds.Height + 2, e.Bounds.Top, WarningIcon.Width, WarningIcon.Height);
			}
			else // Draw color rectangle
			{
				using(SolidBrush bg = new SolidBrush(item.Color))
				{
					e.Graphics.FillRectangle(bg, e.Bounds.Left + e.Bounds.Height + 2, e.Bounds.Top + 2, e.Bounds.Height, e.Bounds.Height - 5);
				}
				using(Pen outline = new Pen(Color.Black))
				{
					e.Graphics.DrawRectangle(outline, e.Bounds.Left + e.Bounds.Height + 2, e.Bounds.Top + 2, e.Bounds.Height, e.Bounds.Height - 5);
				}
			}

			// Draw text
			int offset = e.Bounds.Left + e.Bounds.Height * 2 + 4;
			Rectangle textbounds = new Rectangle(offset, e.Bounds.Top, e.Bounds.Width - offset, e.Bounds.Height);
			e.Graphics.DrawString(item.ToString(), e.Font, new SolidBrush(e.ForeColor), textbounds.Left, textbounds.Top);

			// Draw focus rectangle
			if(e.State == DrawItemState.Focus)
				ControlPaint.DrawFocusRectangle(e.Graphics, textbounds, e.ForeColor, e.BackColor); 
		}
	}

	interface IColoredListBoxItem
	{
		bool ShowWarning { get; }
		Color Color { get; }
	}
}
