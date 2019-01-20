namespace CodeImp.DoomBuilder.BuilderModes
{
	partial class DrawLineOptionsPanel
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
			this.continuousdrawing = new System.Windows.Forms.ToolStripButton();
			this.autoclosedrawing = new System.Windows.Forms.ToolStripButton();
			this.showguidelines = new System.Windows.Forms.ToolStripButton();
			this.toolStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolStrip1
			// 
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.continuousdrawing,
            this.autoclosedrawing,
            this.showguidelines});
			this.toolStrip1.Location = new System.Drawing.Point(0, 0);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Size = new System.Drawing.Size(619, 25);
			this.toolStrip1.TabIndex = 8;
			this.toolStrip1.Text = "toolStrip1";
			// 
			// continuousdrawing
			// 
			this.continuousdrawing.CheckOnClick = true;
			this.continuousdrawing.Image = global::CodeImp.DoomBuilder.BuilderModes.Properties.Resources.Repeat;
			this.continuousdrawing.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.continuousdrawing.Name = "continuousdrawing";
			this.continuousdrawing.Size = new System.Drawing.Size(135, 22);
			this.continuousdrawing.Text = "Continuous drawing";
			this.continuousdrawing.CheckedChanged += new System.EventHandler(this.continuousdrawing_CheckedChanged);
			// 
			// autoclosedrawing
			// 
			this.autoclosedrawing.CheckOnClick = true;
			this.autoclosedrawing.Image = global::CodeImp.DoomBuilder.BuilderModes.Properties.Resources.NewSector2;
			this.autoclosedrawing.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.autoclosedrawing.Margin = new System.Windows.Forms.Padding(2, 1, 0, 2);
			this.autoclosedrawing.Name = "autoclosedrawing";
			this.autoclosedrawing.Size = new System.Drawing.Size(133, 22);
			this.autoclosedrawing.Text = "Auto-finish drawing";
			this.autoclosedrawing.CheckedChanged += new System.EventHandler(this.autoclosedrawing_CheckedChanged);
			// 
			// showguidelines
			// 
			this.showguidelines.CheckOnClick = true;
			this.showguidelines.Image = global::CodeImp.DoomBuilder.BuilderModes.Properties.Resources.Guidelines;
			this.showguidelines.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.showguidelines.Margin = new System.Windows.Forms.Padding(2, 1, 0, 2);
			this.showguidelines.Name = "showguidelines";
			this.showguidelines.Size = new System.Drawing.Size(82, 22);
			this.showguidelines.Text = "Guidelines";
			this.showguidelines.CheckedChanged += new System.EventHandler(this.showguidelines_CheckedChanged);
			// 
			// DrawLineOptionsPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.toolStrip1);
			this.Name = "DrawLineOptionsPanel";
			this.Size = new System.Drawing.Size(619, 60);
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.ToolStripButton continuousdrawing;
		private System.Windows.Forms.ToolStripButton autoclosedrawing;
		private System.Windows.Forms.ToolStripButton showguidelines;
	}
}
