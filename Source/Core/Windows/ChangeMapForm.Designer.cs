namespace CodeImp.DoomBuilder.Windows
{
	partial class ChangeMapForm
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
			System.Windows.Forms.Label label2;
			System.Windows.Forms.ColumnHeader columnHeader1;
			this.mapslist = new System.Windows.Forms.ListView();
			this.cancel = new System.Windows.Forms.Button();
			this.apply = new System.Windows.Forms.Button();
			label2 = new System.Windows.Forms.Label();
			columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.SuspendLayout();
			// 
			// label2
			// 
			label2.Location = new System.Drawing.Point(12, 9);
			label2.Name = "label2";
			label2.Size = new System.Drawing.Size(396, 17);
			label2.TabIndex = 17;
			label2.Text = "Please select the map to load for editing.";
			// 
			// columnHeader1
			// 
			columnHeader1.Text = "Map name";
			columnHeader1.Width = -1;
			// 
			// mapslist
			// 
			this.mapslist.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.mapslist.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            columnHeader1});
			this.mapslist.FullRowSelect = true;
			this.mapslist.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.mapslist.HideSelection = false;
			this.mapslist.LabelWrap = false;
			this.mapslist.Location = new System.Drawing.Point(12, 29);
			this.mapslist.MultiSelect = false;
			this.mapslist.Name = "mapslist";
			this.mapslist.ShowGroups = false;
			this.mapslist.Size = new System.Drawing.Size(396, 125);
			this.mapslist.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.mapslist.TabIndex = 1;
			this.mapslist.UseCompatibleStateImageBehavior = false;
			this.mapslist.View = System.Windows.Forms.View.List;
			this.mapslist.DoubleClick += new System.EventHandler(this.mapslist_DoubleClick);
			// 
			// cancel
			// 
			this.cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancel.Location = new System.Drawing.Point(296, 161);
			this.cancel.Name = "cancel";
			this.cancel.Size = new System.Drawing.Size(112, 25);
			this.cancel.TabIndex = 2;
			this.cancel.Text = "Cancel";
			this.cancel.UseVisualStyleBackColor = true;
			this.cancel.Click += new System.EventHandler(this.cancel_Click);
			// 
			// apply
			// 
			this.apply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.apply.Location = new System.Drawing.Point(178, 161);
			this.apply.Name = "apply";
			this.apply.Size = new System.Drawing.Size(112, 25);
			this.apply.TabIndex = 3;
			this.apply.Text = "OK";
			this.apply.UseVisualStyleBackColor = true;
			this.apply.Click += new System.EventHandler(this.apply_Click);
			// 
			// ChangeMapForm
			// 
			this.AcceptButton = this.apply;
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.CancelButton = this.cancel;
			this.ClientSize = new System.Drawing.Size(420, 192);
			this.Controls.Add(this.cancel);
			this.Controls.Add(this.apply);
			this.Controls.Add(this.mapslist);
			this.Controls.Add(label2);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ChangeMapForm";
			this.Opacity = 0;
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Change Map";
			this.Shown += new System.EventHandler(this.ChangeMapForm_Shown);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListView mapslist;
		private System.Windows.Forms.Button cancel;
		private System.Windows.Forms.Button apply;
	}
}