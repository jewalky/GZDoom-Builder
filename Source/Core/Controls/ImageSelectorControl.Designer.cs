namespace CodeImp.DoomBuilder.Controls
{
	partial class ImageSelectorControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if(disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.preview = new System.Windows.Forms.Panel();
			this.togglefullname = new System.Windows.Forms.Button();
			this.labelSize = new CodeImp.DoomBuilder.Controls.TransparentLabel();
			this.imagebox = new CodeImp.DoomBuilder.Controls.ConfigurablePictureBox();
			this.timer = new System.Windows.Forms.Timer(this.components);
			this.tooltip = new System.Windows.Forms.ToolTip(this.components);
			this.name = new CodeImp.DoomBuilder.Controls.AutoSelectTextbox();
			this.preview.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.imagebox)).BeginInit();
			this.SuspendLayout();
			// 
			// preview
			// 
			this.preview.BackColor = System.Drawing.SystemColors.AppWorkspace;
			this.preview.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			this.preview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.preview.Controls.Add(this.togglefullname);
			this.preview.Controls.Add(this.labelSize);
			this.preview.Controls.Add(this.imagebox);
			this.preview.Location = new System.Drawing.Point(0, 0);
			this.preview.Name = "preview";
			this.preview.Size = new System.Drawing.Size(68, 60);
			this.preview.TabIndex = 1;
			// 
			// togglefullname
			// 
			this.togglefullname.BackColor = System.Drawing.Color.Transparent;
			this.togglefullname.Image = global::CodeImp.DoomBuilder.Properties.Resources.Collapse;
			this.togglefullname.Location = new System.Drawing.Point(43, 35);
			this.togglefullname.Name = "togglefullname";
			this.togglefullname.Size = new System.Drawing.Size(20, 20);
			this.togglefullname.TabIndex = 3;
			this.togglefullname.UseVisualStyleBackColor = false;
			this.togglefullname.Visible = false;
			this.togglefullname.Click += new System.EventHandler(this.togglefullname_Click);
			// 
			// labelSize
			// 
			this.labelSize.AutoSize = true;
			this.labelSize.BackColor = System.Drawing.SystemColors.ControlText;
			this.labelSize.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.labelSize.ForeColor = System.Drawing.SystemColors.HighlightText;
			this.labelSize.Location = new System.Drawing.Point(1, 1);
			this.labelSize.MaximumSize = new System.Drawing.Size(0, 13);
			this.labelSize.Name = "labelSize";
			this.labelSize.Size = new System.Drawing.Size(48, 13);
			this.labelSize.TabIndex = 0;
			this.labelSize.Text = "128x128";
			this.labelSize.Visible = false;
			// 
			// imagebox
			// 
			this.imagebox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			this.imagebox.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.Default;
			this.imagebox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.imagebox.Highlighted = false;
			this.imagebox.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
			this.imagebox.Location = new System.Drawing.Point(0, 0);
			this.imagebox.Name = "imagebox";
			this.imagebox.PageUnit = System.Drawing.GraphicsUnit.Pixel;
			this.imagebox.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
			this.imagebox.Size = new System.Drawing.Size(66, 58);
			this.imagebox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.imagebox.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
			this.imagebox.TabIndex = 3;
			this.imagebox.TabStop = false;
			this.imagebox.MouseLeave += new System.EventHandler(this.preview_MouseLeave);
			this.imagebox.Click += new System.EventHandler(this.preview_Click);
			this.imagebox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.preview_MouseDown);
			this.imagebox.MouseEnter += new System.EventHandler(this.preview_MouseEnter);
			// 
			// timer
			// 
			this.timer.Tick += new System.EventHandler(this.timer_Tick);
			// 
			// name
			// 
			this.name.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
			this.name.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
			this.name.Location = new System.Drawing.Point(0, 64);
			this.name.MaxLength = 8;
			this.name.Name = "name";
			this.name.Size = new System.Drawing.Size(68, 20);
			this.name.TabIndex = 2;
			this.name.TextChanged += new System.EventHandler(this.name_TextChanged);
			// 
			// ImageSelectorControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.Controls.Add(this.name);
			this.Controls.Add(this.preview);
			this.Name = "ImageSelectorControl";
			this.Size = new System.Drawing.Size(115, 136);
			this.Layout += new System.Windows.Forms.LayoutEventHandler(this.ImageSelectorControl_Layout);
			this.Resize += new System.EventHandler(this.ImageSelectorControl_Resize);
			this.EnabledChanged += new System.EventHandler(this.ImageSelectorControl_EnabledChanged);
			this.preview.ResumeLayout(false);
			this.preview.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.imagebox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected System.Windows.Forms.Panel preview;
		protected CodeImp.DoomBuilder.Controls.AutoSelectTextbox name;
		private CodeImp.DoomBuilder.Controls.TransparentLabel labelSize;
		protected System.Windows.Forms.Timer timer;
		private System.Windows.Forms.ToolTip tooltip;
		private ConfigurablePictureBox imagebox;
		private System.Windows.Forms.Button togglefullname;

	}
}
