namespace CodeImp.DoomBuilder.USDF
{
	partial class ToolsForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.toolstrip = new System.Windows.Forms.ToolStrip();
			this.dialogbutton = new System.Windows.Forms.ToolStripButton();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.editmenu = new System.Windows.Forms.ToolStripMenuItem();
			this.dialogitem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolstrip.SuspendLayout();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolstrip
			// 
			this.toolstrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.dialogbutton});
			this.toolstrip.Location = new System.Drawing.Point(0, 24);
			this.toolstrip.Name = "toolstrip";
			this.toolstrip.Size = new System.Drawing.Size(196, 25);
			this.toolstrip.TabIndex = 0;
			this.toolstrip.Text = "toolStrip1";
			// 
			// dialogbutton
			// 
			this.dialogbutton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.dialogbutton.Image = global::CodeImp.DoomBuilder.USDF.Properties.Resources.Dialog;
			this.dialogbutton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.dialogbutton.Name = "dialogbutton";
			this.dialogbutton.Size = new System.Drawing.Size(23, 22);
			this.dialogbutton.Tag = "opendialogeditor";
			this.dialogbutton.Text = "Open Dialog Editor";
			this.dialogbutton.Click += new System.EventHandler(this.InvokeTaggedAction);
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editmenu});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(196, 24);
			this.menuStrip1.TabIndex = 1;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// editmenu
			// 
			this.editmenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.dialogitem});
			this.editmenu.Name = "editmenu";
			this.editmenu.Size = new System.Drawing.Size(39, 20);
			this.editmenu.Text = "Edit";
			// 
			// dialogitem
			// 
			this.dialogitem.Image = global::CodeImp.DoomBuilder.USDF.Properties.Resources.Dialog;
			this.dialogitem.Name = "dialogitem";
			this.dialogitem.Size = new System.Drawing.Size(152, 22);
			this.dialogitem.Tag = "opendialogeditor";
			this.dialogitem.Text = "Dialog Editor...";
			this.dialogitem.Click += new System.EventHandler(this.InvokeTaggedAction);
			// 
			// ToolsForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(196, 78);
			this.Controls.Add(this.toolstrip);
			this.Controls.Add(this.menuStrip1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MainMenuStrip = this.menuStrip1;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ToolsForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "ToolsForm";
			this.toolstrip.ResumeLayout(false);
			this.toolstrip.PerformLayout();
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ToolStrip toolstrip;
		private System.Windows.Forms.ToolStripButton dialogbutton;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem editmenu;
		private System.Windows.Forms.ToolStripMenuItem dialogitem;
	}
}