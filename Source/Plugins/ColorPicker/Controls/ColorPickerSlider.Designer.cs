namespace CodeImp.DoomBuilder.ColorPicker.Controls {
	partial class ColorPickerSlider {
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

		#region Код, автоматически созданный конструктором компонентов

		/// <summary> 
		/// Обязательный метод для поддержки конструктора - не изменяйте 
		/// содержимое данного метода при помощи редактора кода.
		/// </summary>
		private void InitializeComponent() {
			this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
			this.trackBar1 = new System.Windows.Forms.TrackBar();
			this.label1 = new System.Windows.Forms.Label();
			this.labelMin = new System.Windows.Forms.Label();
			this.labelMax = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
			this.SuspendLayout();
			// 
			// numericUpDown1
			// 
			this.numericUpDown1.Location = new System.Drawing.Point(263, 16);
			this.numericUpDown1.Name = "numericUpDown1";
			this.numericUpDown1.Size = new System.Drawing.Size(44, 20);
			this.numericUpDown1.TabIndex = 5;
			this.numericUpDown1.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
			// 
			// trackBar1
			// 
			this.trackBar1.LargeChange = 32;
			this.trackBar1.Location = new System.Drawing.Point(115, 7);
			this.trackBar1.Maximum = 512;
			this.trackBar1.Name = "trackBar1";
			this.trackBar1.Size = new System.Drawing.Size(142, 45);
			this.trackBar1.SmallChange = 4;
			this.trackBar1.TabIndex = 4;
			this.trackBar1.TickFrequency = 16;
			this.trackBar1.TickStyle = System.Windows.Forms.TickStyle.Both;
			this.trackBar1.ValueChanged += new System.EventHandler(this.trackBar1_ValueChanged);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(3, 17);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(106, 14);
			this.label1.TabIndex = 3;
			this.label1.Text = "Secondary intensity:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelMin
			// 
			this.labelMin.Location = new System.Drawing.Point(111, -1);
			this.labelMin.Name = "labelMin";
			this.labelMin.Size = new System.Drawing.Size(36, 15);
			this.labelMin.TabIndex = 6;
			this.labelMin.Text = "0";
			this.labelMin.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// labelMax
			// 
			this.labelMax.Location = new System.Drawing.Point(231, -1);
			this.labelMax.Name = "labelMax";
			this.labelMax.Size = new System.Drawing.Size(36, 15);
			this.labelMax.TabIndex = 7;
			this.labelMax.Text = "512";
			this.labelMax.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// ColorPickerSlider
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.Controls.Add(this.labelMax);
			this.Controls.Add(this.labelMin);
			this.Controls.Add(this.numericUpDown1);
			this.Controls.Add(this.trackBar1);
			this.Controls.Add(this.label1);
			this.Name = "ColorPickerSlider";
			this.Size = new System.Drawing.Size(311, 45);
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.NumericUpDown numericUpDown1;
		private System.Windows.Forms.TrackBar trackBar1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label labelMin;
		private System.Windows.Forms.Label labelMax;
	}
}
