namespace CodeImp.DoomBuilder.Controls
{
	partial class ScriptEditorControl
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
			this.scriptedit = new ScintillaNET.Scintilla();
			this.contextmenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.menuundo = new System.Windows.Forms.ToolStripMenuItem();
			this.menuredo = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.menucut = new System.Windows.Forms.ToolStripMenuItem();
			this.menucopy = new System.Windows.Forms.ToolStripMenuItem();
			this.menupaste = new System.Windows.Forms.ToolStripMenuItem();
			this.menudelete = new System.Windows.Forms.ToolStripMenuItem();
			this.menuduplicateline = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.menuselectall = new System.Windows.Forms.ToolStripMenuItem();
			this.menufindusages = new System.Windows.Forms.ToolStripMenuItem();
			this.scriptpanel = new System.Windows.Forms.Panel();
			this.functionbar = new System.Windows.Forms.ComboBox();
			this.contextmenu.SuspendLayout();
			this.scriptpanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// scriptedit
			// 
			this.scriptedit.AutoCIgnoreCase = true;
			this.scriptedit.AutoCMaxHeight = 12;
			this.scriptedit.AutoCOrder = ScintillaNET.Order.Custom;
			this.scriptedit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.scriptedit.CaretWidth = 2;
			this.scriptedit.ContextMenuStrip = this.contextmenu;
			this.scriptedit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.scriptedit.ExtraAscent = 1;
			this.scriptedit.ExtraDescent = 1;
			this.scriptedit.FontQuality = ScintillaNET.FontQuality.LcdOptimized;
			this.scriptedit.Location = new System.Drawing.Point(0, 0);
			this.scriptedit.Name = "scriptedit";
			this.scriptedit.ScrollWidth = 200;
			this.scriptedit.Size = new System.Drawing.Size(474, 381);
			this.scriptedit.TabIndex = 0;
			this.scriptedit.TabStop = false;
			this.scriptedit.UseTabs = true;
			this.scriptedit.WhitespaceSize = 2;
			this.scriptedit.TextChanged += new System.EventHandler(this.scriptedit_TextChanged);
			this.scriptedit.CharAdded += new System.EventHandler<ScintillaNET.CharAddedEventArgs>(this.scriptedit_CharAdded);
			this.scriptedit.AutoCCompleted += new System.EventHandler<ScintillaNET.AutoCSelectionEventArgs>(this.scriptedit_AutoCCompleted);
			this.scriptedit.InsertCheck += new System.EventHandler<ScintillaNET.InsertCheckEventArgs>(this.scriptedit_InsertCheck);
			this.scriptedit.UpdateUI += new System.EventHandler<ScintillaNET.UpdateUIEventArgs>(this.scriptedit_UpdateUI);
			// 
			// contextmenu
			// 
			this.contextmenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuundo,
            this.menuredo,
            this.toolStripSeparator1,
            this.menucut,
            this.menucopy,
            this.menupaste,
            this.menudelete,
            this.menuduplicateline,
            this.toolStripSeparator2,
            this.menuselectall,
            this.menufindusages});
			this.contextmenu.Name = "contextmenu";
			this.contextmenu.Size = new System.Drawing.Size(209, 236);
			this.contextmenu.Opening += new System.ComponentModel.CancelEventHandler(this.contextmenu_Opening);
			// 
			// menuundo
			// 
			this.menuundo.Image = global::CodeImp.DoomBuilder.Properties.Resources.Undo;
			this.menuundo.Name = "menuundo";
			this.menuundo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
			this.menuundo.Size = new System.Drawing.Size(208, 22);
			this.menuundo.Text = "Undo";
			this.menuundo.Click += new System.EventHandler(this.menuundo_Click);
			// 
			// menuredo
			// 
			this.menuredo.Image = global::CodeImp.DoomBuilder.Properties.Resources.Redo;
			this.menuredo.Name = "menuredo";
			this.menuredo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
			this.menuredo.Size = new System.Drawing.Size(208, 22);
			this.menuredo.Text = "Redo";
			this.menuredo.Click += new System.EventHandler(this.menuredo_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(205, 6);
			// 
			// menucut
			// 
			this.menucut.Image = global::CodeImp.DoomBuilder.Properties.Resources.Cut;
			this.menucut.Name = "menucut";
			this.menucut.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
			this.menucut.Size = new System.Drawing.Size(208, 22);
			this.menucut.Text = "Cut";
			this.menucut.Click += new System.EventHandler(this.menucut_Click);
			// 
			// menucopy
			// 
			this.menucopy.Image = global::CodeImp.DoomBuilder.Properties.Resources.Copy;
			this.menucopy.Name = "menucopy";
			this.menucopy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
			this.menucopy.Size = new System.Drawing.Size(208, 22);
			this.menucopy.Text = "Copy";
			this.menucopy.Click += new System.EventHandler(this.menucopy_Click);
			// 
			// menupaste
			// 
			this.menupaste.Image = global::CodeImp.DoomBuilder.Properties.Resources.Paste;
			this.menupaste.Name = "menupaste";
			this.menupaste.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
			this.menupaste.Size = new System.Drawing.Size(208, 22);
			this.menupaste.Text = "Paste";
			this.menupaste.Click += new System.EventHandler(this.menupaste_Click);
			// 
			// menudelete
			// 
			this.menudelete.Image = global::CodeImp.DoomBuilder.Properties.Resources.Close;
			this.menudelete.Name = "menudelete";
			this.menudelete.ShortcutKeyDisplayString = "Del";
			this.menudelete.Size = new System.Drawing.Size(208, 22);
			this.menudelete.Text = "Delete";
			this.menudelete.Click += new System.EventHandler(this.menudelete_Click);
			// 
			// menuduplicateline
			// 
			this.menuduplicateline.Name = "menuduplicateline";
			this.menuduplicateline.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D)));
			this.menuduplicateline.Size = new System.Drawing.Size(208, 22);
			this.menuduplicateline.Text = "Duplicate line";
			this.menuduplicateline.Click += new System.EventHandler(this.menuduplicateline_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(205, 6);
			// 
			// menuselectall
			// 
			this.menuselectall.Name = "menuselectall";
			this.menuselectall.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
			this.menuselectall.Size = new System.Drawing.Size(208, 22);
			this.menuselectall.Text = "Select all";
			this.menuselectall.Click += new System.EventHandler(this.menuselectall_Click);
			// 
			// menufindusages
			// 
			this.menufindusages.Image = global::CodeImp.DoomBuilder.Properties.Resources.Search;
			this.menufindusages.Name = "menufindusages";
			this.menufindusages.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift)
						| System.Windows.Forms.Keys.F)));
			this.menufindusages.Size = new System.Drawing.Size(208, 22);
			this.menufindusages.Text = "Find usages";
			this.menufindusages.Click += new System.EventHandler(this.menufindusages_Click);
			// 
			// scriptpanel
			// 
			this.scriptpanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.scriptpanel.Controls.Add(this.scriptedit);
			this.scriptpanel.Location = new System.Drawing.Point(0, 27);
			this.scriptpanel.Name = "scriptpanel";
			this.scriptpanel.Size = new System.Drawing.Size(474, 381);
			this.scriptpanel.TabIndex = 2;
			// 
			// functionbar
			// 
			this.functionbar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.functionbar.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.functionbar.FormattingEnabled = true;
			this.functionbar.Location = new System.Drawing.Point(0, 0);
			this.functionbar.Name = "functionbar";
			this.functionbar.Size = new System.Drawing.Size(474, 21);
			this.functionbar.TabIndex = 2;
			this.functionbar.TabStop = false;
			this.functionbar.SelectedIndexChanged += new System.EventHandler(this.functionbar_SelectedIndexChanged);
			this.functionbar.DropDown += new System.EventHandler(this.functionbar_DropDown);
			// 
			// ScriptEditorControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.Controls.Add(this.scriptpanel);
			this.Controls.Add(this.functionbar);
			this.Name = "ScriptEditorControl";
			this.Size = new System.Drawing.Size(474, 408);
			this.contextmenu.ResumeLayout(false);
			this.scriptpanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private ScintillaNET.Scintilla scriptedit;
		private System.Windows.Forms.Panel scriptpanel;
		private System.Windows.Forms.ComboBox functionbar;
		private System.Windows.Forms.ContextMenuStrip contextmenu;
		private System.Windows.Forms.ToolStripMenuItem menuundo;
		private System.Windows.Forms.ToolStripMenuItem menuredo;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem menucut;
		private System.Windows.Forms.ToolStripMenuItem menucopy;
		private System.Windows.Forms.ToolStripMenuItem menupaste;
		private System.Windows.Forms.ToolStripMenuItem menudelete;
		private System.Windows.Forms.ToolStripMenuItem menuduplicateline;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripMenuItem menuselectall;
		private System.Windows.Forms.ToolStripMenuItem menufindusages;
	}
}
