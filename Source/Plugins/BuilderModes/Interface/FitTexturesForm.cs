#region ================== Namespaces

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Windows;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	internal struct FitTextureOptions
	{
		public float HorizontalRepeat;
		public float VerticalRepeat;
		public int PatternWidth;
		public int PatternHeight;
		public bool FitWidth;
		public bool FitHeight;
		public bool FitAcrossSurfaces;
		public bool AutoWidth;
		public bool AutoHeight;
		public Rectangle GlobalBounds;
		public Rectangle Bounds;

		//Initial texture coordinats
		public float InitialOffsetX;
		public float InitialOffsetY;
		public float ControlSideOffsetX;
		public float ControlSideOffsetY;
		public float InitialScaleX;
		public float InitialScaleY;
	}
	
	internal partial class FitTexturesForm : DelayedForm
	{
		#region ================== Event handlers

		#endregion

		#region ================== Variables

		private static Point location = Point.Empty;
		private bool blockupdate;
		private int prevhorizrepeat;
		private int prevvertrepeat;

		// Settings
		private static int horizontalrepeat = 1;
		private static int verticalrepeat = 1;
		private static bool fitacrosssurfaces = true;
		private static bool fitwidth = true;
		private static bool fitheight = true;

		//Surface stuff
		private List<SortedVisualSide> strips;

		#endregion

		#region ================== Constructor

		public FitTexturesForm() 
		{
			InitializeComponent();

			if(!location.IsEmpty)
			{
				this.StartPosition = FormStartPosition.Manual;
				this.Location = location;
			}
		}

		#endregion

		#region ================== Methods

		public bool Setup(IEnumerable<BaseVisualGeometrySidedef> sides)
		{
			// Get shapes
			strips = BuilderModesTools.SortVisualSides(sides);

			// No dice...
			if(strips.Count == 0)
			{
				General.Interface.DisplayStatus(StatusType.Warning, "Failed to setup sidedef chains...");
				this.DialogResult = DialogResult.Cancel;
				this.Close();
				return false;
			}

#if DEBUG
			//debug
			DrawDebugUV();
#endif

			// Restore settings
			blockupdate = true;

			horizrepeat.Text = horizontalrepeat.ToString();
			vertrepeat.Text = verticalrepeat.ToString();
			prevhorizrepeat = horizontalrepeat;
			prevvertrepeat = verticalrepeat;
			cbfitconnected.Checked = fitacrosssurfaces;
			cbfitconnected.Enabled = (strips.Count > 1);
			cbfitwidth.Checked = fitwidth;
			cbfitheight.Checked = fitheight;
			UpdateRepeatGroup();

			blockupdate = false;

			//trigger update
			UpdateChanges();

			return true;
		}

		private void UpdateChanges()
		{
			// Apply changes
			FitTextureOptions options = new FitTextureOptions
			                            {
											FitAcrossSurfaces = (cbfitconnected.Enabled && cbfitconnected.Checked),
											FitWidth = cbfitwidth.Checked,
											FitHeight = cbfitheight.Checked,
											PatternWidth = (int)patternwidth.GetResultFloat(0),
											PatternHeight = (int)patternheight.GetResultFloat(0),
											AutoWidth = cbautowidth.Checked,
											AutoHeight = cbautoheight.Checked,
											HorizontalRepeat = (float)horizrepeat.GetResultFloat(0),
											VerticalRepeat = (float)vertrepeat.GetResultFloat(0)
			                            };

			foreach(SortedVisualSide side in strips) side.OnTextureFit(options);
		}

		private void UpdateRepeatGroup()
		{
			// Disable whole group?
			repeatgroup.Enabled = cbfitwidth.Checked || cbfitheight.Checked;
			if(!repeatgroup.Enabled) return;

			// Update control status
			cbautowidth.Enabled = cbfitwidth.Checked;
			patternwidth.Enabled = cbfitwidth.Checked && (cbautowidth.Enabled && cbautowidth.Checked);
			labelpatternwidth.Enabled = patternwidth.Enabled;
			labelhorizrepeat.Enabled = cbfitwidth.Checked;
			horizrepeat.Enabled = cbfitwidth.Checked && !cbautowidth.Checked;
			resethoriz.Enabled = cbfitwidth.Checked && !cbautowidth.Checked;

			cbautoheight.Enabled = cbfitheight.Checked;
			patternheight.Enabled = cbfitheight.Checked && (cbautoheight.Enabled && cbautoheight.Checked);
			labelpatternheight.Enabled = patternheight.Enabled;
			labelvertrepeat.Enabled = cbfitheight.Checked;
			vertrepeat.Enabled = cbfitheight.Checked && !cbautoheight.Checked;
			resetvert.Enabled = cbfitheight.Checked && !cbautoheight.Checked;
			
			
		}

		#endregion

		#region ================== Events

		private void FitTexturesForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			location = this.Location;

			// Store settings
			if(this.DialogResult == DialogResult.OK)
			{
				horizontalrepeat = (int)horizrepeat.GetResultFloat(0);
				verticalrepeat = (int)vertrepeat.GetResultFloat(0);
				fitacrosssurfaces = cbfitwidth.Checked;
				fitwidth = cbfitwidth.Checked;
				fitheight = cbfitheight.Checked;
			}
		}

		private void resethoriz_Click(object sender, EventArgs e)
		{
			prevhorizrepeat = 1;
			horizrepeat.Text = "1";
		}

		private void resetvert_Click(object sender, EventArgs e)
		{
			prevvertrepeat = 1;
			vertrepeat.Text = "1";
		}

		private void horizrepeat_ValueChanged(object sender, EventArgs e) 
		{
			if(blockupdate) return;

			if(horizrepeat.GetResultFloat(0) == 0)
			{
				horizrepeat.Text = prevhorizrepeat > 0 ? "-1" : "1";
				return;
			}

			prevhorizrepeat = (int)horizrepeat.GetResultFloat(0);
			UpdateChanges();
		}

		private void vertrepeat_ValueChanged(object sender, EventArgs e) 
		{
			if(blockupdate) return;

			if(vertrepeat.GetResultFloat(0) == 0) 
			{
				vertrepeat.Text = prevvertrepeat > 0 ? "-1" : "1";
				return;
			}

			prevvertrepeat = (int)vertrepeat.GetResultFloat(0);
			UpdateChanges();
		}

		private void accept_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void cancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		private void cb_CheckedChanged(object sender, EventArgs e) 
		{
			if(blockupdate) return;
			UpdateRepeatGroup();
			UpdateChanges();
		}

		private void patternsize_ValueChanged(object sender, EventArgs e)
		{
			if (blockupdate) return;
			UpdateChanges();
		}

		#endregion

		#region ================== Debug

#if DEBUG
		private void DrawDebugUV()
		{
			const int margin = 20;

			//sort by texture...
			Dictionary<long, List<SortedVisualSide>> sortedstrips = new Dictionary<long, List<SortedVisualSide>>();
			foreach(SortedVisualSide side in strips)
			{
				if(!sortedstrips.ContainsKey(side.Side.Texture.LongName))
					sortedstrips.Add(side.Side.Texture.LongName, new List<SortedVisualSide>());

				sortedstrips[side.Side.Texture.LongName].Add(side);
			}

			foreach(KeyValuePair<long, List<SortedVisualSide>> pair in sortedstrips)
			{
				//find bounds
				int minx = int.MaxValue;
				int maxx = int.MinValue;
				int miny = int.MaxValue;
				int maxy = int.MinValue;

				foreach(SortedVisualSide side in pair.Value) 
				{
					if(side.Bounds.X < minx) minx = side.Bounds.X;
					if(side.Bounds.X + side.Bounds.Width > maxx) maxx = side.Bounds.X + side.Bounds.Width;
					if(side.Bounds.Y < miny) miny = side.Bounds.Y;
					if(side.Bounds.Y + side.Bounds.Height > maxy) maxy = side.Bounds.Y + side.Bounds.Height;
				}

				Bitmap bmp = new Bitmap(maxx - minx + margin * 2, maxy - miny + margin * 2);

				using(Graphics g = Graphics.FromImage(bmp)) 
				{
					int i = 0;

					foreach(SortedVisualSide side in pair.Value) 
					{
						Color c = General.Colors.BrightColors[General.Random(0, General.Colors.BrightColors.Length - 1)].ToColor();
						int x = side.Bounds.X - minx + margin;
						int y = side.Bounds.Y - miny + margin;

						using(Pen p = new Pen(c))
						{
							g.DrawRectangle(p, x, y, side.Bounds.Width, side.Bounds.Height);
						}
						using(Brush b = new SolidBrush(c))
						{
							g.DrawString(i++ + ": line " + side.Side.Sidedef.Line.Index + "; x:" + side.Bounds.X + " y:" + side.Bounds.Y, this.Font, b, x + 2, y + 2);
						}
					}
				}

				bmp.Save("testuv_" + pair.Value[0].Side.Texture.ShortName + ".png", ImageFormat.Png);
			}

			General.Interface.DisplayStatus(StatusType.Info, "Saved test image!");
		}
#endif

		#endregion


	}
}
