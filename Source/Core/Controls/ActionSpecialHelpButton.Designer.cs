namespace CodeImp.DoomBuilder.Controls
{
	partial class ActionSpecialHelpButton
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if(disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			this.button = new System.Windows.Forms.Button();
			this.tooltip = new System.Windows.Forms.ToolTip(this.components);
			this.SuspendLayout();
			// 
			// button
			// 
			this.button.Dock = System.Windows.Forms.DockStyle.Fill;
			this.button.Image = global::CodeImp.DoomBuilder.Properties.Resources.Help;
			this.button.Location = new System.Drawing.Point(0, 0);
			this.button.Margin = new System.Windows.Forms.Padding(0);
			this.button.Name = "button";
			this.button.Size = new System.Drawing.Size(28, 26);
			this.button.TabIndex = 0;
			this.tooltip.SetToolTip(this.button, "Open Action Reference");
			this.button.UseVisualStyleBackColor = true;
			this.button.Click += new System.EventHandler(this.button_Click);
			// 
			// ActionSpecialHelpButton
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.Controls.Add(this.button);
			this.Name = "ActionSpecialHelpButton";
			this.Size = new System.Drawing.Size(28, 26);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button button;
		private System.Windows.Forms.ToolTip tooltip;
	}
}
