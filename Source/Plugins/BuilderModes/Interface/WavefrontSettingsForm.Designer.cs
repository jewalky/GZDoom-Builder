namespace CodeImp.DoomBuilder.BuilderModes.Interface
{
	partial class WavefrontSettingsForm
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
			this.tbExportPath = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.cbFixScale = new System.Windows.Forms.CheckBox();
			this.export = new System.Windows.Forms.Button();
			this.cancel = new System.Windows.Forms.Button();
			this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
			this.cbExportTextures = new System.Windows.Forms.CheckBox();
			this.nudScale = new System.Windows.Forms.NumericUpDown();
			this.label2 = new System.Windows.Forms.Label();
			this.browse = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.nudScale)).BeginInit();
			this.SuspendLayout();
			// 
			// tbExportPath
			// 
			this.tbExportPath.Location = new System.Drawing.Point(55, 12);
			this.tbExportPath.Name = "tbExportPath";
			this.tbExportPath.Size = new System.Drawing.Size(344, 20);
			this.tbExportPath.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(18, 14);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(32, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Path:";
			// 
			// cbFixScale
			// 
			this.cbFixScale.AutoSize = true;
			this.cbFixScale.Location = new System.Drawing.Point(15, 64);
			this.cbFixScale.Name = "cbFixScale";
			this.cbFixScale.Size = new System.Drawing.Size(268, 17);
			this.cbFixScale.TabIndex = 3;
			this.cbFixScale.Text = "Export for GZDoom (increase vertical scale by 20%)";
			this.cbFixScale.UseVisualStyleBackColor = true;
			// 
			// export
			// 
			this.export.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.export.Location = new System.Drawing.Point(279, 84);
			this.export.Name = "export";
			this.export.Size = new System.Drawing.Size(75, 23);
			this.export.TabIndex = 4;
			this.export.Text = "Export";
			this.export.UseVisualStyleBackColor = true;
			this.export.Click += new System.EventHandler(this.export_Click);
			// 
			// cancel
			// 
			this.cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancel.Location = new System.Drawing.Point(360, 84);
			this.cancel.Name = "cancel";
			this.cancel.Size = new System.Drawing.Size(75, 23);
			this.cancel.TabIndex = 5;
			this.cancel.Text = "Cancel";
			this.cancel.UseVisualStyleBackColor = true;
			this.cancel.Click += new System.EventHandler(this.cancel_Click);
			// 
			// saveFileDialog
			// 
			this.saveFileDialog.DefaultExt = "obj";
			this.saveFileDialog.Filter = "Wavefront obj files|*.obj";
			this.saveFileDialog.Title = "Select save location:";
			// 
			// cbExportTextures
			// 
			this.cbExportTextures.AutoSize = true;
			this.cbExportTextures.Location = new System.Drawing.Point(15, 87);
			this.cbExportTextures.Name = "cbExportTextures";
			this.cbExportTextures.Size = new System.Drawing.Size(96, 17);
			this.cbExportTextures.TabIndex = 6;
			this.cbExportTextures.Text = "Export textures";
			this.cbExportTextures.UseVisualStyleBackColor = true;
			// 
			// nudScale
			// 
			this.nudScale.DecimalPlaces = 4;
			this.nudScale.Location = new System.Drawing.Point(55, 38);
			this.nudScale.Maximum = new decimal(new int[] {
            2048,
            0,
            0,
            0});
			this.nudScale.Minimum = new decimal(new int[] {
            2048,
            0,
            0,
            -2147483648});
			this.nudScale.Name = "nudScale";
			this.nudScale.Size = new System.Drawing.Size(94, 20);
			this.nudScale.TabIndex = 8;
			this.nudScale.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 40);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(37, 13);
			this.label2.TabIndex = 9;
			this.label2.Text = "Scale:";
			// 
			// browse
			// 
			this.browse.Image = global::CodeImp.DoomBuilder.BuilderModes.Properties.Resources.Folder;
			this.browse.Location = new System.Drawing.Point(405, 10);
			this.browse.Name = "browse";
			this.browse.Size = new System.Drawing.Size(30, 24);
			this.browse.TabIndex = 1;
			this.browse.UseVisualStyleBackColor = true;
			this.browse.Click += new System.EventHandler(this.browse_Click);
			// 
			// WavefrontSettingsForm
			// 
			this.AcceptButton = this.export;
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.CancelButton = this.cancel;
			this.ClientSize = new System.Drawing.Size(447, 109);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.nudScale);
			this.Controls.Add(this.cbExportTextures);
			this.Controls.Add(this.cancel);
			this.Controls.Add(this.export);
			this.Controls.Add(this.cbFixScale);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.browse);
			this.Controls.Add(this.tbExportPath);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "WavefrontSettingsForm";
			this.Opacity = 0;
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "Export to Wavefront .obj";
			((System.ComponentModel.ISupportInitialize)(this.nudScale)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox tbExportPath;
		private System.Windows.Forms.Button browse;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.CheckBox cbFixScale;
		private System.Windows.Forms.Button export;
		private System.Windows.Forms.Button cancel;
		private System.Windows.Forms.SaveFileDialog saveFileDialog;
		private System.Windows.Forms.CheckBox cbExportTextures;
		private System.Windows.Forms.NumericUpDown nudScale;
		private System.Windows.Forms.Label label2;
	}
}