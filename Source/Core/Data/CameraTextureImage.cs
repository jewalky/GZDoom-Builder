#region ================== Namespaces

using System;
using System.Drawing;
using System.IO;
using CodeImp.DoomBuilder.ZDoom;

#endregion

namespace CodeImp.DoomBuilder.Data
{
	public class CameraTextureImage : ImageData
	{
		#region ================== Constructor / Disposer

		// Constructor
		public CameraTextureImage(CameraTextureData data)
		{
			// Initialize
			this.UseColorCorrection = false;
			this.worldpanning = data.WorldPanning;
			SetName(data.Name);

			// Get width and height from image
			this.width = data.Width;
			this.height = data.Height;
			scale.x = data.ScaleX;
			scale.y = data.ScaleY;

			// We have no destructor
			GC.SuppressFinalize(this);
		}

		#endregion

		#region ================== Methods

		// This loads the image
		protected override void LocalLoadImage()
		{
			// Leave when already loaded
			if(this.IsImageLoaded) return;
			
			lock(this)
			{
				bitmap = new Bitmap(width, height);

				int w = Math.Max(2, Math.Min(width, height) / 24); // line width
				int o = w / 2; // line center offset
				int l = w * 3; // line length
				name = name.ToUpperInvariant();

				using(Graphics g = Graphics.FromImage(bitmap))
				{
					// Fill bg
					g.FillRectangle(Brushes.Black, 0, 0, width, height);

					// Draw corners
					Color color = General.Colors.BrightColors[General.Random(0, General.Colors.BrightColors.Length - 1)].ToColor();
					using(var pen = new Pen(color, w))
					{
						g.DrawLines(pen, new[] { new Point(l, o), new Point(o, o), new Point(o, l) }); // TL
						g.DrawLines(pen, new[] { new Point(width - l, o), new Point(width - o, o), new Point(width - o, l) }); // TR
						g.DrawLines(pen, new[] { new Point(l, height - o), new Point(o, height - o), new Point(o, height - l) }); // BL
						g.DrawLines(pen, new[] { new Point(width - l, height - o), new Point(width - o, height - o), new Point(width - o, height - l) }); // BR
					}

					// Calculate required font size
					const string rec = "\u25CFREC";
					float targetwidth = Math.Max(l * 2, 22);
					SizeF fontsize = g.MeasureString(rec, General.MainWindow.Font);
					float scaleratio = Math.Min(targetwidth / fontsize.Height, targetwidth / fontsize.Width);

					// Draw "REC" text
					using(Font font = new Font(General.MainWindow.Font.FontFamily, General.MainWindow.Font.Size * scaleratio))
					{
						using(var brush = new SolidBrush(Color.Red))
						{
							g.DrawString(rec, font, brush, new RectangleF(l / 2, l / 2 - w / 2, fontsize.Width * scaleratio, fontsize.Height * scaleratio));
						}
					}

					// Calculate required font size
					targetwidth = Math.Min(width, height);
					targetwidth -= targetwidth / 6;
					fontsize = g.MeasureString(name, General.MainWindow.Font);
					scaleratio = Math.Min(targetwidth / fontsize.Height, targetwidth / fontsize.Width);

					// Draw texture name
					using(Font font = new Font(General.MainWindow.Font.FontFamily, General.MainWindow.Font.Size * scaleratio))
					{
						using(var brush = new SolidBrush(color))
						{
							StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
							g.DrawString(name, font, brush, new RectangleF(0, 0, width, height), sf);
						}
					}
				}

				// Pass on to base
				base.LocalLoadImage();
			}
		}

		//mxd
		protected override void SetName(string name)
		{
			if(!General.Map.Config.UseLongTextureNames)
			{
				if(name.Length > DataManager.CLASIC_IMAGE_NAME_LENGTH) name = name.Substring(0, DataManager.CLASIC_IMAGE_NAME_LENGTH);
				name = name.ToUpperInvariant();
			}

			base.SetName(name);
			this.virtualname = "[CAMERA TEXTURES]" + Path.AltDirectorySeparatorChar + name;
		}

		#endregion
	}
}
