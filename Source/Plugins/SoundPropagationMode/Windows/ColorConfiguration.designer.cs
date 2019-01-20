namespace CodeImp.DoomBuilder.SoundPropagationMode
{
	partial class ColorConfiguration
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
			this.highlightcolor = new CodeImp.DoomBuilder.Controls.ColorControl();
			this.level1color = new CodeImp.DoomBuilder.Controls.ColorControl();
			this.level2color = new CodeImp.DoomBuilder.Controls.ColorControl();
			this.nosoundcolor = new CodeImp.DoomBuilder.Controls.ColorControl();
			this.blocksoundcolor = new CodeImp.DoomBuilder.Controls.ColorControl();
			this.okbutton = new System.Windows.Forms.Button();
			this.cancelbutton = new System.Windows.Forms.Button();
			this.resetcolors = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// highlightcolor
			// 
			this.highlightcolor.BackColor = System.Drawing.Color.Transparent;
			this.highlightcolor.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.highlightcolor.Label = "Highlight color:";
			this.highlightcolor.Location = new System.Drawing.Point(12, 12);
			this.highlightcolor.MaximumSize = new System.Drawing.Size(10000, 23);
			this.highlightcolor.MinimumSize = new System.Drawing.Size(100, 23);
			this.highlightcolor.Name = "highlightcolor";
			this.highlightcolor.Size = new System.Drawing.Size(150, 23);
			this.highlightcolor.TabIndex = 0;
			// 
			// level1color
			// 
			this.level1color.BackColor = System.Drawing.Color.Transparent;
			this.level1color.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.level1color.Label = "Level 1 color:";
			this.level1color.Location = new System.Drawing.Point(12, 41);
			this.level1color.MaximumSize = new System.Drawing.Size(10000, 23);
			this.level1color.MinimumSize = new System.Drawing.Size(100, 23);
			this.level1color.Name = "level1color";
			this.level1color.Size = new System.Drawing.Size(150, 23);
			this.level1color.TabIndex = 1;
			// 
			// level2color
			// 
			this.level2color.BackColor = System.Drawing.Color.Transparent;
			this.level2color.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.level2color.Label = "Level 2 color:";
			this.level2color.Location = new System.Drawing.Point(12, 70);
			this.level2color.MaximumSize = new System.Drawing.Size(10000, 23);
			this.level2color.MinimumSize = new System.Drawing.Size(100, 23);
			this.level2color.Name = "level2color";
			this.level2color.Size = new System.Drawing.Size(150, 23);
			this.level2color.TabIndex = 2;
			// 
			// nosoundcolor
			// 
			this.nosoundcolor.BackColor = System.Drawing.Color.Transparent;
			this.nosoundcolor.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.nosoundcolor.Label = "No sound color:";
			this.nosoundcolor.Location = new System.Drawing.Point(12, 99);
			this.nosoundcolor.MaximumSize = new System.Drawing.Size(10000, 23);
			this.nosoundcolor.MinimumSize = new System.Drawing.Size(100, 23);
			this.nosoundcolor.Name = "nosoundcolor";
			this.nosoundcolor.Size = new System.Drawing.Size(150, 23);
			this.nosoundcolor.TabIndex = 3;
			// 
			// blocksoundcolor
			// 
			this.blocksoundcolor.BackColor = System.Drawing.Color.Transparent;
			this.blocksoundcolor.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.blocksoundcolor.Label = "Block sound color:";
			this.blocksoundcolor.Location = new System.Drawing.Point(12, 128);
			this.blocksoundcolor.MaximumSize = new System.Drawing.Size(10000, 23);
			this.blocksoundcolor.MinimumSize = new System.Drawing.Size(100, 23);
			this.blocksoundcolor.Name = "blocksoundcolor";
			this.blocksoundcolor.Size = new System.Drawing.Size(150, 23);
			this.blocksoundcolor.TabIndex = 4;
			// 
			// okbutton
			// 
			this.okbutton.Location = new System.Drawing.Point(12, 186);
			this.okbutton.Name = "okbutton";
			this.okbutton.Size = new System.Drawing.Size(73, 23);
			this.okbutton.TabIndex = 6;
			this.okbutton.Text = "OK";
			this.okbutton.UseVisualStyleBackColor = true;
			this.okbutton.Click += new System.EventHandler(this.okbutton_Click);
			// 
			// cancelbutton
			// 
			this.cancelbutton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelbutton.Location = new System.Drawing.Point(89, 186);
			this.cancelbutton.Name = "cancelbutton";
			this.cancelbutton.Size = new System.Drawing.Size(73, 23);
			this.cancelbutton.TabIndex = 7;
			this.cancelbutton.Text = "Cancel";
			this.cancelbutton.UseVisualStyleBackColor = true;
			// 
			// resetcolors
			// 
			this.resetcolors.Location = new System.Drawing.Point(12, 157);
			this.resetcolors.Name = "resetcolors";
			this.resetcolors.Size = new System.Drawing.Size(150, 23);
			this.resetcolors.TabIndex = 5;
			this.resetcolors.Text = "Reset colors";
			this.resetcolors.UseVisualStyleBackColor = true;
			this.resetcolors.Click += new System.EventHandler(this.resetcolors_Click);
			// 
			// ColorConfiguration
			// 
			this.AcceptButton = this.okbutton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.CancelButton = this.cancelbutton;
			this.ClientSize = new System.Drawing.Size(174, 214);
			this.Controls.Add(this.resetcolors);
			this.Controls.Add(this.cancelbutton);
			this.Controls.Add(this.okbutton);
			this.Controls.Add(this.blocksoundcolor);
			this.Controls.Add(this.nosoundcolor);
			this.Controls.Add(this.level2color);
			this.Controls.Add(this.level1color);
			this.Controls.Add(this.highlightcolor);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ColorConfiguration";
			this.Opacity = 0;
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Color Configuration";
			this.ResumeLayout(false);

		}

		#endregion

		private CodeImp.DoomBuilder.Controls.ColorControl highlightcolor;
		private CodeImp.DoomBuilder.Controls.ColorControl level1color;
		private CodeImp.DoomBuilder.Controls.ColorControl level2color;
		private CodeImp.DoomBuilder.Controls.ColorControl nosoundcolor;
		private CodeImp.DoomBuilder.Controls.ColorControl blocksoundcolor;
		private System.Windows.Forms.Button okbutton;
		private System.Windows.Forms.Button cancelbutton;
		private System.Windows.Forms.Button resetcolors;

	}
}