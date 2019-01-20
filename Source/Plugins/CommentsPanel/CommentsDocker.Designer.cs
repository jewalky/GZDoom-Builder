namespace CodeImp.DoomBuilder.CommentsPanel
{
	partial class CommentsDocker
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			this.optionsgroup = new System.Windows.Forms.GroupBox();
			this.clickselects = new System.Windows.Forms.CheckBox();
			this.filtermode = new System.Windows.Forms.CheckBox();
			this.grid = new System.Windows.Forms.DataGridView();
			this.iconcolumn = new System.Windows.Forms.DataGridViewImageColumn();
			this.textcolumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.updatetimer = new System.Windows.Forms.Timer(this.components);
			this.contextmenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.editobjectitem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.selectitem = new System.Windows.Forms.ToolStripMenuItem();
			this.selectadditiveitem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
			this.removecommentsitem = new System.Windows.Forms.ToolStripMenuItem();
			this.addcomment = new System.Windows.Forms.Button();
			this.addcommentgroup = new System.Windows.Forms.GroupBox();
			this.addcommenttext = new System.Windows.Forms.TextBox();
			this.enabledtimer = new System.Windows.Forms.Timer(this.components);
			this.optionsgroup.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.contextmenu.SuspendLayout();
			this.addcommentgroup.SuspendLayout();
			this.SuspendLayout();
			// 
			// optionsgroup
			// 
			this.optionsgroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.optionsgroup.Controls.Add(this.clickselects);
			this.optionsgroup.Controls.Add(this.filtermode);
			this.optionsgroup.Location = new System.Drawing.Point(3, 561);
			this.optionsgroup.Name = "optionsgroup";
			this.optionsgroup.Size = new System.Drawing.Size(244, 93);
			this.optionsgroup.TabIndex = 1;
			this.optionsgroup.TabStop = false;
			// 
			// clickselects
			// 
			this.clickselects.AutoSize = true;
			this.clickselects.Location = new System.Drawing.Point(15, 57);
			this.clickselects.Name = "clickselects";
			this.clickselects.Size = new System.Drawing.Size(96, 17);
			this.clickselects.TabIndex = 1;
			this.clickselects.Text = "Select on click";
			this.clickselects.UseVisualStyleBackColor = true;
			this.clickselects.CheckedChanged += new System.EventHandler(this.clickselects_CheckedChanged);
			// 
			// filtermode
			// 
			this.filtermode.AutoSize = true;
			this.filtermode.Location = new System.Drawing.Point(15, 25);
			this.filtermode.Name = "filtermode";
			this.filtermode.Size = new System.Drawing.Size(168, 17);
			this.filtermode.TabIndex = 0;
			this.filtermode.Text = "Comments from this mode only";
			this.filtermode.UseVisualStyleBackColor = true;
			this.filtermode.CheckedChanged += new System.EventHandler(this.filtermode_CheckedChanged);
			// 
			// grid
			// 
			this.grid.AllowUserToAddRows = false;
			this.grid.AllowUserToDeleteRows = false;
			this.grid.AllowUserToResizeColumns = false;
			this.grid.AllowUserToResizeRows = false;
			this.grid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.grid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
			this.grid.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
			this.grid.BackgroundColor = System.Drawing.SystemColors.Window;
			this.grid.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.grid.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
			this.grid.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
			this.grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.grid.ColumnHeadersVisible = false;
			this.grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.iconcolumn,
            this.textcolumn});
			this.grid.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
			this.grid.Location = new System.Drawing.Point(0, 0);
			this.grid.Name = "grid";
			this.grid.ReadOnly = true;
			this.grid.RowHeadersVisible = false;
			dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
			dataGridViewCellStyle1.Padding = new System.Windows.Forms.Padding(2, 4, 2, 5);
			this.grid.RowsDefaultCellStyle = dataGridViewCellStyle1;
			this.grid.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.grid.ShowCellErrors = false;
			this.grid.ShowCellToolTips = false;
			this.grid.ShowEditingIcon = false;
			this.grid.ShowRowErrors = false;
			this.grid.Size = new System.Drawing.Size(250, 465);
			this.grid.StandardTab = true;
			this.grid.TabIndex = 6;
			this.grid.MouseDown += new System.Windows.Forms.MouseEventHandler(this.grid_MouseDown);
			this.grid.Leave += new System.EventHandler(this.grid_Leave);
			this.grid.MouseUp += new System.Windows.Forms.MouseEventHandler(this.grid_MouseUp);
			// 
			// iconcolumn
			// 
			this.iconcolumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
			this.iconcolumn.HeaderText = "Icon";
			this.iconcolumn.MinimumWidth = 20;
			this.iconcolumn.Name = "iconcolumn";
			this.iconcolumn.ReadOnly = true;
			this.iconcolumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.iconcolumn.Width = 24;
			// 
			// textcolumn
			// 
			this.textcolumn.HeaderText = "Text";
			this.textcolumn.Name = "textcolumn";
			this.textcolumn.ReadOnly = true;
			// 
			// updatetimer
			// 
			this.updatetimer.Interval = 2000;
			this.updatetimer.Tick += new System.EventHandler(this.updatetimer_Tick);
			// 
			// contextmenu
			// 
			this.contextmenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editobjectitem,
            this.toolStripMenuItem1,
            this.selectitem,
            this.selectadditiveitem,
            this.toolStripMenuItem2,
            this.removecommentsitem});
			this.contextmenu.Name = "contextMenuStrip1";
			this.contextmenu.Size = new System.Drawing.Size(175, 116);
			this.contextmenu.Closed += new System.Windows.Forms.ToolStripDropDownClosedEventHandler(this.contextmenu_Closed);
			// 
			// editobjectitem
			// 
			this.editobjectitem.Name = "editobjectitem";
			this.editobjectitem.Size = new System.Drawing.Size(174, 22);
			this.editobjectitem.Text = "Edit Objects";
			this.editobjectitem.Click += new System.EventHandler(this.editobjectitem_Click);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(171, 6);
			// 
			// selectitem
			// 
			this.selectitem.Name = "selectitem";
			this.selectitem.Size = new System.Drawing.Size(174, 22);
			this.selectitem.Text = "Select";
			this.selectitem.Click += new System.EventHandler(this.selectitem_Click);
			// 
			// selectadditiveitem
			// 
			this.selectadditiveitem.Name = "selectadditiveitem";
			this.selectadditiveitem.Size = new System.Drawing.Size(174, 22);
			this.selectadditiveitem.Text = "Select Additive";
			this.selectadditiveitem.Click += new System.EventHandler(this.selectadditiveitem_Click);
			// 
			// toolStripMenuItem2
			// 
			this.toolStripMenuItem2.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
			this.toolStripMenuItem2.Name = "toolStripMenuItem2";
			this.toolStripMenuItem2.Size = new System.Drawing.Size(171, 6);
			// 
			// removecommentsitem
			// 
			this.removecommentsitem.Name = "removecommentsitem";
			this.removecommentsitem.Size = new System.Drawing.Size(174, 22);
			this.removecommentsitem.Text = "Remove Comment";
			this.removecommentsitem.Click += new System.EventHandler(this.removecommentsitem_Click);
			// 
			// addcomment
			// 
			this.addcomment.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.addcomment.Location = new System.Drawing.Point(6, 45);
			this.addcomment.Name = "addcomment";
			this.addcomment.Size = new System.Drawing.Size(232, 31);
			this.addcomment.TabIndex = 7;
			this.addcomment.Text = "Set Selection Comment";
			this.addcomment.UseVisualStyleBackColor = true;
			this.addcomment.Click += new System.EventHandler(this.addcomment_Click);
			// 
			// addcommentgroup
			// 
			this.addcommentgroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.addcommentgroup.Controls.Add(this.addcommenttext);
			this.addcommentgroup.Controls.Add(this.addcomment);
			this.addcommentgroup.Location = new System.Drawing.Point(3, 471);
			this.addcommentgroup.Name = "addcommentgroup";
			this.addcommentgroup.Size = new System.Drawing.Size(244, 84);
			this.addcommentgroup.TabIndex = 8;
			this.addcommentgroup.TabStop = false;
			// 
			// addcommenttext
			// 
			this.addcommenttext.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.addcommenttext.Location = new System.Drawing.Point(6, 19);
			this.addcommenttext.Name = "addcommenttext";
			this.addcommenttext.Size = new System.Drawing.Size(232, 20);
			this.addcommenttext.TabIndex = 8;
			this.addcommenttext.KeyDown += new System.Windows.Forms.KeyEventHandler(this.addcommenttext_KeyDown);
			// 
			// enabledtimer
			// 
			this.enabledtimer.Interval = 333;
			this.enabledtimer.Tick += new System.EventHandler(this.enabledtimer_Tick);
			// 
			// CommentsDocker
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.Controls.Add(this.addcommentgroup);
			this.Controls.Add(this.grid);
			this.Controls.Add(this.optionsgroup);
			this.Name = "CommentsDocker";
			this.Size = new System.Drawing.Size(250, 657);
			this.optionsgroup.ResumeLayout(false);
			this.optionsgroup.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.contextmenu.ResumeLayout(false);
			this.addcommentgroup.ResumeLayout(false);
			this.addcommentgroup.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox optionsgroup;
		private System.Windows.Forms.CheckBox filtermode;
		private System.Windows.Forms.DataGridView grid;
		private System.Windows.Forms.DataGridViewImageColumn iconcolumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn textcolumn;
		private System.Windows.Forms.Timer updatetimer;
		private System.Windows.Forms.ContextMenuStrip contextmenu;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem selectitem;
		private System.Windows.Forms.ToolStripMenuItem selectadditiveitem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
		private System.Windows.Forms.ToolStripMenuItem removecommentsitem;
		private System.Windows.Forms.ToolStripMenuItem editobjectitem;
		private System.Windows.Forms.CheckBox clickselects;
		private System.Windows.Forms.Button addcomment;
		private System.Windows.Forms.GroupBox addcommentgroup;
		private System.Windows.Forms.TextBox addcommenttext;
		private System.Windows.Forms.Timer enabledtimer;
	}
}
