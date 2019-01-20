namespace CodeImp.DoomBuilder.Controls
{
	partial class PairedFieldsControl
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
			this.bLink = new System.Windows.Forms.Button();
			this.value1 = new CodeImp.DoomBuilder.Controls.ButtonsNumericTextbox();
			this.value2 = new CodeImp.DoomBuilder.Controls.ButtonsNumericTextbox();
			this.tooltip = new System.Windows.Forms.ToolTip(this.components);
			this.SuspendLayout();
			// 
			// bReset
			// 
			this.bReset.Image = global::CodeImp.DoomBuilder.Properties.Resources.Reset;
			this.bReset.Location = new System.Drawing.Point(161, 1);
			this.bReset.Name = "bReset";
			this.bReset.Size = new System.Drawing.Size(23, 23);
			this.bReset.TabIndex = 40;
			this.tooltip.SetToolTip(this.bReset, "Reset");
			this.bReset.UseVisualStyleBackColor = true;
			this.bReset.Visible = false;
			this.bReset.Click += new System.EventHandler(this.bReset_Click);
			// 
			// bLink
			// 
			this.bLink.Image = global::CodeImp.DoomBuilder.Properties.Resources.Link;
			this.bLink.Location = new System.Drawing.Point(136, 1);
			this.bLink.Name = "bLink";
			this.bLink.Size = new System.Drawing.Size(23, 23);
			this.bLink.TabIndex = 39;
			this.tooltip.SetToolTip(this.bLink, "Link values");
			this.bLink.UseVisualStyleBackColor = true;
			this.bLink.Click += new System.EventHandler(this.bLink_Click);
			// 
			// value1
			// 
			this.value1.AllowDecimal = false;
			this.value1.AllowExpressions = true;
			this.value1.AllowNegative = true;
			this.value1.AllowRelative = true;
			this.value1.ButtonStep = 1;
			this.value1.ButtonStepBig = 10F;
			this.value1.ButtonStepFloat = 1F;
			this.value1.ButtonStepSmall = 0.1F;
			this.value1.ButtonStepsUseModifierKeys = false;
			this.value1.ButtonStepsWrapAround = false;
			this.value1.Location = new System.Drawing.Point(3, 1);
			this.value1.Name = "value1";
			this.value1.Size = new System.Drawing.Size(62, 24);
			this.value1.StepValues = null;
			this.value1.TabIndex = 37;
			this.value1.Tag = "offsetx_top";
			this.value1.WhenTextChanged += new System.EventHandler(this.value1_WhenTextChanged);
			// 
			// value2
			// 
			this.value2.AllowDecimal = false;
			this.value2.AllowExpressions = true;
			this.value2.AllowNegative = true;
			this.value2.AllowRelative = true;
			this.value2.ButtonStep = 1;
			this.value2.ButtonStepBig = 10F;
			this.value2.ButtonStepFloat = 1F;
			this.value2.ButtonStepSmall = 0.1F;
			this.value2.ButtonStepsUseModifierKeys = false;
			this.value2.ButtonStepsWrapAround = false;
			this.value2.Location = new System.Drawing.Point(71, 1);
			this.value2.Name = "value2";
			this.value2.Size = new System.Drawing.Size(62, 24);
			this.value2.StepValues = null;
			this.value2.TabIndex = 38;
			this.value2.Tag = "offsety_top";
			this.value2.WhenTextChanged += new System.EventHandler(this.value2_WhenTextChanged);
			// 
			// PairedFieldsControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.Controls.Add(this.bReset);
			this.Controls.Add(this.bLink);
			this.Controls.Add(this.value1);
			this.Controls.Add(this.value2);
			this.Name = "PairedFieldsControl";
			this.Size = new System.Drawing.Size(186, 26);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button bLink;
		private CodeImp.DoomBuilder.Controls.ButtonsNumericTextbox value1;
		private CodeImp.DoomBuilder.Controls.ButtonsNumericTextbox value2;
		private System.Windows.Forms.Button bReset;
		private System.Windows.Forms.ToolTip tooltip;
	}
}
