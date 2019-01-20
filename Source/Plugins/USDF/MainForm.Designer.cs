namespace CodeImp.DoomBuilder.USDF
{
	partial class MainForm
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.tree = new System.Windows.Forms.TreeView();
			this.treeimages = new System.Windows.Forms.ImageList(this.components);
			this.SuspendLayout();
			// 
			// tree
			// 
			this.tree.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)));
			this.tree.HideSelection = false;
			this.tree.ImageIndex = 0;
			this.tree.ImageList = this.treeimages;
			this.tree.Indent = 22;
			this.tree.ItemHeight = 18;
			this.tree.Location = new System.Drawing.Point(12, 12);
			this.tree.Name = "tree";
			this.tree.PathSeparator = ".";
			this.tree.SelectedImageIndex = 0;
			this.tree.Size = new System.Drawing.Size(257, 588);
			this.tree.TabIndex = 0;
			// 
			// treeimages
			// 
			this.treeimages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("treeimages.ImageStream")));
			this.treeimages.TransparentColor = System.Drawing.Color.Transparent;
			this.treeimages.Images.SetKeyName(0, "Dialog2.png");
			this.treeimages.Images.SetKeyName(1, "book_closed.png");
			this.treeimages.Images.SetKeyName(2, "book_open.png");
			this.treeimages.Images.SetKeyName(3, "page_user.png");
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.ClientSize = new System.Drawing.Size(942, 612);
			this.Controls.Add(this.tree);
			this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "MainForm";
			this.Opacity = 0;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Dialog Editor";
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.Move += new System.EventHandler(this.MainForm_Move);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
			this.ResizeEnd += new System.EventHandler(this.MainForm_ResizeEnd);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TreeView tree;
		private System.Windows.Forms.ImageList treeimages;

	}
}