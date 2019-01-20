namespace CodeImp.DoomBuilder.TagRange
{
	partial class ToolsForm
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
			this.toolstrip = new System.Windows.Forms.ToolStrip();
			this.seperator1 = new System.Windows.Forms.ToolStripSeparator();
			this.tagrangebutton = new System.Windows.Forms.ToolStripButton();
			this.seperator2 = new System.Windows.Forms.ToolStripSeparator();
			this.toolstrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolstrip
			// 
			this.toolstrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.seperator1,
            this.tagrangebutton,
            this.seperator2});
			this.toolstrip.Location = new System.Drawing.Point(0, 0);
			this.toolstrip.Name = "toolstrip";
			this.toolstrip.Size = new System.Drawing.Size(330, 25);
			this.toolstrip.TabIndex = 0;
			this.toolstrip.Text = "toolStrip1";
			// 
			// seperator1
			// 
			this.seperator1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.seperator1.Name = "seperator1";
			this.seperator1.Size = new System.Drawing.Size(6, 25);
			// 
			// tagrangebutton
			// 
			this.tagrangebutton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tagrangebutton.Image = global::CodeImp.DoomBuilder.TagRange.Properties.Resources.tag_blue;
			this.tagrangebutton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tagrangebutton.Name = "tagrangebutton";
			this.tagrangebutton.Size = new System.Drawing.Size(23, 22);
			this.tagrangebutton.Tag = "rangetagselection";
			this.tagrangebutton.Text = "Tag Range";
			this.tagrangebutton.Click += new System.EventHandler(this.InvokeTaggedAction);
			// 
			// seperator2
			// 
			this.seperator2.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.seperator2.Name = "seperator2";
			this.seperator2.Size = new System.Drawing.Size(6, 25);
			// 
			// ToolsForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.ClientSize = new System.Drawing.Size(330, 230);
			this.Controls.Add(this.toolstrip);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ToolsForm";
			this.Text = "Tag Range";
			this.toolstrip.ResumeLayout(false);
			this.toolstrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ToolStrip toolstrip;
		private System.Windows.Forms.ToolStripButton tagrangebutton;
		private System.Windows.Forms.ToolStripSeparator seperator2;
		private System.Windows.Forms.ToolStripSeparator seperator1;
	}
}