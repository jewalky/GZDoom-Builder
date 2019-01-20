namespace CodeImp.DoomBuilder.BuilderModes.Editing
{
	partial class WAuthorTools
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
			this.linedefpopup = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.propertiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.splitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.flipToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.curveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.linedefpopup.SuspendLayout();
			this.SuspendLayout();
			// 
			// linedefpopup
			// 
			this.linedefpopup.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.propertiesToolStripMenuItem,
            this.toolStripMenuItem1,
            this.deleteToolStripMenuItem,
            this.splitToolStripMenuItem,
            this.flipToolStripMenuItem,
            this.curveToolStripMenuItem});
			this.linedefpopup.Name = "linedefpopup";
			this.linedefpopup.Size = new System.Drawing.Size(147, 120);
			// 
			// propertiesToolStripMenuItem
			// 
			this.propertiesToolStripMenuItem.Name = "propertiesToolStripMenuItem";
			this.propertiesToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
			this.propertiesToolStripMenuItem.Text = "Properties...";
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(143, 6);
			// 
			// deleteToolStripMenuItem
			// 
			this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
			this.deleteToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
			this.deleteToolStripMenuItem.Text = "Delete";
			// 
			// splitToolStripMenuItem
			// 
			this.splitToolStripMenuItem.Name = "splitToolStripMenuItem";
			this.splitToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
			this.splitToolStripMenuItem.Text = "Split";
			// 
			// flipToolStripMenuItem
			// 
			this.flipToolStripMenuItem.Name = "flipToolStripMenuItem";
			this.flipToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
			this.flipToolStripMenuItem.Text = "Flip";
			// 
			// curveToolStripMenuItem
			// 
			this.curveToolStripMenuItem.Name = "curveToolStripMenuItem";
			this.curveToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
			this.curveToolStripMenuItem.Text = "Curve...";
			// 
			// WAuthorTools
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 14F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(243, 130);
			this.ControlBox = false;
			this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "WAuthorTools";
			this.Text = "WAuthorTools";
			this.linedefpopup.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ContextMenuStrip linedefpopup;
		private System.Windows.Forms.ToolStripMenuItem propertiesToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem splitToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem flipToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem curveToolStripMenuItem;
	}
}