namespace CodeImp.DoomBuilder.UDBScript
{
	partial class MessageForm
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
			if (disposing && (components != null))
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
			this.btnButton1 = new System.Windows.Forms.Button();
			this.btnButton2 = new System.Windows.Forms.Button();
			this.btnAbortScript = new System.Windows.Forms.Button();
			this.tbMessage = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// btnButton1
			// 
			this.btnButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnButton1.Location = new System.Drawing.Point(341, 210);
			this.btnButton1.Name = "btnButton1";
			this.btnButton1.Size = new System.Drawing.Size(75, 23);
			this.btnButton1.TabIndex = 0;
			this.btnButton1.Text = "button1";
			this.btnButton1.UseVisualStyleBackColor = true;
			this.btnButton1.Click += new System.EventHandler(this.button_Click);
			// 
			// btnButton2
			// 
			this.btnButton2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnButton2.Location = new System.Drawing.Point(260, 210);
			this.btnButton2.Name = "btnButton2";
			this.btnButton2.Size = new System.Drawing.Size(75, 23);
			this.btnButton2.TabIndex = 1;
			this.btnButton2.Text = "button2";
			this.btnButton2.UseVisualStyleBackColor = true;
			this.btnButton2.Click += new System.EventHandler(this.button_Click);
			// 
			// btnAbortScript
			// 
			this.btnAbortScript.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnAbortScript.DialogResult = System.Windows.Forms.DialogResult.Abort;
			this.btnAbortScript.Location = new System.Drawing.Point(12, 210);
			this.btnAbortScript.Name = "btnAbortScript";
			this.btnAbortScript.Size = new System.Drawing.Size(75, 23);
			this.btnAbortScript.TabIndex = 2;
			this.btnAbortScript.Text = "Abort script";
			this.btnAbortScript.UseVisualStyleBackColor = true;
			this.btnAbortScript.Click += new System.EventHandler(this.button_Click);
			// 
			// tbMessage
			// 
			this.tbMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tbMessage.Location = new System.Drawing.Point(12, 12);
			this.tbMessage.Multiline = true;
			this.tbMessage.Name = "tbMessage";
			this.tbMessage.ReadOnly = true;
			this.tbMessage.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.tbMessage.Size = new System.Drawing.Size(404, 192);
			this.tbMessage.TabIndex = 3;
			// 
			// MessageForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(428, 245);
			this.Controls.Add(this.tbMessage);
			this.Controls.Add(this.btnAbortScript);
			this.Controls.Add(this.btnButton2);
			this.Controls.Add(this.btnButton1);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MessageForm";
			this.ShowIcon = false;
			this.Text = "Script Message";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnButton1;
		private System.Windows.Forms.Button btnButton2;
		private System.Windows.Forms.Button btnAbortScript;
		private System.Windows.Forms.TextBox tbMessage;
	}
}