
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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Data;
using SlimDX;

#endregion

namespace CodeImp.DoomBuilder.Controls
{
	/// <summary>
	/// Abstract control that provides a list of images.
	/// </summary>
	public abstract partial class ImageSelectorControl : UserControl
	{
		#region ================== Variables

		public event EventHandler OnValueChanged; //mxd
		
		private MouseButtons button;
		private ImageData image; //mxd
		private string previousimagename; //mxd
		protected bool multipletextures; //mxd
		
		#endregion

		#region ================== Properties
		
		public string TextureName { get { return name.Text; } set { name.Text = value; } }

		[Browsable(false)]
		public bool MultipleTextures { get { return multipletextures; } set { multipletextures = value; } }

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		protected ImageSelectorControl()
		{
			// Initialize
			InitializeComponent();
		}
		
		// Setup
		public virtual void Initialize()
		{
			// set the max length of texture names
			name.MaxLength = General.Map.Config.MaxTextureNameLength;
			if(!General.Map.Options.UseLongTextureNames) this.name.CharacterCasing = CharacterCasing.Upper; //mxd
			labelSize.BackColor = Color.FromArgb(196, labelSize.BackColor);
		}
		
		#endregion

		#region ================== Events

		// When resized
		private void ImageSelectorControl_Resize(object sender, EventArgs e)
		{
			// Fixed size
			preview.Width = this.ClientSize.Width;
			preview.Height = this.ClientSize.Height - name.Height - 4;
			name.Width = this.ClientSize.Width;
			name.Top = this.ClientSize.Height - name.Height;
			togglefullname.Left = preview.Right - togglefullname.Width - 1; //mxd
			togglefullname.Top = preview.Bottom - togglefullname.Height - 1; //mxd
		}
		
		// Layout change
		private void ImageSelectorControl_Layout(object sender, LayoutEventArgs e)
		{
			ImageSelectorControl_Resize(sender, EventArgs.Empty);
		}
		
		// Image clicked
		private void preview_Click(object sender, EventArgs e)
		{
			imagebox.BackColor = SystemColors.Highlight;
			switch(button)
			{
				case MouseButtons.Right: name.Text = "-"; break;
				case MouseButtons.Left:
					// We need to change CharacterCasing before applying the text, so let's hack around a bit...
					string newname = BrowseImage(name.Text);
					name.CharacterCasing = (IsLongTextureName(newname) ? CharacterCasing.Normal : CharacterCasing.Upper);
					name.Text = newname;
					break;
			}
		}
		
		// Name text changed
		private void name_TextChanged(object sender, EventArgs e)
		{
			// Show it centered
			ShowPreview(FindImage(name.Text));

			// Update tooltip (mxd)
			tooltip.SetToolTip(imagebox, name.Text);
		}
		
		// Mouse pressed
		private void preview_MouseDown(object sender, MouseEventArgs e)
		{
			button = e.Button;
			if((button == MouseButtons.Left) || ((button == MouseButtons.Right)))
			{
				imagebox.BackColor = AdjustedColor(SystemColors.Highlight, 0.2f);
			}
		}

		// Mouse leaves
		private void preview_MouseLeave(object sender, EventArgs e)
		{
			imagebox.BackColor = SystemColors.AppWorkspace;
			imagebox.Highlighted = false;
		}
		
		// Mouse enters
		private void preview_MouseEnter(object sender, EventArgs e)
		{
			imagebox.BackColor = SystemColors.Highlight;
			imagebox.Highlighted = true;
		}

		//mxd
		private void timer_Tick(object sender, EventArgs e) 
		{
			Refresh();
		}

		//mxd
		private void ImageSelectorControl_EnabledChanged(object sender, EventArgs e) 
		{
			labelSize.Visible = !(!General.Settings.ShowTextureSizes || !this.Enabled || string.IsNullOrEmpty(labelSize.Text));
		}

		//mxd
		private void togglefullname_Click(object sender, EventArgs e)
		{
			// Toggle between short and full name
			if(string.Compare(name.Text, image.ShortName, StringComparison.OrdinalIgnoreCase) == 0)
			{
				name.CharacterCasing = CharacterCasing.Normal;
				name.Text = image.Name;
			}
			else
			{
				name.CharacterCasing = CharacterCasing.Upper;
				name.Text = image.ShortName;
			}

			// Update icon and tooltip
			UpdateToggleImageNameButton(image);
		}
		
		#endregion

		#region ================== Methods
		
		// This refreshes the control
		new public void Refresh()
		{
			if(General.Map == null) return;
			ShowPreview(FindImage(name.Text));
			base.Refresh();
		}

		//mxd
		public void StopUpdate()
		{
			timer.Stop();
		}
		
		// This redraws the image preview
		private void ShowPreview(Image image)
		{
			// Dispose old image
			imagebox.Image = null;
			
			if(image != null)
			{
				// Show it centered
				imagebox.Image = image;
				imagebox.Refresh();
			}

			//mxd. Dispatch event
			if(OnValueChanged != null && previousimagename != name.Text) 
			{
				previousimagename = name.Text;
				OnValueChanged(this, EventArgs.Empty);
			}
		}

		//mxd
		protected void DisplayImageSize(float width, float height)
		{
			width = Math.Abs(width);
			height = Math.Abs(height);
			labelSize.Text = (width > 0 && height > 0) ? width + "x" + height : string.Empty;
			ImageSelectorControl_EnabledChanged(this, EventArgs.Empty);
		}

		//mxd
		private bool IsLongTextureName(string imagename)
		{
			if(!General.Map.Config.UseLongTextureNames || string.IsNullOrEmpty(imagename) || imagename == "-") 
				return false;

			ImageData texture = GetImageData(imagename);
			if(texture == null || !texture.HasLongName) return false;

			return string.Compare(imagename, texture.ShortName, StringComparison.OrdinalIgnoreCase) != 0;
		}

		
		// This must determine and return the image to show
		protected abstract Image FindImage(string imagename);

		//mxd. This gets ImageData by name...
		protected abstract ImageData GetImageData(string imagename);

		// This must show the image browser and return the selected texture name
		protected abstract string BrowseImage(string imagename);

		protected void UpdateToggleImageNameButton(ImageData image)
		{
			this.image = image;
			
			// Update visibility
			if(!General.Map.Config.UseLongTextureNames || image == null || !image.HasLongName) 
			{
				togglefullname.Visible = false;
				return;
			}

			// Update icon and tooltip
			togglefullname.Visible = true;
			if(string.Compare(image.ShortName, name.Text, StringComparison.OrdinalIgnoreCase) == 0)
			{
				togglefullname.Image = Properties.Resources.Expand;
				tooltip.SetToolTip(togglefullname, "Switch to full name");
			}
			else
			{
				togglefullname.Image = Properties.Resources.Collapse;
				tooltip.SetToolTip(togglefullname, "Switch to short name");
			}
		}

		// This determines the result value
		public string GetResult(string original)
		{
			// Anyting entered?
			if(name.Text.Trim().Length > 0)
			{
				// Return the new value
				return name.Text;
			}

			// Nothing given, keep original value
			return original;
		}

		// This brightens or darkens a color
		private static Color AdjustedColor(Color c, float amount)
		{
			Color4 cc = new Color4(c);

			// Adjust color
			cc.Red = Saturate((cc.Red * (1f + amount)) + (amount * 0.5f));
			cc.Green = Saturate((cc.Green * (1f + amount)) + (amount * 0.5f));
			cc.Blue = Saturate((cc.Blue * (1f + amount)) + (amount * 0.5f));

			// Return result
			return Color.FromArgb(cc.ToArgb());
		}

		// This clamps a value between 0 and 1
		private static float Saturate(float v)
		{
			if(v < 0f) return 0f; 
			if(v > 1f) return 1f; 
			return v;
		}
		
		#endregion
	}
}
