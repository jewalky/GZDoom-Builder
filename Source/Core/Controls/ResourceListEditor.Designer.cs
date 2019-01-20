namespace CodeImp.DoomBuilder.Controls
{
	partial class ResourceListEditor
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
			System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem(new string[] {
            "C:\\Windows\\Doom\\Doom2.wad"}, 3, System.Drawing.SystemColors.GrayText, System.Drawing.SystemColors.Window, null);
			System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem(new string[] {
            "C:\\My\\Little\\Textures\\"}, 2, System.Drawing.SystemColors.GrayText, System.Drawing.SystemColors.Window, null);
			System.Windows.Forms.ListViewItem listViewItem3 = new System.Windows.Forms.ListViewItem("C:\\My\\Little\\Pony.wad", 1);
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ResourceListEditor));
			this.editresource = new System.Windows.Forms.Button();
			this.deleteresources = new System.Windows.Forms.Button();
			this.addresource = new System.Windows.Forms.Button();
			this.resourceitems = new CodeImp.DoomBuilder.Controls.ResourceListView();
			this.column = new System.Windows.Forms.ColumnHeader();
			this.copypastemenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.copyresources = new System.Windows.Forms.ToolStripMenuItem();
			this.cutresources = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.pasteresources = new System.Windows.Forms.ToolStripMenuItem();
			this.replaceresources = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.removeresources = new System.Windows.Forms.ToolStripMenuItem();
			this.images = new System.Windows.Forms.ImageList(this.components);
			this.copypastemenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// editresource
			// 
			this.editresource.Enabled = false;
			this.editresource.Location = new System.Drawing.Point(122, 140);
			this.editresource.Name = "editresource";
			this.editresource.Size = new System.Drawing.Size(136, 24);
			this.editresource.TabIndex = 0;
			this.editresource.Text = "Resource options...";
			this.editresource.UseVisualStyleBackColor = true;
			this.editresource.Click += new System.EventHandler(this.editresource_Click);
			// 
			// deleteresources
			// 
			this.deleteresources.Enabled = false;
			this.deleteresources.Location = new System.Drawing.Point(259, 140);
			this.deleteresources.Name = "deleteresources";
			this.deleteresources.Size = new System.Drawing.Size(88, 24);
			this.deleteresources.TabIndex = 0;
			this.deleteresources.Text = "Remove";
			this.deleteresources.UseVisualStyleBackColor = true;
			this.deleteresources.Click += new System.EventHandler(this.deleteresources_Click);
			// 
			// addresource
			// 
			this.addresource.Location = new System.Drawing.Point(3, 140);
			this.addresource.Name = "addresource";
			this.addresource.Size = new System.Drawing.Size(118, 24);
			this.addresource.TabIndex = 0;
			this.addresource.Text = "Add resource...";
			this.addresource.UseVisualStyleBackColor = true;
			this.addresource.Click += new System.EventHandler(this.addresource_Click);
			// 
			// resourceitems
			// 
			this.resourceitems.AllowDrop = true;
			this.resourceitems.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.column});
			this.resourceitems.ContextMenuStrip = this.copypastemenu;
			this.resourceitems.FullRowSelect = true;
			this.resourceitems.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.resourceitems.HideSelection = false;
			this.resourceitems.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1,
            listViewItem2,
            listViewItem3});
			this.resourceitems.Location = new System.Drawing.Point(0, 0);
			this.resourceitems.Name = "resourceitems";
			this.resourceitems.ShowGroups = false;
			this.resourceitems.ShowItemToolTips = true;
			this.resourceitems.Size = new System.Drawing.Size(350, 138);
			this.resourceitems.SmallImageList = this.images;
			this.resourceitems.TabIndex = 0;
			this.resourceitems.UseCompatibleStateImageBehavior = false;
			this.resourceitems.View = System.Windows.Forms.View.Details;
			this.resourceitems.ClientSizeChanged += new System.EventHandler(this.resourceitems_ClientSizeChanged);
			this.resourceitems.SizeChanged += new System.EventHandler(this.resources_SizeChanged);
			this.resourceitems.DoubleClick += new System.EventHandler(this.resourceitems_DoubleClick);
			this.resourceitems.DragDrop += new System.Windows.Forms.DragEventHandler(this.resourceitems_DragDrop);
			this.resourceitems.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.resourceitems_ItemSelectionChanged);
			this.resourceitems.KeyUp += new System.Windows.Forms.KeyEventHandler(this.resourceitems_KeyUp);
			this.resourceitems.DragOver += new System.Windows.Forms.DragEventHandler(this.resourceitems_DragOver);
			// 
			// column
			// 
			this.column.Text = "Resource location";
			this.column.Width = 200;
			// 
			// copypastemenu
			// 
			this.copypastemenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyresources,
            this.cutresources,
            this.toolStripSeparator1,
            this.pasteresources,
            this.replaceresources,
            this.toolStripSeparator2,
            this.removeresources});
			this.copypastemenu.Name = "copypastemenu";
			this.copypastemenu.Size = new System.Drawing.Size(118, 126);
			this.copypastemenu.Opening += new System.ComponentModel.CancelEventHandler(this.copypastemenu_Opening);
			// 
			// copyresources
			// 
			this.copyresources.Image = global::CodeImp.DoomBuilder.Properties.Resources.Copy;
			this.copyresources.Name = "copyresources";
			this.copyresources.Size = new System.Drawing.Size(117, 22);
			this.copyresources.Text = "Copy";
			this.copyresources.Click += new System.EventHandler(this.copyresources_Click);
			// 
			// cutresources
			// 
			this.cutresources.Image = global::CodeImp.DoomBuilder.Properties.Resources.Cut;
			this.cutresources.Name = "cutresources";
			this.cutresources.Size = new System.Drawing.Size(117, 22);
			this.cutresources.Text = "Cut";
			this.cutresources.Click += new System.EventHandler(this.cutresources_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(114, 6);
			// 
			// pasteresources
			// 
			this.pasteresources.Image = global::CodeImp.DoomBuilder.Properties.Resources.Paste;
			this.pasteresources.Name = "pasteresources";
			this.pasteresources.Size = new System.Drawing.Size(117, 22);
			this.pasteresources.Text = "Paste";
			this.pasteresources.Click += new System.EventHandler(this.pasteresources_Click);
			// 
			// replaceresources
			// 
			this.replaceresources.Image = global::CodeImp.DoomBuilder.Properties.Resources.Replace;
			this.replaceresources.Name = "replaceresources";
			this.replaceresources.Size = new System.Drawing.Size(117, 22);
			this.replaceresources.Text = "Replace";
			this.replaceresources.Click += new System.EventHandler(this.replaceresources_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(114, 6);
			// 
			// removeresources
			// 
			this.removeresources.Image = global::CodeImp.DoomBuilder.Properties.Resources.SearchClear;
			this.removeresources.Name = "removeresources";
			this.removeresources.Size = new System.Drawing.Size(117, 22);
			this.removeresources.Text = "Remove";
			this.removeresources.Click += new System.EventHandler(this.removeresources_Click);
			// 
			// images
			// 
			this.images.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("images.ImageStream")));
			this.images.TransparentColor = System.Drawing.Color.Transparent;
			this.images.Images.SetKeyName(0, "Folder.ico");
			this.images.Images.SetKeyName(1, "File.ico");
			this.images.Images.SetKeyName(2, "PK3.ico");
			this.images.Images.SetKeyName(3, "FolderLocked.ico");
			this.images.Images.SetKeyName(4, "FileLocked.ico");
			this.images.Images.SetKeyName(5, "PK3Locked.ico");
			// 
			// ResourceListEditor
			// 
			this.AllowDrop = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.Controls.Add(this.resourceitems);
			this.Controls.Add(this.addresource);
			this.Controls.Add(this.editresource);
			this.Controls.Add(this.deleteresources);
			this.Name = "ResourceListEditor";
			this.Size = new System.Drawing.Size(350, 166);
			this.Resize += new System.EventHandler(this.ResourceListEditor_Resize);
			this.copypastemenu.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button deleteresources;
		private System.Windows.Forms.Button editresource;
		private System.Windows.Forms.Button addresource;
		private CodeImp.DoomBuilder.Controls.ResourceListView resourceitems;
		private System.Windows.Forms.ColumnHeader column;
		private System.Windows.Forms.ImageList images;
		private System.Windows.Forms.ContextMenuStrip copypastemenu;
		private System.Windows.Forms.ToolStripMenuItem copyresources;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem pasteresources;
		private System.Windows.Forms.ToolStripMenuItem replaceresources;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripMenuItem removeresources;
		private System.Windows.Forms.ToolStripMenuItem cutresources;
	}
}
