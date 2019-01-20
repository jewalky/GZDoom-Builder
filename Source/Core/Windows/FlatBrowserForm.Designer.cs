namespace CodeImp.DoomBuilder.Windows
{
	partial class FlatBrowserForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FlatBrowserForm));
			this.cancel = new System.Windows.Forms.Button();
			this.apply = new System.Windows.Forms.Button();
			this.texturesets = new System.Windows.Forms.ListView();
			this.namecolumn = new System.Windows.Forms.ColumnHeader();
			this.countcolumn = new System.Windows.Forms.ColumnHeader();
			this.smallimages = new System.Windows.Forms.ImageList(this.components);
			this.browser = new CodeImp.DoomBuilder.Controls.ImageBrowserControl();
			this.SuspendLayout();
			// 
			// cancel
			// 
			this.cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancel.Location = new System.Drawing.Point(781, 596);
			this.cancel.Name = "cancel";
			this.cancel.Size = new System.Drawing.Size(100, 25);
			this.cancel.TabIndex = 3;
			this.cancel.TabStop = false;
			this.cancel.Text = "Cancel";
			this.cancel.UseVisualStyleBackColor = true;
			this.cancel.Click += new System.EventHandler(this.cancel_Click);
			// 
			// apply
			// 
			this.apply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.apply.Location = new System.Drawing.Point(675, 596);
			this.apply.Name = "apply";
			this.apply.Size = new System.Drawing.Size(100, 25);
			this.apply.TabIndex = 2;
			this.apply.TabStop = false;
			this.apply.Text = "OK";
			this.apply.UseVisualStyleBackColor = true;
			this.apply.Click += new System.EventHandler(this.apply_Click);
			// 
			// texturesets
			// 
			this.texturesets.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)));
			this.texturesets.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.namecolumn,
            this.countcolumn});
			this.texturesets.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.texturesets.FullRowSelect = true;
			this.texturesets.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.texturesets.HideSelection = false;
			this.texturesets.Location = new System.Drawing.Point(12, 9);
			this.texturesets.MultiSelect = false;
			this.texturesets.Name = "texturesets";
			this.texturesets.Size = new System.Drawing.Size(200, 576);
			this.texturesets.SmallImageList = this.smallimages;
			this.texturesets.TabIndex = 0;
			this.texturesets.TabStop = false;
			this.texturesets.UseCompatibleStateImageBehavior = false;
			this.texturesets.View = System.Windows.Forms.View.Details;
			this.texturesets.SelectedIndexChanged += new System.EventHandler(this.texturesets_SelectedIndexChanged);
			// 
			// namecolumn
			// 
			this.namecolumn.Text = "Name";
			this.namecolumn.Width = 152;
			// 
			// countcolumn
			// 
			this.countcolumn.Text = "Count";
			this.countcolumn.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.countcolumn.Width = 42;
			// 
			// smallimages
			// 
			this.smallimages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("smallimages.ImageStream")));
			this.smallimages.TransparentColor = System.Drawing.Color.Transparent;
			this.smallimages.Images.SetKeyName(0, "KnownTextureSet2.ico");
			this.smallimages.Images.SetKeyName(1, "AllTextureSet2.ico");
			this.smallimages.Images.SetKeyName(2, "FileTextureSet.ico");
			this.smallimages.Images.SetKeyName(3, "FolderTextureSet.ico");
			this.smallimages.Images.SetKeyName(4, "PK3TextureSet.ico");
			// 
			// browser
			// 
			this.browser.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.browser.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.browser.HideInputBox = false;
			this.browser.LabelText = "Select or enter a flat name:";
			this.browser.Location = new System.Drawing.Point(218, 9);
			this.browser.Name = "browser";
			this.browser.PreventSelection = false;
			this.browser.Size = new System.Drawing.Size(663, 610);
			this.browser.TabIndex = 1;
			this.browser.TabStop = false;
			this.browser.SelectedItemDoubleClicked += new CodeImp.DoomBuilder.Controls.ImageBrowserControl.SelectedItemDoubleClickDelegate(this.browser_SelectedItemDoubleClicked);
			this.browser.SelectedItemChanged += new CodeImp.DoomBuilder.Controls.ImageBrowserControl.SelectedItemChangedDelegate(this.browser_SelectedItemChanged);
			// 
			// FlatBrowserForm
			// 
			this.AcceptButton = this.apply;
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.CancelButton = this.cancel;
			this.ClientSize = new System.Drawing.Size(893, 631);
			this.Controls.Add(this.texturesets);
			this.Controls.Add(this.cancel);
			this.Controls.Add(this.apply);
			this.Controls.Add(this.browser);
			this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.MinimizeBox = false;
			this.Name = "FlatBrowserForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Browse Flats";
			this.Load += new System.EventHandler(this.FlatBrowserForm_Load);
			this.Shown += new System.EventHandler(this.FlatBrowserForm_Shown);
			this.Activated += new System.EventHandler(this.FlatBrowserForm_Activated);
			this.Move += new System.EventHandler(this.FlatBrowserForm_Move);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FlatBrowserForm_FormClosing);
			this.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.FlatBrowserForm_HelpRequested);
			this.ResizeEnd += new System.EventHandler(this.FlatBrowserForm_ResizeEnd);
			this.ResumeLayout(false);

		}

		#endregion

		private CodeImp.DoomBuilder.Controls.ImageBrowserControl browser;
		private System.Windows.Forms.Button cancel;
		private System.Windows.Forms.Button apply;
		private System.Windows.Forms.ListView texturesets;
		private System.Windows.Forms.ColumnHeader namecolumn;
		private System.Windows.Forms.ImageList smallimages;
		private System.Windows.Forms.ColumnHeader countcolumn;
	}
}