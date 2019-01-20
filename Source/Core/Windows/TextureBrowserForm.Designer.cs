namespace CodeImp.DoomBuilder.Windows
{
	partial class TextureBrowserForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TextureBrowserForm));
            this.cancel = new System.Windows.Forms.Button();
            this.apply = new System.Windows.Forms.Button();
            this.smallimages = new System.Windows.Forms.ImageList(this.components);
            this.tvTextureSets = new CodeImp.DoomBuilder.Controls.MultiSelectTreeview();
            this.browser = new CodeImp.DoomBuilder.Controls.ImageBrowserControl();
            this.panel = new System.Windows.Forms.Panel();
            this.splitter = new CodeImp.DoomBuilder.Controls.CollapsibleSplitContainer();
            this.panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitter)).BeginInit();
            this.splitter.Panel1.SuspendLayout();
            this.splitter.Panel2.SuspendLayout();
            this.splitter.SuspendLayout();
            this.SuspendLayout();
            // 
            // cancel
            // 
            this.cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancel.Location = new System.Drawing.Point(682, 413);
            this.cancel.Name = "cancel";
            this.cancel.Size = new System.Drawing.Size(98, 25);
            this.cancel.TabIndex = 3;
            this.cancel.TabStop = false;
            this.cancel.Text = "Cancel";
            this.cancel.UseVisualStyleBackColor = true;
            this.cancel.Click += new System.EventHandler(this.cancel_Click);
            // 
            // apply
            // 
            this.apply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.apply.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.apply.Location = new System.Drawing.Point(682, 381);
            this.apply.Name = "apply";
            this.apply.Size = new System.Drawing.Size(98, 30);
            this.apply.TabIndex = 2;
            this.apply.TabStop = false;
            this.apply.Text = "OK";
            this.apply.UseVisualStyleBackColor = true;
            this.apply.Click += new System.EventHandler(this.apply_Click);
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
            this.smallimages.Images.SetKeyName(5, "WadTextureSet.png");
            this.smallimages.Images.SetKeyName(6, "FolderImage.png");
            this.smallimages.Images.SetKeyName(7, "ArchiveImage.png");
            this.smallimages.Images.SetKeyName(8, "TextLump.png");
            // 
            // tvTextureSets
            // 
            this.tvTextureSets.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tvTextureSets.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tvTextureSets.HideSelection = false;
            this.tvTextureSets.ImageIndex = 0;
            this.tvTextureSets.ImageList = this.smallimages;
            this.tvTextureSets.Location = new System.Drawing.Point(0, 3);
            this.tvTextureSets.Name = "tvTextureSets";
            this.tvTextureSets.SelectedImageIndex = 0;
            this.tvTextureSets.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            this.tvTextureSets.SelectionMode = CodeImp.DoomBuilder.Controls.TreeViewSelectionMode.SingleSelect;
            this.tvTextureSets.Size = new System.Drawing.Size(198, 374);
            this.tvTextureSets.TabIndex = 4;
            this.tvTextureSets.TabStop = false;
            this.tvTextureSets.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.tvTextureSets_NodeMouseClick);
            this.tvTextureSets.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tvTextureSets_KeyUp);
            // 
            // browser
            // 
            this.browser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.browser.HideInputBox = false;
            this.browser.Location = new System.Drawing.Point(0, 0);
            this.browser.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.browser.Name = "browser";
            this.browser.Padding = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.browser.PreventSelection = false;
            this.browser.Size = new System.Drawing.Size(573, 442);
            this.browser.TabIndex = 1;
            this.browser.TabStop = false;
            this.browser.SelectedItemChanged += new CodeImp.DoomBuilder.Controls.ImageBrowserControl.SelectedItemChangedDelegate(this.browser_SelectedItemChanged);
            this.browser.SelectedItemDoubleClicked += new CodeImp.DoomBuilder.Controls.ImageBrowserControl.SelectedItemDoubleClickDelegate(this.browser_SelectedItemDoubleClicked);
            // 
            // panel
            // 
            this.panel.Controls.Add(this.tvTextureSets);
            this.panel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel.Location = new System.Drawing.Point(0, 0);
            this.panel.Name = "panel";
            this.panel.Size = new System.Drawing.Size(203, 442);
            this.panel.TabIndex = 6;
            // 
            // splitter
            // 
            this.splitter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitter.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitter.Location = new System.Drawing.Point(0, 0);
            this.splitter.Name = "splitter";
            // 
            // splitter.Panel1
            // 
            this.splitter.Panel1.Controls.Add(this.browser);
            // 
            // splitter.Panel2
            // 
            this.splitter.Panel2.Controls.Add(this.panel);
            this.splitter.Panel2MinSize = 100;
            this.splitter.Size = new System.Drawing.Size(784, 442);
            this.splitter.SplitterDistance = 573;
            this.splitter.SplitterWidth = 8;
            this.splitter.TabIndex = 0;
            this.splitter.TabStop = false;
            // 
            // TextureBrowserForm
            // 
            this.AcceptButton = this.apply;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.cancel;
            this.ClientSize = new System.Drawing.Size(784, 442);
            this.Controls.Add(this.apply);
            this.Controls.Add(this.cancel);
            this.Controls.Add(this.splitter);
            this.MinimizeBox = false;
            this.Name = "TextureBrowserForm";
            this.Opacity = 0D;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Browse Textures";
            this.Activated += new System.EventHandler(this.TextureBrowserForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TextureBrowserForm_FormClosing);
            this.Shown += new System.EventHandler(this.TextureBrowserForm_Shown);
            this.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.TextureBrowserForm_HelpRequested);
            this.panel.ResumeLayout(false);
            this.splitter.Panel1.ResumeLayout(false);
            this.splitter.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitter)).EndInit();
            this.splitter.ResumeLayout(false);
            this.ResumeLayout(false);

		}

		#endregion

		private CodeImp.DoomBuilder.Controls.ImageBrowserControl browser;
		private System.Windows.Forms.Button cancel;
		private System.Windows.Forms.Button apply;
		private System.Windows.Forms.ImageList smallimages;
		private CodeImp.DoomBuilder.Controls.MultiSelectTreeview tvTextureSets;
		private System.Windows.Forms.Panel panel;
		private CodeImp.DoomBuilder.Controls.CollapsibleSplitContainer splitter;
	}
}