namespace CodeImp.DoomBuilder.Controls
{
	partial class ColorFieldsControl
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
			this.bReset = new System.Windows.Forms.Button();
			this.cpColor = new CodeImp.DoomBuilder.Controls.ColorControl();
			this.tbColor = new CodeImp.DoomBuilder.Controls.AutoSelectTextbox();
			this.tooltip = new System.Windows.Forms.ToolTip(this.components);
			this.SuspendLayout();
			// 
			// bReset
			// 
			this.bReset.Image = global::CodeImp.DoomBuilder.Properties.Resources.Reset;
			this.bReset.Location = new System.Drawing.Point(179, 3);
			this.bReset.Name = "bReset";
			this.bReset.Size = new System.Drawing.Size(23, 23);
			this.bReset.TabIndex = 43;
			this.tooltip.SetToolTip(this.bReset, "Reset");
			this.bReset.UseVisualStyleBackColor = true;
			this.bReset.Visible = false;
			this.bReset.Click += new System.EventHandler(this.bReset_Click);
			// 
			// cpColor
			// 
			this.cpColor.BackColor = System.Drawing.Color.Transparent;
			this.cpColor.Label = "Light:";
			this.cpColor.Location = new System.Drawing.Point(3, 3);
			this.cpColor.MaximumSize = new System.Drawing.Size(10000, 23);
			this.cpColor.MinimumSize = new System.Drawing.Size(100, 23);
			this.cpColor.Name = "cpColor";
			this.cpColor.Size = new System.Drawing.Size(100, 23);
			this.cpColor.TabIndex = 41;
			this.cpColor.ColorChanged += new System.EventHandler(this.cpColor_ColorChanged);
			// 
			// tbColor
			// 
			this.tbColor.Location = new System.Drawing.Point(109, 5);
			this.tbColor.Name = "tbColor";
			this.tbColor.Size = new System.Drawing.Size(64, 20);
			this.tbColor.TabIndex = 45;
			this.tbColor.TextChanged += new System.EventHandler(this.tbColor_TextChanged);
			// 
			// ColorFieldsControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.Controls.Add(this.tbColor);
			this.Controls.Add(this.bReset);
			this.Controls.Add(this.cpColor);
			this.Name = "ColorFieldsControl";
			this.Size = new System.Drawing.Size(207, 29);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button bReset;
		private CodeImp.DoomBuilder.Controls.ColorControl cpColor;
		private CodeImp.DoomBuilder.Controls.AutoSelectTextbox tbColor;
		private System.Windows.Forms.ToolTip tooltip;
	}
}
