namespace CodeImp.DoomBuilder.Controls
{
	partial class ButtonsNumericTextbox
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
			this.buttons = new System.Windows.Forms.VScrollBar();
			this.tooltip = new System.Windows.Forms.ToolTip(this.components);
			this.textbox = new CodeImp.DoomBuilder.Controls.NumericTextbox();
			this.SuspendLayout();
			// 
			// buttons
			// 
			this.buttons.LargeChange = 10000;
			this.buttons.Location = new System.Drawing.Point(163, 0);
			this.buttons.Maximum = 10000;
			this.buttons.Minimum = -10000;
			this.buttons.Name = "buttons";
			this.buttons.Size = new System.Drawing.Size(18, 24);
			this.buttons.TabIndex = 1;
			this.buttons.ValueChanged += new System.EventHandler(this.buttons_ValueChanged);
			// 
			// textbox
			// 
			this.textbox.AllowDecimal = false;
			this.textbox.AllowNegative = false;
			this.textbox.AllowRelative = false;
			this.textbox.ForeColor = System.Drawing.SystemColors.WindowText;
			this.textbox.ImeMode = System.Windows.Forms.ImeMode.Off;
			this.textbox.Location = new System.Drawing.Point(0, 2);
			this.textbox.Name = "textbox";
			this.textbox.Size = new System.Drawing.Size(160, 20);
			this.textbox.TabIndex = 0;
			this.textbox.TextChanged += new System.EventHandler(this.textbox_TextChanged);
			this.textbox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textbox_KeyDown);
			// 
			// ButtonsNumericTextbox
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.Controls.Add(this.buttons);
			this.Controls.Add(this.textbox);
			this.Name = "ButtonsNumericTextbox";
			this.Size = new System.Drawing.Size(289, 68);
			this.Layout += new System.Windows.Forms.LayoutEventHandler(this.ClickableNumericTextbox_Layout);
			this.Resize += new System.EventHandler(this.ClickableNumericTextbox_Resize);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private NumericTextbox textbox;
		private System.Windows.Forms.VScrollBar buttons;
		private System.Windows.Forms.ToolTip tooltip;
	}
}
