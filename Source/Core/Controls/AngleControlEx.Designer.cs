namespace CodeImp.DoomBuilder.Controls
{
	partial class AngleControlEx
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
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.SuspendLayout();
			// 
			// AngleControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.Name = "AngleControl";
			this.Size = new System.Drawing.Size(40, 40);
			this.toolTip.SetToolTip(this, "Left-click (and drag) to set angle snapped to 45-degree increment.\r\nRight-click (" +
					"and drag) to set precise angle.");
			this.Load += new System.EventHandler(this.AngleSelector_Load);
			this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.AngleSelector_MouseMove);
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.AngleSelector_MouseDown);
			this.SizeChanged += new System.EventHandler(this.AngleSelector_SizeChanged);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ToolTip toolTip;
	}
}
