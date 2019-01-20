namespace CodeImp.DoomBuilder.Windows
{
	partial class BitFlagsAndOptionsForm
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
			this.cancel = new System.Windows.Forms.Button();
			this.apply = new System.Windows.Forms.Button();
			this.groupoptions = new System.Windows.Forms.GroupBox();
			this.options = new CodeImp.DoomBuilder.Controls.CheckboxArrayControl();
			this.groupflags = new System.Windows.Forms.GroupBox();
			this.flags = new CodeImp.DoomBuilder.Controls.CheckboxArrayControl();
			this.groupoptions.SuspendLayout();
			this.groupflags.SuspendLayout();
			this.SuspendLayout();
			// 
			// cancel
			// 
			this.cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancel.Location = new System.Drawing.Point(105, 425);
			this.cancel.Name = "cancel";
			this.cancel.Size = new System.Drawing.Size(91, 25);
			this.cancel.TabIndex = 4;
			this.cancel.Text = "Cancel";
			this.cancel.UseVisualStyleBackColor = true;
			this.cancel.Click += new System.EventHandler(this.cancel_Click);
			// 
			// apply
			// 
			this.apply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.apply.Location = new System.Drawing.Point(11, 425);
			this.apply.Name = "apply";
			this.apply.Size = new System.Drawing.Size(91, 25);
			this.apply.TabIndex = 3;
			this.apply.Text = "OK";
			this.apply.UseVisualStyleBackColor = true;
			this.apply.Click += new System.EventHandler(this.apply_Click);
			// 
			// groupoptions
			// 
			this.groupoptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.groupoptions.Controls.Add(this.options);
			this.groupoptions.Location = new System.Drawing.Point(12, 12);
			this.groupoptions.Name = "groupoptions";
			this.groupoptions.Size = new System.Drawing.Size(184, 200);
			this.groupoptions.TabIndex = 5;
			this.groupoptions.TabStop = false;
			this.groupoptions.Text = " Options ";
			// 
			// options
			// 
			this.options.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.options.AutoScroll = true;
			this.options.Columns = 1;
			this.options.Location = new System.Drawing.Point(6, 19);
			this.options.Name = "options";
			this.options.Size = new System.Drawing.Size(172, 175);
			this.options.TabIndex = 1;
			this.options.VerticalSpacing = 1;
			// 
			// groupflags
			// 
			this.groupflags.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.groupflags.Controls.Add(this.flags);
			this.groupflags.Location = new System.Drawing.Point(12, 218);
			this.groupflags.Name = "groupflags";
			this.groupflags.Size = new System.Drawing.Size(184, 200);
			this.groupflags.TabIndex = 6;
			this.groupflags.TabStop = false;
			this.groupflags.Text = " Flags ";
			// 
			// flags
			// 
			this.flags.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.flags.AutoScroll = true;
			this.flags.Columns = 1;
			this.flags.Location = new System.Drawing.Point(6, 19);
			this.flags.Name = "flags";
			this.flags.Size = new System.Drawing.Size(172, 175);
			this.flags.TabIndex = 1;
			this.flags.VerticalSpacing = 1;
			// 
			// BitFlagsAndOptionsForm
			// 
			this.AcceptButton = this.apply;
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.CancelButton = this.cancel;
			this.ClientSize = new System.Drawing.Size(208, 456);
			this.Controls.Add(this.groupflags);
			this.Controls.Add(this.groupoptions);
			this.Controls.Add(this.cancel);
			this.Controls.Add(this.apply);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "BitFlagsAndOptionsForm";
			this.Opacity = 0;
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Options";
			this.groupoptions.ResumeLayout(false);
			this.groupflags.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button cancel;
		private System.Windows.Forms.Button apply;
		private System.Windows.Forms.GroupBox groupoptions;
		private CodeImp.DoomBuilder.Controls.CheckboxArrayControl options;
		private System.Windows.Forms.GroupBox groupflags;
		private CodeImp.DoomBuilder.Controls.CheckboxArrayControl flags;
	}
}