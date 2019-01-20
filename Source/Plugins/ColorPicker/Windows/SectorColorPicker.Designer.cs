namespace CodeImp.DoomBuilder.ColorPicker.Windows
{
	partial class SectorColorPicker
	{
		/// <summary>
		/// Требуется переменная конструктора.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Освободить все используемые ресурсы.
		/// </summary>
		/// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
		protected override void Dispose(bool disposing) {
			if(disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Код, автоматически созданный конструктором форм Windows

		/// <summary>
		/// Обязательный метод для поддержки конструктора - не изменяйте
		/// содержимое данного метода при помощи редактора кода.
		/// </summary>
		private void InitializeComponent() {
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.rbFadeColor = new System.Windows.Forms.RadioButton();
			this.rbSectorColor = new System.Windows.Forms.RadioButton();
			this.colorPickerControl1 = new CodeImp.DoomBuilder.ColorPicker.Controls.ColorPickerControl();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.rbFadeColor);
			this.groupBox1.Controls.Add(this.rbSectorColor);
			this.groupBox1.Location = new System.Drawing.Point(8, 212);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(311, 62);
			this.groupBox1.TabIndex = 2;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Edit:";
			// 
			// rbFadeColor
			// 
			this.rbFadeColor.AutoSize = true;
			this.rbFadeColor.Location = new System.Drawing.Point(7, 41);
			this.rbFadeColor.Name = "rbFadeColor";
			this.rbFadeColor.Size = new System.Drawing.Size(75, 17);
			this.rbFadeColor.TabIndex = 1;
			this.rbFadeColor.Tag = "fadecolor";
			this.rbFadeColor.Text = "Fade color";
			this.rbFadeColor.UseVisualStyleBackColor = true;
			// 
			// rbSectorColor
			// 
			this.rbSectorColor.AutoSize = true;
			this.rbSectorColor.Checked = true;
			this.rbSectorColor.Location = new System.Drawing.Point(7, 20);
			this.rbSectorColor.Name = "rbSectorColor";
			this.rbSectorColor.Size = new System.Drawing.Size(82, 17);
			this.rbSectorColor.TabIndex = 0;
			this.rbSectorColor.TabStop = true;
			this.rbSectorColor.Tag = "lightcolor";
			this.rbSectorColor.Text = "Sector color";
			this.rbSectorColor.UseVisualStyleBackColor = true;
			// 
			// colorPickerControl1
			// 
			this.colorPickerControl1.Location = new System.Drawing.Point(8, 9);
			this.colorPickerControl1.Name = "colorPickerControl1";
			this.colorPickerControl1.Size = new System.Drawing.Size(311, 197);
			this.colorPickerControl1.TabIndex = 1;
			// 
			// SectorColorPicker
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.ClientSize = new System.Drawing.Size(327, 278);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.colorPickerControl1);
			this.DoubleBuffered = true;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SectorColorPicker";
			this.Opacity = 0;
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "SectorColorPicker";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SectorColorPicker_FormClosing);
			this.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.SectorColorPicker_HelpRequested);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private CodeImp.DoomBuilder.ColorPicker.Controls.ColorPickerControl colorPickerControl1;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.RadioButton rbFadeColor;
		private System.Windows.Forms.RadioButton rbSectorColor;
	}
}