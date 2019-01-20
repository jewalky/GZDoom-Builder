namespace CodeImp.DoomBuilder.BuilderEffects
{
	partial class DirectionalShadingForm
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
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.sunangletb = new CodeImp.DoomBuilder.Controls.ButtonsNumericTextbox();
			this.sunangle = new CodeImp.DoomBuilder.Controls.AngleControlEx();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.lightamount = new IntControl();
			this.lightcolor = new CodeImp.DoomBuilder.Controls.ColorFieldsControl();
			this.cancel = new System.Windows.Forms.Button();
			this.apply = new System.Windows.Forms.Button();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.shadeamount = new IntControl();
			this.shadecolor = new CodeImp.DoomBuilder.Controls.ColorFieldsControl();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.sunangletb);
			this.groupBox1.Controls.Add(this.sunangle);
			this.groupBox1.Location = new System.Drawing.Point(12, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(91, 154);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = " Sun angle ";
			// 
			// sunangletb
			// 
			this.sunangletb.AllowDecimal = false;
			this.sunangletb.AllowExpressions = true;
			this.sunangletb.AllowNegative = true;
			this.sunangletb.AllowRelative = true;
			this.sunangletb.ButtonStep = 5;
			this.sunangletb.ButtonStepBig = 15F;
			this.sunangletb.ButtonStepFloat = 1F;
			this.sunangletb.ButtonStepSmall = 1F;
			this.sunangletb.ButtonStepsUseModifierKeys = true;
			this.sunangletb.ButtonStepsWrapAround = false;
			this.sunangletb.Location = new System.Drawing.Point(6, 100);
			this.sunangletb.Name = "sunangletb";
			this.sunangletb.Size = new System.Drawing.Size(76, 24);
			this.sunangletb.StepValues = null;
			this.sunangletb.TabIndex = 22;
			this.sunangletb.WhenTextChanged += new System.EventHandler(this.sunangletb_WhenTextChanged);
			// 
			// sunangle
			// 
			this.sunangle.Angle = 0;
			this.sunangle.AngleOffset = 0;
			this.sunangle.DoomAngleClamping = false;
			this.sunangle.Location = new System.Drawing.Point(6, 18);
			this.sunangle.Name = "sunangle";
			this.sunangle.Size = new System.Drawing.Size(76, 76);
			this.sunangle.TabIndex = 21;
			this.sunangle.AngleChanged += new System.EventHandler(this.sunangle_AngleChanged);
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.lightamount);
			this.groupBox2.Controls.Add(this.lightcolor);
			this.groupBox2.Location = new System.Drawing.Point(109, 12);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(293, 74);
			this.groupBox2.TabIndex = 1;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = " Light";
			// 
			// lightamount
			// 
			this.lightamount.AllowNegative = false;
			this.lightamount.ExtendedLimits = false;
			this.lightamount.Label = "Amount:";
			this.lightamount.Location = new System.Drawing.Point(6, 15);
			this.lightamount.Maximum = 255;
			this.lightamount.Minimum = 0;
			this.lightamount.Name = "lightamount";
			this.lightamount.Size = new System.Drawing.Size(281, 24);
			this.lightamount.TabIndex = 28;
			this.lightamount.Value = 0;
			this.lightamount.OnValueChanged += new System.EventHandler(this.OnShadingChanged);
			this.lightamount.OnValueChanging += new System.EventHandler(this.OnShadingChanged);
			// 
			// lightcolor
			// 
			this.lightcolor.DefaultValue = 16777215;
			this.lightcolor.Field = "";
			this.lightcolor.Label = "Color:";
			this.lightcolor.Location = new System.Drawing.Point(33, 41);
			this.lightcolor.Name = "lightcolor";
			this.lightcolor.Size = new System.Drawing.Size(207, 31);
			this.lightcolor.TabIndex = 27;
			this.lightcolor.OnValueChanged += new System.EventHandler(this.OnShadingChanged);
			// 
			// cancel
			// 
			this.cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancel.Location = new System.Drawing.Point(312, 176);
			this.cancel.Name = "cancel";
			this.cancel.Size = new System.Drawing.Size(90, 29);
			this.cancel.TabIndex = 2;
			this.cancel.Text = "Cancel";
			this.cancel.UseVisualStyleBackColor = true;
			this.cancel.Click += new System.EventHandler(this.cancel_Click);
			// 
			// apply
			// 
			this.apply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.apply.Location = new System.Drawing.Point(216, 176);
			this.apply.Name = "apply";
			this.apply.Size = new System.Drawing.Size(90, 29);
			this.apply.TabIndex = 3;
			this.apply.Text = "Apply";
			this.apply.UseVisualStyleBackColor = true;
			this.apply.Click += new System.EventHandler(this.apply_Click);
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.shadeamount);
			this.groupBox3.Controls.Add(this.shadecolor);
			this.groupBox3.Location = new System.Drawing.Point(109, 92);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(293, 74);
			this.groupBox3.TabIndex = 29;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = " Shade";
			// 
			// shadeamount
			// 
			this.shadeamount.AllowNegative = false;
			this.shadeamount.ExtendedLimits = false;
			this.shadeamount.Label = "Amount:";
			this.shadeamount.Location = new System.Drawing.Point(6, 15);
			this.shadeamount.Maximum = 255;
			this.shadeamount.Minimum = 0;
			this.shadeamount.Name = "shadeamount";
			this.shadeamount.Size = new System.Drawing.Size(281, 24);
			this.shadeamount.TabIndex = 28;
			this.shadeamount.Value = 0;
			this.shadeamount.OnValueChanged += new System.EventHandler(this.OnShadingChanged);
			this.shadeamount.OnValueChanging += new System.EventHandler(this.OnShadingChanged);
			// 
			// shadecolor
			// 
			this.shadecolor.DefaultValue = 16777215;
			this.shadecolor.Field = "";
			this.shadecolor.Label = "Color:";
			this.shadecolor.Location = new System.Drawing.Point(33, 41);
			this.shadecolor.Name = "shadecolor";
			this.shadecolor.Size = new System.Drawing.Size(207, 31);
			this.shadecolor.TabIndex = 27;
			this.shadecolor.OnValueChanged += new System.EventHandler(this.OnShadingChanged);
			// 
			// DirectionalShadingForm
			// 
			this.AcceptButton = this.apply;
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.CancelButton = this.cancel;
			this.ClientSize = new System.Drawing.Size(414, 211);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.apply);
			this.Controls.Add(this.cancel);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "DirectionalShadingForm";
			this.ShowInTaskbar = false;
			this.Text = "Directional Shading";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DirectionalShadingForm_FormClosing);
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox groupBox1;
		private CodeImp.DoomBuilder.Controls.AngleControlEx sunangle;
		private CodeImp.DoomBuilder.Controls.ButtonsNumericTextbox sunangletb;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Button cancel;
		private System.Windows.Forms.Button apply;
		private CodeImp.DoomBuilder.Controls.ColorFieldsControl lightcolor;
		private System.Windows.Forms.GroupBox groupBox3;
		private CodeImp.DoomBuilder.Controls.ColorFieldsControl shadecolor;
		private IntControl lightamount;
		private IntControl shadeamount;
	}
}