#region ================== Namespaces

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

#endregion

namespace CodeImp.DoomBuilder.Controls
{
	public class ConfigurablePictureBox : PictureBox
	{
		#region ================== Constants

		private const int BORDER_SIZE = 4;

		#endregion

		#region ================== Variables

		private InterpolationMode interpolationmode = InterpolationMode.NearestNeighbor;
		private SmoothingMode smoothingmode = SmoothingMode.Default;
		private CompositingQuality compositingquality = CompositingQuality.Default;
		private PixelOffsetMode pixeloffsetmode = PixelOffsetMode.None;
		private GraphicsUnit pageunit = GraphicsUnit.Pixel;
		private readonly Color highlight = Color.FromArgb(196, SystemColors.Highlight);

		#endregion

		#region ================== Properties

		public InterpolationMode InterpolationMode { get { return interpolationmode; } set { interpolationmode = value; } }
		public SmoothingMode SmoothingMode { get { return smoothingmode; } set { smoothingmode = value; } }
		public CompositingQuality CompositingQuality { get { return compositingquality; } set { compositingquality = value; } }
		public PixelOffsetMode PixelOffsetMode { get { return pixeloffsetmode; } set { pixeloffsetmode = value; } }
		public GraphicsUnit PageUnit { get { return pageunit; } set { pageunit = value; } }
		public bool Highlighted { get; set; }

		#endregion

		#region ================== Events

		protected override void OnPaint(PaintEventArgs pe)
		{
			pe.Graphics.InterpolationMode = InterpolationMode;
			pe.Graphics.SmoothingMode = SmoothingMode;
			pe.Graphics.CompositingQuality = CompositingQuality;
			pe.Graphics.PageUnit = PageUnit;
			pe.Graphics.PixelOffsetMode = PixelOffsetMode;
			base.OnPaint(pe);

			if(Highlighted)
			{
				pe.Graphics.PixelOffsetMode = PixelOffsetMode.None;
				ControlPaint.DrawBorder(pe.Graphics, DisplayRectangle,
								  highlight, BORDER_SIZE, ButtonBorderStyle.Solid,
								  highlight, BORDER_SIZE, ButtonBorderStyle.Solid,
								  highlight, BORDER_SIZE, ButtonBorderStyle.Solid,
								  highlight, BORDER_SIZE, ButtonBorderStyle.Solid);
			}
		}

		#endregion
	}
}
