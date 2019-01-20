namespace CodeImp.DoomBuilder.Controls
{
	partial class AngleControl
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
			this.button0 = new System.Windows.Forms.RadioButton();
			this.button1 = new System.Windows.Forms.RadioButton();
			this.button2 = new System.Windows.Forms.RadioButton();
			this.button3 = new System.Windows.Forms.RadioButton();
			this.button4 = new System.Windows.Forms.RadioButton();
			this.button5 = new System.Windows.Forms.RadioButton();
			this.button6 = new System.Windows.Forms.RadioButton();
			this.button7 = new System.Windows.Forms.RadioButton();
			this.SuspendLayout();
			// 
			// button0
			// 
			this.button0.AutoSize = true;
			this.button0.BackColor = System.Drawing.Color.Transparent;
			this.button0.Location = new System.Drawing.Point(62, 33);
			this.button0.Name = "button0";
			this.button0.Padding = new System.Windows.Forms.Padding(3);
			this.button0.Size = new System.Drawing.Size(20, 19);
			this.button0.TabIndex = 0;
			this.button0.UseVisualStyleBackColor = false;
			this.button0.CheckedChanged += new System.EventHandler(this.button_CheckedChanged);
			// 
			// button1
			// 
			this.button1.AutoSize = true;
			this.button1.BackColor = System.Drawing.Color.Transparent;
			this.button1.Location = new System.Drawing.Point(54, 11);
			this.button1.Name = "button1";
			this.button1.Padding = new System.Windows.Forms.Padding(3);
			this.button1.Size = new System.Drawing.Size(20, 19);
			this.button1.TabIndex = 1;
			this.button1.UseVisualStyleBackColor = false;
			this.button1.CheckedChanged += new System.EventHandler(this.button_CheckedChanged);
			// 
			// button2
			// 
			this.button2.AutoSize = true;
			this.button2.BackColor = System.Drawing.Color.Transparent;
			this.button2.Location = new System.Drawing.Point(33, 3);
			this.button2.Name = "button2";
			this.button2.Padding = new System.Windows.Forms.Padding(3);
			this.button2.Size = new System.Drawing.Size(20, 19);
			this.button2.TabIndex = 2;
			this.button2.UseVisualStyleBackColor = false;
			this.button2.CheckedChanged += new System.EventHandler(this.button_CheckedChanged);
			// 
			// button3
			// 
			this.button3.AutoSize = true;
			this.button3.BackColor = System.Drawing.Color.Transparent;
			this.button3.Location = new System.Drawing.Point(11, 11);
			this.button3.Name = "button3";
			this.button3.Padding = new System.Windows.Forms.Padding(3);
			this.button3.Size = new System.Drawing.Size(20, 19);
			this.button3.TabIndex = 3;
			this.button3.UseVisualStyleBackColor = false;
			this.button3.CheckedChanged += new System.EventHandler(this.button_CheckedChanged);
			// 
			// button4
			// 
			this.button4.AutoSize = true;
			this.button4.BackColor = System.Drawing.Color.Transparent;
			this.button4.Location = new System.Drawing.Point(3, 33);
			this.button4.Name = "button4";
			this.button4.Padding = new System.Windows.Forms.Padding(3);
			this.button4.Size = new System.Drawing.Size(20, 19);
			this.button4.TabIndex = 4;
			this.button4.UseVisualStyleBackColor = false;
			this.button4.CheckedChanged += new System.EventHandler(this.button_CheckedChanged);
			// 
			// button5
			// 
			this.button5.AutoSize = true;
			this.button5.BackColor = System.Drawing.Color.Transparent;
			this.button5.Location = new System.Drawing.Point(11, 55);
			this.button5.Name = "button5";
			this.button5.Padding = new System.Windows.Forms.Padding(3);
			this.button5.Size = new System.Drawing.Size(20, 19);
			this.button5.TabIndex = 5;
			this.button5.UseVisualStyleBackColor = false;
			this.button5.CheckedChanged += new System.EventHandler(this.button_CheckedChanged);
			// 
			// button6
			// 
			this.button6.AutoSize = true;
			this.button6.BackColor = System.Drawing.Color.Transparent;
			this.button6.Location = new System.Drawing.Point(33, 63);
			this.button6.Name = "button6";
			this.button6.Padding = new System.Windows.Forms.Padding(3);
			this.button6.Size = new System.Drawing.Size(20, 19);
			this.button6.TabIndex = 6;
			this.button6.UseVisualStyleBackColor = false;
			this.button6.CheckedChanged += new System.EventHandler(this.button_CheckedChanged);
			// 
			// button7
			// 
			this.button7.AutoSize = true;
			this.button7.BackColor = System.Drawing.Color.Transparent;
			this.button7.Location = new System.Drawing.Point(54, 55);
			this.button7.Name = "button7";
			this.button7.Padding = new System.Windows.Forms.Padding(3);
			this.button7.Size = new System.Drawing.Size(20, 19);
			this.button7.TabIndex = 7;
			this.button7.UseVisualStyleBackColor = false;
			this.button7.CheckedChanged += new System.EventHandler(this.button_CheckedChanged);
			// 
			// AngleControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.Controls.Add(this.button7);
			this.Controls.Add(this.button6);
			this.Controls.Add(this.button5);
			this.Controls.Add(this.button4);
			this.Controls.Add(this.button3);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.button0);
			this.Name = "AngleControl";
			this.Size = new System.Drawing.Size(84, 84);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.AngleControl_Paint);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.RadioButton button0;
		private System.Windows.Forms.RadioButton button1;
		private System.Windows.Forms.RadioButton button2;
		private System.Windows.Forms.RadioButton button3;
		private System.Windows.Forms.RadioButton button4;
		private System.Windows.Forms.RadioButton button5;
		private System.Windows.Forms.RadioButton button6;
		private System.Windows.Forms.RadioButton button7;
	}
}
