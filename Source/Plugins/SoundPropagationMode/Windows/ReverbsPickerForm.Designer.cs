namespace CodeImp.DoomBuilder.SoundPropagationMode
{
	partial class ReverbsPickerForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.list = new System.Windows.Forms.ListBox();
			this.accept = new System.Windows.Forms.Button();
			this.cancel = new System.Windows.Forms.Button();
			this.cbactiveenv = new System.Windows.Forms.CheckBox();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.list);
			this.groupBox1.Location = new System.Drawing.Point(12, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(190, 351);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = " Sound Environments: ";
			// 
			// list
			// 
			this.list.Dock = System.Windows.Forms.DockStyle.Fill;
			this.list.FormattingEnabled = true;
			this.list.Location = new System.Drawing.Point(3, 16);
			this.list.Name = "list";
			this.list.Size = new System.Drawing.Size(184, 329);
			this.list.TabIndex = 0;
			this.list.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.list_MouseDoubleClick);
			this.list.SelectedIndexChanged += new System.EventHandler(this.list_SelectedIndexChanged);
			// 
			// accept
			// 
			this.accept.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.accept.Enabled = false;
			this.accept.Location = new System.Drawing.Point(14, 394);
			this.accept.Name = "accept";
			this.accept.Size = new System.Drawing.Size(91, 23);
			this.accept.TabIndex = 0;
			this.accept.Text = "OK";
			this.accept.UseVisualStyleBackColor = true;
			this.accept.Click += new System.EventHandler(this.accept_Click);
			// 
			// cancel
			// 
			this.cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancel.Location = new System.Drawing.Point(111, 394);
			this.cancel.Name = "cancel";
			this.cancel.Size = new System.Drawing.Size(91, 23);
			this.cancel.TabIndex = 1;
			this.cancel.Text = "Cancel";
			this.cancel.UseVisualStyleBackColor = true;
			// 
			// cbactiveenv
			// 
			this.cbactiveenv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.cbactiveenv.AutoSize = true;
			this.cbactiveenv.Location = new System.Drawing.Point(15, 369);
			this.cbactiveenv.Name = "cbactiveenv";
			this.cbactiveenv.Size = new System.Drawing.Size(152, 17);
			this.cbactiveenv.TabIndex = 1;
			this.cbactiveenv.Text = "Active Sound Environment";
			this.cbactiveenv.UseVisualStyleBackColor = true;
			// 
			// ReverbsPickerForm
			// 
			this.AcceptButton = this.accept;
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.CancelButton = this.cancel;
			this.ClientSize = new System.Drawing.Size(214, 422);
			this.Controls.Add(this.cbactiveenv);
			this.Controls.Add(this.cancel);
			this.Controls.Add(this.accept);
			this.Controls.Add(this.groupBox1);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(230, 200);
			this.Name = "ReverbsPickerForm";
			this.Opacity = 0;
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Choose a Sound Environment";
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button accept;
		private System.Windows.Forms.Button cancel;
		private System.Windows.Forms.ListBox list;
		private System.Windows.Forms.CheckBox cbactiveenv;
	}
}