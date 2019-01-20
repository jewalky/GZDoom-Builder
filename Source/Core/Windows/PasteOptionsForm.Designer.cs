namespace CodeImp.DoomBuilder.Windows
{
	partial class PasteOptionsForm
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
			this.paste = new System.Windows.Forms.Button();
			this.cancel = new System.Windows.Forms.Button();
			this.pasteoptions = new CodeImp.DoomBuilder.Controls.PasteOptionsControl();
			this.SuspendLayout();
			// 
			// paste
			// 
			this.paste.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.paste.Location = new System.Drawing.Point(142, 152);
			this.paste.Name = "paste";
			this.paste.Size = new System.Drawing.Size(112, 25);
			this.paste.TabIndex = 3;
			this.paste.Text = "Paste";
			this.paste.UseVisualStyleBackColor = true;
			this.paste.Click += new System.EventHandler(this.paste_Click);
			// 
			// cancel
			// 
			this.cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancel.Location = new System.Drawing.Point(260, 152);
			this.cancel.Name = "cancel";
			this.cancel.Size = new System.Drawing.Size(112, 25);
			this.cancel.TabIndex = 4;
			this.cancel.Text = "Cancel";
			this.cancel.UseVisualStyleBackColor = true;
			this.cancel.Click += new System.EventHandler(this.cancel_Click);
			// 
			// pasteoptions
			// 
			this.pasteoptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.pasteoptions.Location = new System.Drawing.Point(12, 12);
			this.pasteoptions.Name = "pasteoptions";
			this.pasteoptions.Size = new System.Drawing.Size(360, 138);
			this.pasteoptions.TabIndex = 5;
			// 
			// PasteOptionsForm
			// 
			this.AcceptButton = this.paste;
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.CancelButton = this.cancel;
			this.ClientSize = new System.Drawing.Size(384, 184);
			this.Controls.Add(this.pasteoptions);
			this.Controls.Add(this.cancel);
			this.Controls.Add(this.paste);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "PasteOptionsForm";
			this.Opacity = 0;
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Paste Special";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button paste;
		private System.Windows.Forms.Button cancel;
		private CodeImp.DoomBuilder.Controls.PasteOptionsControl pasteoptions;
	}
}