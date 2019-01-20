namespace CodeImp.DoomBuilder.Controls
{
	partial class ScriptResourcesControl
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
			this.filterproject = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.filterprojectclear = new System.Windows.Forms.Button();
			this.filterbytype = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.projecttree = new CodeImp.DoomBuilder.Controls.BufferedTreeView();
			this.SuspendLayout();
			// 
			// filterproject
			// 
			this.filterproject.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.filterproject.Location = new System.Drawing.Point(76, 3);
			this.filterproject.Name = "filterproject";
			this.filterproject.Size = new System.Drawing.Size(190, 20);
			this.filterproject.TabIndex = 5;
			this.filterproject.TextChanged += new System.EventHandler(this.filterproject_TextChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(37, 6);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(32, 13);
			this.label1.TabIndex = 4;
			this.label1.Text = "Filter:";
			// 
			// filterprojectclear
			// 
			this.filterprojectclear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.filterprojectclear.Image = global::CodeImp.DoomBuilder.Properties.Resources.SearchClear;
			this.filterprojectclear.Location = new System.Drawing.Point(271, 1);
			this.filterprojectclear.Name = "filterprojectclear";
			this.filterprojectclear.Size = new System.Drawing.Size(25, 24);
			this.filterprojectclear.TabIndex = 6;
			this.filterprojectclear.UseVisualStyleBackColor = true;
			this.filterprojectclear.Click += new System.EventHandler(this.filterprojectclear_Click);
			// 
			// filterbytype
			// 
			this.filterbytype.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.filterbytype.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.filterbytype.FormattingEnabled = true;
			this.filterbytype.Location = new System.Drawing.Point(75, 29);
			this.filterbytype.Name = "filterbytype";
			this.filterbytype.Size = new System.Drawing.Size(221, 21);
			this.filterbytype.TabIndex = 8;
			this.filterbytype.SelectedIndexChanged += new System.EventHandler(this.filterbytype_SelectedIndexChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(9, 33);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(60, 13);
			this.label2.TabIndex = 9;
			this.label2.Text = "Script type:";
			// 
			// projecttree
			// 
			this.projecttree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.projecttree.HideSelection = false;
			this.projecttree.Location = new System.Drawing.Point(3, 56);
			this.projecttree.Name = "projecttree";
			this.projecttree.ShowNodeToolTips = true;
			this.projecttree.Size = new System.Drawing.Size(293, 494);
			this.projecttree.TabIndex = 7;
			this.projecttree.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.projecttree_NodeMouseDoubleClick);
			this.projecttree.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.projecttree_BeforeExpand);
			this.projecttree.BeforeCollapse += new System.Windows.Forms.TreeViewCancelEventHandler(this.projecttree_BeforeCollapse);
			this.projecttree.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.projecttree_NodeMouseClick);
			// 
			// ScriptResourcesControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.Controls.Add(this.label2);
			this.Controls.Add(this.filterbytype);
			this.Controls.Add(this.projecttree);
			this.Controls.Add(this.filterprojectclear);
			this.Controls.Add(this.filterproject);
			this.Controls.Add(this.label1);
			this.Margin = new System.Windows.Forms.Padding(0);
			this.Name = "ScriptResourcesControl";
			this.Size = new System.Drawing.Size(299, 553);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CodeImp.DoomBuilder.Controls.BufferedTreeView projecttree;
		private System.Windows.Forms.Button filterprojectclear;
		private System.Windows.Forms.TextBox filterproject;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox filterbytype;
		private System.Windows.Forms.Label label2;
	}
}
