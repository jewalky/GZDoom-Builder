namespace CodeImp.DoomBuilder.ColorPicker.Windows {
	partial class LightColorPicker {
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
			this.colorPickerControl1 = new CodeImp.DoomBuilder.ColorPicker.Controls.ColorPickerControl();
			this.cbRelativeMode = new System.Windows.Forms.CheckBox();
			this.colorPickerSlider1 = new CodeImp.DoomBuilder.ColorPicker.Controls.ColorPickerSlider();
			this.colorPickerSlider2 = new CodeImp.DoomBuilder.ColorPicker.Controls.ColorPickerSlider();
			this.colorPickerSlider3 = new CodeImp.DoomBuilder.ColorPicker.Controls.ColorPickerSlider();
			this.SuspendLayout();
			// 
			// colorPickerControl1
			// 
			this.colorPickerControl1.Location = new System.Drawing.Point(8, 8);
			this.colorPickerControl1.Name = "colorPickerControl1";
			this.colorPickerControl1.Size = new System.Drawing.Size(311, 183);
			this.colorPickerControl1.TabIndex = 0;
			// 
			// cbRelativeMode
			// 
			this.cbRelativeMode.AutoSize = true;
			this.cbRelativeMode.Location = new System.Drawing.Point(14, 209);
			this.cbRelativeMode.Name = "cbRelativeMode";
			this.cbRelativeMode.Size = new System.Drawing.Size(95, 17);
			this.cbRelativeMode.TabIndex = 5;
			this.cbRelativeMode.Text = "Relative Mode";
			this.cbRelativeMode.UseVisualStyleBackColor = true;
			// 
			// colorPickerSlider1
			// 
			this.colorPickerSlider1.Location = new System.Drawing.Point(8, 233);
			this.colorPickerSlider1.Name = "colorPickerSlider1";
			this.colorPickerSlider1.ShowLimits = true;
			this.colorPickerSlider1.Size = new System.Drawing.Size(311, 45);
			this.colorPickerSlider1.TabIndex = 6;
			this.colorPickerSlider1.Value = 0;
			// 
			// colorPickerSlider2
			// 
			this.colorPickerSlider2.Location = new System.Drawing.Point(8, 284);
			this.colorPickerSlider2.Name = "colorPickerSlider2";
			this.colorPickerSlider2.ShowLimits = false;
			this.colorPickerSlider2.Size = new System.Drawing.Size(311, 48);
			this.colorPickerSlider2.TabIndex = 7;
			this.colorPickerSlider2.Value = 0;
			// 
			// colorPickerSlider3
			// 
			this.colorPickerSlider3.Location = new System.Drawing.Point(8, 338);
			this.colorPickerSlider3.Name = "colorPickerSlider3";
			this.colorPickerSlider3.ShowLimits = true;
			this.colorPickerSlider3.Size = new System.Drawing.Size(311, 48);
			this.colorPickerSlider3.TabIndex = 8;
			this.colorPickerSlider3.Value = 0;
			// 
			// LightColorPicker
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.ClientSize = new System.Drawing.Size(327, 391);
			this.Controls.Add(this.colorPickerSlider3);
			this.Controls.Add(this.colorPickerSlider2);
			this.Controls.Add(this.colorPickerSlider1);
			this.Controls.Add(this.cbRelativeMode);
			this.Controls.Add(this.colorPickerControl1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "LightColorPicker";
			this.Opacity = 0;
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "LightColorPicker";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.LightColorPicker_FormClosing);
			this.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.LightColorPicker_HelpRequested);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CodeImp.DoomBuilder.ColorPicker.Controls.ColorPickerControl colorPickerControl1;
		private System.Windows.Forms.CheckBox cbRelativeMode;
		private CodeImp.DoomBuilder.ColorPicker.Controls.ColorPickerSlider colorPickerSlider1;
		private CodeImp.DoomBuilder.ColorPicker.Controls.ColorPickerSlider colorPickerSlider2;
		private CodeImp.DoomBuilder.ColorPicker.Controls.ColorPickerSlider colorPickerSlider3;

	}
}