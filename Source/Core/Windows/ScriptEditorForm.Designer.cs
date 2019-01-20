namespace CodeImp.DoomBuilder.Windows
{
	partial class ScriptEditorForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScriptEditorForm));
			this.editor = new CodeImp.DoomBuilder.Controls.ScriptEditorPanel();
			this.SuspendLayout();
			// 
			// editor
			// 
			this.editor.BackColor = System.Drawing.SystemColors.Control;
			this.editor.Dock = System.Windows.Forms.DockStyle.Fill;
			this.editor.Location = new System.Drawing.Point(0, 0);
			this.editor.Name = "editor";
			this.editor.Size = new System.Drawing.Size(729, 578);
			this.editor.TabIndex = 0;
			this.editor.TabStop = false;
			// 
			// ScriptEditorForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.ClientSize = new System.Drawing.Size(729, 578);
			this.Controls.Add(this.editor);
			this.DoubleBuffered = true;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "ScriptEditorForm";
			this.Opacity = 0;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Script Editor";
			this.Load += new System.EventHandler(this.ScriptEditorForm_Load);
			this.Shown += new System.EventHandler(this.ScriptEditorForm_Shown);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ScriptEditorForm_FormClosing);
			this.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.ScriptEditorForm_HelpRequested);
			this.ResumeLayout(false);

		}

		#endregion

		private CodeImp.DoomBuilder.Controls.ScriptEditorPanel editor;
	}
}