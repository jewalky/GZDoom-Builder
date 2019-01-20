namespace CodeImp.DoomBuilder.BuilderEffects
{
	partial class MenusForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if(disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.menuStrip = new System.Windows.Forms.MenuStrip();
			this.stripimport = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.stripmodes = new System.Windows.Forms.ToolStripMenuItem();
			this.menujitter = new System.Windows.Forms.ToolStripMenuItem();
			this.menusectorflatshading = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStrip = new System.Windows.Forms.ToolStrip();
			this.buttonjitter = new System.Windows.Forms.ToolStripButton();
			this.buttonsectorflatshading = new System.Windows.Forms.ToolStripButton();
			this.menuStrip.SuspendLayout();
			this.toolStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// menuStrip
			// 
			this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.stripimport,
            this.stripmodes});
			this.menuStrip.Location = new System.Drawing.Point(0, 0);
			this.menuStrip.Name = "menuStrip";
			this.menuStrip.Size = new System.Drawing.Size(452, 24);
			this.menuStrip.TabIndex = 0;
			this.menuStrip.Text = "menuStrip1";
			// 
			// stripimport
			// 
			this.stripimport.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1});
			this.stripimport.Name = "stripimport";
			this.stripimport.Size = new System.Drawing.Size(55, 20);
			this.stripimport.Text = "Import";
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Image = global::CodeImp.DoomBuilder.BuilderEffects.Properties.Resources.Terrain;
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(215, 22);
			this.toolStripMenuItem1.Tag = "importobjasterrain";
			this.toolStripMenuItem1.Text = "Wavefront .obj as Terrain...";
			this.toolStripMenuItem1.Click += new System.EventHandler(this.InvokeTaggedAction);
			// 
			// stripmodes
			// 
			this.stripmodes.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menujitter,
            this.menusectorflatshading});
			this.stripmodes.Name = "stripmodes";
			this.stripmodes.Size = new System.Drawing.Size(55, 20);
			this.stripmodes.Text = "Modes";
			// 
			// menujitter
			// 
			this.menujitter.Image = global::CodeImp.DoomBuilder.BuilderEffects.Properties.Resources.Jitter;
			this.menujitter.Name = "menujitter";
			this.menujitter.Size = new System.Drawing.Size(220, 22);
			this.menujitter.Tag = "applyjitter";
			this.menujitter.Text = "Randomize...";
			this.menujitter.Click += new System.EventHandler(this.InvokeTaggedAction);
			// 
			// menusectorflatshading
			// 
			this.menusectorflatshading.Image = global::CodeImp.DoomBuilder.BuilderEffects.Properties.Resources.FlatShading;
			this.menusectorflatshading.Name = "menusectorflatshading";
			this.menusectorflatshading.Size = new System.Drawing.Size(220, 22);
			this.menusectorflatshading.Tag = "applydirectionalshading";
			this.menusectorflatshading.Text = "Apply Directional Shading...";
			this.menusectorflatshading.Click += new System.EventHandler(this.InvokeTaggedAction);
			// 
			// toolStrip
			// 
			this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonjitter,
            this.buttonsectorflatshading});
			this.toolStrip.Location = new System.Drawing.Point(0, 24);
			this.toolStrip.Name = "toolStrip";
			this.toolStrip.Size = new System.Drawing.Size(452, 25);
			this.toolStrip.TabIndex = 1;
			this.toolStrip.Text = "toolStrip1";
			// 
			// buttonjitter
			// 
			this.buttonjitter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonjitter.Image = global::CodeImp.DoomBuilder.BuilderEffects.Properties.Resources.Jitter;
			this.buttonjitter.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonjitter.Name = "buttonjitter";
			this.buttonjitter.Size = new System.Drawing.Size(23, 22);
			this.buttonjitter.Tag = "applyjitter";
			this.buttonjitter.Text = "Randomize";
			this.buttonjitter.Click += new System.EventHandler(this.InvokeTaggedAction);
			// 
			// buttonsectorflatshading
			// 
			this.buttonsectorflatshading.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonsectorflatshading.Image = global::CodeImp.DoomBuilder.BuilderEffects.Properties.Resources.FlatShading;
			this.buttonsectorflatshading.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonsectorflatshading.Name = "buttonsectorflatshading";
			this.buttonsectorflatshading.Size = new System.Drawing.Size(23, 22);
			this.buttonsectorflatshading.Tag = "applydirectionalshading";
			this.buttonsectorflatshading.Text = "Apply Directional Shading";
			this.buttonsectorflatshading.Click += new System.EventHandler(this.InvokeTaggedAction);
			// 
			// MenusForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.ClientSize = new System.Drawing.Size(452, 129);
			this.Controls.Add(this.toolStrip);
			this.Controls.Add(this.menuStrip);
			this.MainMenuStrip = this.menuStrip;
			this.Name = "MenusForm";
			this.Text = "MenusForm";
			this.menuStrip.ResumeLayout(false);
			this.menuStrip.PerformLayout();
			this.toolStrip.ResumeLayout(false);
			this.toolStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.MenuStrip menuStrip;
		private System.Windows.Forms.ToolStripMenuItem stripimport;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
		private System.Windows.Forms.ToolStrip toolStrip;
		private System.Windows.Forms.ToolStripButton buttonjitter;
		private System.Windows.Forms.ToolStripMenuItem stripmodes;
		private System.Windows.Forms.ToolStripMenuItem menujitter;
		private System.Windows.Forms.ToolStripMenuItem menusectorflatshading;
		private System.Windows.Forms.ToolStripButton buttonsectorflatshading;
	}
}