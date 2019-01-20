namespace CodeImp.DoomBuilder.Controls.Scripting
{
	partial class FindUsagesControl
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
			this.findusagestree = new CodeImp.DoomBuilder.Controls.BufferedTreeView();
			this.contextmenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.menurepeat = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.menuremove = new System.Windows.Forms.ToolStripMenuItem();
			this.menuremoveall = new System.Windows.Forms.ToolStripMenuItem();
			this.contextmenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// findusagestree
			// 
			this.findusagestree.ContextMenuStrip = this.contextmenu;
			this.findusagestree.Dock = System.Windows.Forms.DockStyle.Fill;
			this.findusagestree.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
			this.findusagestree.HideSelection = false;
			this.findusagestree.Location = new System.Drawing.Point(0, 0);
			this.findusagestree.Name = "findusagestree";
			this.findusagestree.Size = new System.Drawing.Size(619, 150);
			this.findusagestree.TabIndex = 0;
			this.findusagestree.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.findusagestree_NodeMouseDoubleClick);
			this.findusagestree.DrawNode += new System.Windows.Forms.DrawTreeNodeEventHandler(this.findusagestree_DrawNode);
			this.findusagestree.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.findusagestree_BeforeExpand);
			this.findusagestree.BeforeCollapse += new System.Windows.Forms.TreeViewCancelEventHandler(this.findusagestree_BeforeCollapse);
			this.findusagestree.KeyUp += new System.Windows.Forms.KeyEventHandler(this.findusagestree_KeyUp);
			this.findusagestree.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.findusagestree_NodeMouseClick);
			// 
			// contextmenu
			// 
			this.contextmenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menurepeat,
            this.toolStripSeparator1,
            this.menuremove,
            this.menuremoveall});
			this.contextmenu.Name = "contextmenu";
			this.contextmenu.Size = new System.Drawing.Size(148, 76);
			this.contextmenu.Opening += new System.ComponentModel.CancelEventHandler(this.contextmenu_Opening);
			// 
			// menurepeat
			// 
			this.menurepeat.Image = global::CodeImp.DoomBuilder.Properties.Resources.Reload;
			this.menurepeat.Name = "menurepeat";
			this.menurepeat.Size = new System.Drawing.Size(147, 22);
			this.menurepeat.Text = "Repeat search";
			this.menurepeat.Click += new System.EventHandler(this.menurepeat_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(144, 6);
			// 
			// menuremove
			// 
			this.menuremove.Image = global::CodeImp.DoomBuilder.Properties.Resources.Close;
			this.menuremove.Name = "menuremove";
			this.menuremove.ShortcutKeys = System.Windows.Forms.Keys.Delete;
			this.menuremove.Size = new System.Drawing.Size(147, 22);
			this.menuremove.Text = "Delete";
			this.menuremove.Click += new System.EventHandler(this.menuremove_Click);
			// 
			// menuremoveall
			// 
			this.menuremoveall.Image = global::CodeImp.DoomBuilder.Properties.Resources.Close;
			this.menuremoveall.Name = "menuremoveall";
			this.menuremoveall.Size = new System.Drawing.Size(147, 22);
			this.menuremoveall.Text = "Delete all";
			this.menuremoveall.Click += new System.EventHandler(this.menuremoveall_Click);
			// 
			// FindUsagesControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.Controls.Add(this.findusagestree);
			this.Name = "FindUsagesControl";
			this.Size = new System.Drawing.Size(619, 150);
			this.contextmenu.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private CodeImp.DoomBuilder.Controls.BufferedTreeView findusagestree;
		private System.Windows.Forms.ContextMenuStrip contextmenu;
		private System.Windows.Forms.ToolStripMenuItem menuremove;
		private System.Windows.Forms.ToolStripMenuItem menuremoveall;
		private System.Windows.Forms.ToolStripMenuItem menurepeat;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
	}
}
