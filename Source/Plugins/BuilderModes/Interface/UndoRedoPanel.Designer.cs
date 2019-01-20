namespace CodeImp.DoomBuilder.BuilderModes
{
	partial class UndoRedoPanel
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
			this.list = new CodeImp.DoomBuilder.Controls.OptimizedListView();
			this.coldescription = new System.Windows.Forms.ColumnHeader();
			this.SuspendLayout();
			// 
			// list
			// 
			this.list.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.coldescription});
			this.list.Dock = System.Windows.Forms.DockStyle.Fill;
			this.list.FullRowSelect = true;
			this.list.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.list.Location = new System.Drawing.Point(0, 0);
			this.list.MultiSelect = false;
			this.list.Name = "list";
			this.list.ShowGroups = false;
			this.list.Size = new System.Drawing.Size(390, 548);
			this.list.TabIndex = 1;
			this.list.UseCompatibleStateImageBehavior = false;
			this.list.View = System.Windows.Forms.View.Details;
			this.list.Resize += new System.EventHandler(this.list_Resize);
			this.list.SelectedIndexChanged += new System.EventHandler(this.list_SelectedIndexChanged);
			this.list.MouseUp += new System.Windows.Forms.MouseEventHandler(this.list_MouseUp);
			this.list.KeyUp += new System.Windows.Forms.KeyEventHandler(this.list_KeyUp);
			// 
			// coldescription
			// 
			this.coldescription.Text = "Description";
			this.coldescription.Width = 238;
			// 
			// UndoRedoPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.Controls.Add(this.list);
			this.Name = "UndoRedoPanel";
			this.Size = new System.Drawing.Size(390, 548);
			this.ResumeLayout(false);

		}

		#endregion

		private CodeImp.DoomBuilder.Controls.OptimizedListView list;
		private System.Windows.Forms.ColumnHeader coldescription;
	}
}
