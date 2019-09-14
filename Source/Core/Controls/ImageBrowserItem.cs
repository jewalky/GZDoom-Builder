#region ================== Namespaces

using System;
using System.Drawing;
using CodeImp.DoomBuilder.Data;
using System.Drawing.Drawing2D;
using System.Text;
#endregion

namespace CodeImp.DoomBuilder.Controls
{
	#region ================== mxd. ImageBrowserItemType

	internal enum ImageBrowserItemType
	{
		// Values order is used when sorting ImageBrowserItems!
		FOLDER_UP,
		FOLDER,
		IMAGE,
	}

	#endregion

	internal class ImageBrowserItem : IComparable<ImageBrowserItem>
	{
		#region ================== Variables

		protected ImageData icon;
		private bool imageloaded;
		private bool showfullname;
		protected ImageBrowserItemType itemtype;
		private string tooltip;

        private static Brush bgbrush, fgbrush_used, fgbrush_unused, selectedbgbrush, selectionbrush, selectiontextbrush, bgbrush_alpha;
        private static Color bgcolor;
        private static Pen selection, frame_used, frame_unused;
        private static StringBuilder size_builder;
        private static Font pixelSizeFont;

        #endregion

        #region ================== Properties

        public ImageData Icon { get { return icon; } }
		public ImageBrowserItemType ItemType { get { return itemtype; } }
		public virtual bool IsPreviewLoaded { get { return icon.IsPreviewLoaded; } }
		public bool ShowFullName { set { showfullname = value; } get { return showfullname && (!(icon is PK3FileImage) || !((PK3FileImage)icon).IsBadForLongTextureNames); } }
		public virtual string TextureName { get { return (ShowFullName ? icon.Name : icon.ShortName); } }
		public virtual int TextureNameWidth { get { return ShowFullName ? icon.NameWidth : icon.ShortNameWidth; } } // biwa
		public string ToolTip { get { return tooltip; } }

		#endregion

		#region ================== Constructor

		// Constructors
		protected ImageBrowserItem() { } //mxd. Needed for inheritance...
		public ImageBrowserItem(ImageData icon, string tooltip, bool showfullname)
		{
			// Initialize
			this.icon = icon;
			this.itemtype = ImageBrowserItemType.IMAGE; //mxd
			this.showfullname = showfullname; //mxd
			this.imageloaded = icon.IsPreviewLoaded; //mxd
			this.tooltip = tooltip; //mxd
		}

		#endregion

		#region ================== Methods

		internal bool CheckRedrawNeeded()
		{
			if(icon.IsPreviewLoaded != imageloaded)
			{
				imageloaded = icon.IsPreviewLoaded;
				return true;
			}
			return false;
		}

        internal static void SetBrushes(bool classicview, int x, int y, int w, int h)
        {
            if (size_builder == null)
            {
                size_builder = new StringBuilder(11); // 11 for 2 parens + 1 'x' + 4 * 2 numbers
            }
            // Pick colors and brushes
            if (General.Settings.BlackBrowsers)
            {
                bgcolor = Color.Black;
                bgbrush = Brushes.Black;
                fgbrush_used = Brushes.Orange;
                fgbrush_unused = Brushes.White;

                if (!classicview)
                {
                    selectedbgbrush = Brushes.Gray;
                }
                else
                {
                    // FIXME - ano - okay this is a bit off
                    // [ZZ] this is not "a bit" off. this is completely off. especially when items have different sizes.
                    //      moved to item drawing.
                }

                frame_used = Pens.Orange;
                frame_unused = Pens.Gray;
                selection = Pens.Red;
                selectionbrush = Brushes.Red;
                selectiontextbrush = Brushes.White;
            }
            else
            {
                bgcolor = SystemColors.Window;
                bgbrush = SystemBrushes.Window;
                fgbrush_used = SystemBrushes.HotTrack;
                fgbrush_unused = SystemBrushes.ControlText;

                if (!classicview)
                {
                    selectedbgbrush = SystemBrushes.Highlight;
                }
                else
                {
                    selectedbgbrush = null;
                }

                frame_used = SystemPens.HotTrack;
                frame_unused = SystemPens.ActiveBorder;
                selection = SystemPens.HotTrack;
                selectionbrush = SystemBrushes.HotTrack;
                selectiontextbrush = SystemBrushes.Window;
            }

            bgbrush_alpha = new SolidBrush(Color.FromArgb(192, bgcolor));

            if (General.Settings.TextureSizesBelow)
            {
                pixelSizeFont = new Font(SystemFonts.MessageBoxFont.FontFamily, SystemFonts.MessageBoxFont.Size * 0.8f, FontStyle.Regular);
            }
            else
            {
                pixelSizeFont = SystemFonts.MessageBoxFont;
            }
        }

		internal void Draw(Graphics g, Image bmp, int x, int y, int w, int h, bool selected, bool used, bool classicview)
		{
			if(bmp == null) return;

            int fontH = 4 + SystemFonts.MessageBoxFont.Height;
            int h2 = h;
            if (General.Settings.ShowTextureSizes && General.Settings.TextureSizesBelow && ItemType == ImageBrowserItemType.IMAGE)
                h2 -= fontH;

            var iw = bmp.Width;
			var ih = bmp.Height;

			if(iw > w && iw >= ih)
			{
				ih = (int)Math.Floor(h * (ih / (float)iw));
				iw = w;
			}
			else if(ih > h2)
			{
				iw = (int)Math.Floor(w * (iw / (float)ih));
				ih = h2;
			}

            int ix = (iw < w ? x + (w - iw) / 2 : x);
			int iy = (ih < h2 ? y + (h2 - ih) / 2 : y);

			// Item bg
			g.FillRectangle(bgbrush, x - 2, y - 2, w + 3, h + 8 + SystemFonts.MessageBoxFont.Height);

            // Selected image bg
            if (selected)
            {
                if (!classicview)
                {
                    g.FillRectangle(selectedbgbrush, x - 2, y - 2, w + 4, h + 2 + SystemFonts.MessageBoxFont.Height);
                }
                else
                {
                    // calculate selectedbgbrush here. once.
                    Color topselected = Color.FromArgb(255, 37, 67, 151);
                    Color bottomselected = Color.FromArgb(255, 1, 20, 83);
                    selectedbgbrush = new LinearGradientBrush(new Point(x - 2, y - 3), new Point(x - 2, y + h + fontH), topselected, bottomselected);

                    g.FillRectangle(selectedbgbrush, x - 13, y - 2, w + 26, h + fontH);
                }
            }

			// Image
			g.DrawImage(bmp, ix, iy, iw, ih);

			// Frame
			if(selected && !classicview)
			{
				g.DrawRectangle(selection, x - 1, y - 1, w + 1, h2 + 1);
				g.DrawRectangle(selection, x - 2, y - 2, w + 3, h2 + 3);

				// Image name bg
				g.FillRectangle(selectionbrush, x - 2, y + h2 + 2, w + 4, SystemFonts.MessageBoxFont.Height + (General.Settings.TextureSizesBelow ? fontH : 0));
			}
			else if (!classicview)
			{
                g.DrawRectangle(used ? frame_used : frame_unused , x - 1, y - 1, w + 1, h2 + 1);
			}

            // Image name
            float textureNameX = classicview ? (x + (float)w / 2 - TextureNameWidth / 2) : (x - 2);
            g.DrawString(TextureName, SystemFonts.MessageBoxFont, (selected ? selectiontextbrush : (used ? fgbrush_used : fgbrush_unused)), textureNameX, y + h2 + 1);

			// Image size
			if(General.Settings.ShowTextureSizes && icon.IsPreviewLoaded && itemtype == ImageBrowserItemType.IMAGE)
			{
                if (!General.Settings.TextureSizesBelow)
                {
                    //string imagesize = Math.Abs(icon.ScaledWidth) + "x" + Math.Abs(icon.ScaledHeight);
                    //ano - let's do this with a stringbuilder instead
                    size_builder.Length = 0;
                    size_builder.Append(Math.Abs(icon.ScaledWidth));
                    size_builder.Append('x');
                    size_builder.Append(Math.Abs(icon.ScaledHeight));

                    string imagesize = size_builder.ToString();
                    SizeF textsize = g.MeasureString(imagesize, pixelSizeFont);

                    textsize.Width += 2;
                    textsize.Height -= 3;

                    // Draw bg
                    if (selected)
                    {
                        g.FillRectangle(selectionbrush, x, y, textsize.Width, textsize.Height);
                    }
                    else
                    {
                         g.FillRectangle(bgbrush_alpha, x, y, textsize.Width, textsize.Height);
                    }

                    g.DrawString(imagesize, pixelSizeFont, (selected ? selectiontextbrush : (used ? fgbrush_used : fgbrush_unused)), x, y - 1);
                }
                else
                {
                    //string imagesize = "(" + Math.Abs(icon.ScaledWidth) + "x" + Math.Abs(icon.ScaledHeight) + ")";
                    //ano - let's do this with a stringbuilder instead
                    size_builder.Length = 0;
                    size_builder.Append("(");
                    size_builder.Append(Math.Abs(icon.ScaledWidth));
                    size_builder.Append('x');
                    size_builder.Append(Math.Abs(icon.ScaledHeight));
                    size_builder.Append(")");

                    string imagesize = size_builder.ToString();

                    // [ZZ] we can't draw it with the regular font: it blends in with the texture name.
                    SolidBrush brush = new SolidBrush(((SolidBrush)(selected ? selectiontextbrush : (used ? fgbrush_used : fgbrush_unused))).Color);
                    brush.Color = Color.FromArgb((int)(brush.Color.A/* * 0.75f*/), brush.Color.R, brush.Color.G, brush.Color.B);
                    
                    SizeF textsize = g.MeasureString(imagesize, pixelSizeFont);

                    float szx = classicview ? (x + (float)w / 2 - textsize.Width / 2) : x;
                    float szy = (float)y + h;

                    g.DrawString(imagesize, pixelSizeFont, brush, szx, szy);
                }
			}
		}

		// Comparer
		public int CompareTo(ImageBrowserItem other)
		{
			if(itemtype != other.itemtype) return ((int)itemtype).CompareTo((int)other.itemtype);
			return this.TextureName.ToUpperInvariant().CompareTo(other.TextureName.ToUpperInvariant());
		}

		#endregion
	}
}
