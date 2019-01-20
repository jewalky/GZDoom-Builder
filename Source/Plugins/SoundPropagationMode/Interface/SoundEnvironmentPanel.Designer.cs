namespace CodeImp.DoomBuilder.SoundPropagationMode
{
	partial class SoundEnvironmentPanel
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
			this.showwarningsonly = new System.Windows.Forms.CheckBox();
			this.soundenvironments = new CodeImp.DoomBuilder.Controls.BufferedTreeView();
			this.SuspendLayout();
			// 
			// showwarningsonly
			// 
			this.showwarningsonly.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.showwarningsonly.AutoSize = true;
			this.showwarningsonly.Location = new System.Drawing.Point(3, 492);
			this.showwarningsonly.Name = "showwarningsonly";
			this.showwarningsonly.Size = new System.Drawing.Size(174, 17);
			this.showwarningsonly.TabIndex = 1;
			this.showwarningsonly.Text = "Show nodes with warnings only";
			this.showwarningsonly.UseVisualStyleBackColor = true;
			this.showwarningsonly.CheckedChanged += new System.EventHandler(this.showwarningsonly_CheckedChanged);
			// 
			// soundenvironments
			// 
			this.soundenvironments.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.soundenvironments.HideSelection = false;
			this.soundenvironments.Location = new System.Drawing.Point(0, 0);
			this.soundenvironments.Name = "soundenvironments";
			this.soundenvironments.ShowNodeToolTips = true;
			this.soundenvironments.Size = new System.Drawing.Size(214, 486);
			this.soundenvironments.TabIndex = 0;
			this.soundenvironments.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.soundenvironments_NodeMouseClick);
			this.soundenvironments.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.soundenvironments_BeforeSelect);
			// 
			// SoundEnvironmentPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.Controls.Add(this.showwarningsonly);
			this.Controls.Add(this.soundenvironments);
			this.Name = "SoundEnvironmentPanel";
			this.Size = new System.Drawing.Size(214, 512);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CodeImp.DoomBuilder.Controls.BufferedTreeView soundenvironments;
		private System.Windows.Forms.CheckBox showwarningsonly;


	}
}
