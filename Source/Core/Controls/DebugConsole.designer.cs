namespace CodeImp.DoomBuilder
{
	partial class DebugConsole
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
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
			this.searchbox = new System.Windows.Forms.ToolStripTextBox();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.autoscroll = new CodeImp.DoomBuilder.Controls.ToolStripCheckBox();
			this.console = new System.Windows.Forms.RichTextBox();
			this.clearall = new System.Windows.Forms.ToolStripButton();
			this.wordwrap = new System.Windows.Forms.ToolStripButton();
			this.filterselector = new System.Windows.Forms.ToolStripDropDownButton();
			this.filterlog = new System.Windows.Forms.ToolStripMenuItem();
			this.filterinfo = new System.Windows.Forms.ToolStripMenuItem();
			this.filterwarning = new System.Windows.Forms.ToolStripMenuItem();
			this.filtererrors = new System.Windows.Forms.ToolStripMenuItem();
			this.filterspecial = new System.Windows.Forms.ToolStripMenuItem();
			this.searchclear = new System.Windows.Forms.ToolStripButton();
			this.alwaysontop = new System.Windows.Forms.ToolStripButton();
			this.toolStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolStrip1
			// 
			this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.clearall,
            this.toolStripSeparator3,
            this.wordwrap,
            this.filterselector,
            this.toolStripSeparator1,
            this.toolStripLabel1,
            this.searchbox,
            this.searchclear,
            this.toolStripSeparator2,
            this.alwaysontop,
            this.autoscroll});
			this.toolStrip1.Location = new System.Drawing.Point(0, 0);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Size = new System.Drawing.Size(800, 25);
			this.toolStrip1.TabIndex = 0;
			this.toolStrip1.Text = "toolStrip1";
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
			// 
			// toolStripLabel1
			// 
			this.toolStripLabel1.Name = "toolStripLabel1";
			this.toolStripLabel1.Size = new System.Drawing.Size(39, 22);
			this.toolStripLabel1.Text = "Filter: ";
			// 
			// searchbox
			// 
			this.searchbox.Name = "searchbox";
			this.searchbox.Size = new System.Drawing.Size(100, 25);
			this.searchbox.TextChanged += new System.EventHandler(this.searchbox_TextChanged);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
			// 
			// autoscroll
			// 
			this.autoscroll.Checked = true;
			this.autoscroll.Margin = new System.Windows.Forms.Padding(3, 3, 0, 2);
			this.autoscroll.Name = "autoscroll";
			this.autoscroll.Size = new System.Drawing.Size(84, 20);
			this.autoscroll.Text = "Auto Scroll";
			// 
			// console
			// 
			this.console.BackColor = System.Drawing.SystemColors.Window;
			this.console.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.console.Dock = System.Windows.Forms.DockStyle.Fill;
			this.console.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.console.Location = new System.Drawing.Point(0, 25);
			this.console.Name = "console";
			this.console.ReadOnly = true;
			this.console.Size = new System.Drawing.Size(800, 125);
			this.console.TabIndex = 1;
			this.console.Text = "";
			// 
			// clearall
			// 
			this.clearall.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.clearall.Image = global::CodeImp.DoomBuilder.Properties.Resources.Clear;
			this.clearall.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.clearall.Name = "clearall";
			this.clearall.Size = new System.Drawing.Size(23, 22);
			this.clearall.Text = "Clear All";
			this.clearall.Click += new System.EventHandler(this.clearall_Click);
			// 
			// wordwrap
			// 
			this.wordwrap.CheckOnClick = true;
			this.wordwrap.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.wordwrap.Image = global::CodeImp.DoomBuilder.Properties.Resources.WordWrap;
			this.wordwrap.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.wordwrap.Name = "wordwrap";
			this.wordwrap.Size = new System.Drawing.Size(23, 22);
			this.wordwrap.Text = "Word Wrap";
			this.wordwrap.Click += new System.EventHandler(this.wordwrap_Click);
			// 
			// filterselector
			// 
			this.filterselector.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.filterselector.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.filterlog,
            this.filterinfo,
            this.filterwarning,
            this.filtererrors,
            this.filterspecial});
			this.filterselector.Image = global::CodeImp.DoomBuilder.Properties.Resources.Filter;
			this.filterselector.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.filterselector.Name = "filterselector";
			this.filterselector.Size = new System.Drawing.Size(29, 22);
			this.filterselector.Text = "Displayed Message Types";
			// 
			// filterlog
			// 
			this.filterlog.CheckOnClick = true;
			this.filterlog.Name = "filterlog";
			this.filterlog.Size = new System.Drawing.Size(124, 22);
			this.filterlog.Tag = 1;
			this.filterlog.Text = "Log";
			this.filterlog.Click += new System.EventHandler(this.filters_Click);
			// 
			// filterinfo
			// 
			this.filterinfo.Checked = true;
			this.filterinfo.CheckOnClick = true;
			this.filterinfo.CheckState = System.Windows.Forms.CheckState.Checked;
			this.filterinfo.Name = "filterinfo";
			this.filterinfo.Size = new System.Drawing.Size(124, 22);
			this.filterinfo.Tag = 2;
			this.filterinfo.Text = "Info";
			this.filterinfo.Click += new System.EventHandler(this.filters_Click);
			// 
			// filterwarning
			// 
			this.filterwarning.Checked = true;
			this.filterwarning.CheckOnClick = true;
			this.filterwarning.CheckState = System.Windows.Forms.CheckState.Checked;
			this.filterwarning.Name = "filterwarning";
			this.filterwarning.Size = new System.Drawing.Size(124, 22);
			this.filterwarning.Tag = 4;
			this.filterwarning.Text = "Warnings";
			this.filterwarning.Click += new System.EventHandler(this.filters_Click);
			// 
			// filtererrors
			// 
			this.filtererrors.Checked = true;
			this.filtererrors.CheckOnClick = true;
			this.filtererrors.CheckState = System.Windows.Forms.CheckState.Checked;
			this.filtererrors.Name = "filtererrors";
			this.filtererrors.Size = new System.Drawing.Size(124, 22);
			this.filtererrors.Tag = 8;
			this.filtererrors.Text = "Errors";
			this.filtererrors.Click += new System.EventHandler(this.filters_Click);
			// 
			// filterspecial
			// 
			this.filterspecial.Checked = true;
			this.filterspecial.CheckOnClick = true;
			this.filterspecial.CheckState = System.Windows.Forms.CheckState.Checked;
			this.filterspecial.Name = "filterspecial";
			this.filterspecial.Size = new System.Drawing.Size(124, 22);
			this.filterspecial.Tag = 16;
			this.filterspecial.Text = "Special";
			this.filterspecial.Click += new System.EventHandler(this.filters_Click);
			// 
			// searchclear
			// 
			this.searchclear.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.searchclear.Image = global::CodeImp.DoomBuilder.Properties.Resources.SearchClear;
			this.searchclear.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.searchclear.Name = "searchclear";
			this.searchclear.Size = new System.Drawing.Size(23, 22);
			this.searchclear.Text = "Clear Filter";
			this.searchclear.Click += new System.EventHandler(this.searchclear_Click);
			// 
			// alwaysontop
			// 
			this.alwaysontop.CheckOnClick = true;
			this.alwaysontop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.alwaysontop.Image = global::CodeImp.DoomBuilder.Properties.Resources.Pin;
			this.alwaysontop.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.alwaysontop.Name = "alwaysontop";
			this.alwaysontop.Size = new System.Drawing.Size(23, 22);
			this.alwaysontop.Text = "Stay on Top";
			// 
			// DebugConsole
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.Controls.Add(this.console);
			this.Controls.Add(this.toolStrip1);
			this.Name = "DebugConsole";
			this.Size = new System.Drawing.Size(800, 150);
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.ToolStripButton clearall;
		private System.Windows.Forms.ToolStripDropDownButton filterselector;
		private System.Windows.Forms.ToolStripMenuItem filterlog;
		private System.Windows.Forms.ToolStripMenuItem filterwarning;
		private System.Windows.Forms.ToolStripMenuItem filtererrors;
		private System.Windows.Forms.ToolStripMenuItem filterspecial;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripLabel toolStripLabel1;
		private System.Windows.Forms.ToolStripTextBox searchbox;
		private System.Windows.Forms.ToolStripButton searchclear;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripMenuItem filterinfo;
		private System.Windows.Forms.ToolStripButton wordwrap;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.RichTextBox console;
		private System.Windows.Forms.ToolStripButton alwaysontop;
		private CodeImp.DoomBuilder.Controls.ToolStripCheckBox autoscroll;
	}
}
