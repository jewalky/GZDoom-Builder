namespace CodeImp.DoomBuilder.Controls
{
	partial class PasteOptionsControl
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
			this.tagsgroup = new System.Windows.Forms.GroupBox();
			this.removeactions = new System.Windows.Forms.CheckBox();
			this.removetags = new System.Windows.Forms.RadioButton();
			this.renumbertags = new System.Windows.Forms.RadioButton();
			this.keeptags = new System.Windows.Forms.RadioButton();
			this.tagsgroup.SuspendLayout();
			this.SuspendLayout();
			// 
			// tagsgroup
			// 
			this.tagsgroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tagsgroup.Controls.Add(this.removeactions);
			this.tagsgroup.Controls.Add(this.removetags);
			this.tagsgroup.Controls.Add(this.renumbertags);
			this.tagsgroup.Controls.Add(this.keeptags);
			this.tagsgroup.Location = new System.Drawing.Point(0, 0);
			this.tagsgroup.Name = "tagsgroup";
			this.tagsgroup.Size = new System.Drawing.Size(443, 132);
			this.tagsgroup.TabIndex = 2;
			this.tagsgroup.TabStop = false;
			this.tagsgroup.Text = " Tags and Actions ";
			// 
			// removeactions
			// 
			this.removeactions.AutoSize = true;
			this.removeactions.Location = new System.Drawing.Point(30, 107);
			this.removeactions.Name = "removeactions";
			this.removeactions.Size = new System.Drawing.Size(116, 17);
			this.removeactions.TabIndex = 3;
			this.removeactions.Text = "Remove all actions";
			this.removeactions.UseVisualStyleBackColor = true;
			// 
			// removetags
			// 
			this.removetags.AutoSize = true;
			this.removetags.Location = new System.Drawing.Point(30, 71);
			this.removetags.Name = "removetags";
			this.removetags.Size = new System.Drawing.Size(101, 17);
			this.removetags.TabIndex = 2;
			this.removetags.TabStop = true;
			this.removetags.Text = "Remove all tags";
			this.removetags.UseVisualStyleBackColor = true;
			// 
			// renumbertags
			// 
			this.renumbertags.AutoSize = true;
			this.renumbertags.Location = new System.Drawing.Point(30, 47);
			this.renumbertags.Name = "renumbertags";
			this.renumbertags.Size = new System.Drawing.Size(271, 17);
			this.renumbertags.TabIndex = 1;
			this.renumbertags.TabStop = true;
			this.renumbertags.Text = "Renumber tags to resolve conflicts with existing tags";
			this.renumbertags.UseVisualStyleBackColor = true;
			// 
			// keeptags
			// 
			this.keeptags.AutoSize = true;
			this.keeptags.Location = new System.Drawing.Point(30, 24);
			this.keeptags.Name = "keeptags";
			this.keeptags.Size = new System.Drawing.Size(217, 17);
			this.keeptags.TabIndex = 0;
			this.keeptags.TabStop = true;
			this.keeptags.Text = "Keep tags the same as they were copied";
			this.keeptags.UseVisualStyleBackColor = true;
			// 
			// PasteOptionsControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.Controls.Add(this.tagsgroup);
			this.Name = "PasteOptionsControl";
			this.Size = new System.Drawing.Size(443, 139);
			this.tagsgroup.ResumeLayout(false);
			this.tagsgroup.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox tagsgroup;
		private System.Windows.Forms.CheckBox removeactions;
		private System.Windows.Forms.RadioButton removetags;
		private System.Windows.Forms.RadioButton renumbertags;
		private System.Windows.Forms.RadioButton keeptags;
	}
}
