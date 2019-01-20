namespace CodeImp.DoomBuilder.Windows
{
	partial class TagStatisticsForm
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
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TagStatisticsForm));
			this.dataGridView = new System.Windows.Forms.DataGridView();
			this.TagColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Label = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Sectors = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Linedefs = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Things = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.apply = new System.Windows.Forms.Button();
			this.cancel = new System.Windows.Forms.Button();
			this.hint = new System.Windows.Forms.TextBox();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
			this.SuspendLayout();
			// 
			// dataGridView
			// 
			this.dataGridView.AllowUserToAddRows = false;
			this.dataGridView.AllowUserToDeleteRows = false;
			this.dataGridView.AllowUserToResizeColumns = false;
			this.dataGridView.AllowUserToResizeRows = false;
			this.dataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.dataGridView.BackgroundColor = System.Drawing.SystemColors.Window;
			this.dataGridView.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.dataGridView.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
			this.dataGridView.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
			dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.dataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
			this.dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.TagColumn,
            this.Label,
            this.Sectors,
            this.Linedefs,
            this.Things});
			this.dataGridView.Location = new System.Drawing.Point(12, 12);
			this.dataGridView.MultiSelect = false;
			this.dataGridView.Name = "dataGridView";
			this.dataGridView.RowHeadersVisible = false;
			this.dataGridView.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.dataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
			this.dataGridView.Size = new System.Drawing.Size(477, 256);
			this.dataGridView.TabIndex = 3;
			this.dataGridView.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridView_CellMouseClick);
			// 
			// TagColumn
			// 
			this.TagColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
			dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			this.TagColumn.DefaultCellStyle = dataGridViewCellStyle2;
			this.TagColumn.HeaderText = "Tag";
			this.TagColumn.Name = "TagColumn";
			this.TagColumn.ReadOnly = true;
			this.TagColumn.Width = 51;
			// 
			// Label
			// 
			this.Label.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.Label.DefaultCellStyle = dataGridViewCellStyle3;
			this.Label.HeaderText = "Label";
			this.Label.Name = "Label";
			// 
			// Sectors
			// 
			this.Sectors.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
			dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			this.Sectors.DefaultCellStyle = dataGridViewCellStyle4;
			this.Sectors.HeaderText = "Sectors";
			this.Sectors.Name = "Sectors";
			this.Sectors.ReadOnly = true;
			this.Sectors.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			this.Sectors.Width = 68;
			// 
			// Linedefs
			// 
			this.Linedefs.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
			dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			this.Linedefs.DefaultCellStyle = dataGridViewCellStyle5;
			this.Linedefs.HeaderText = "Linedefs";
			this.Linedefs.Name = "Linedefs";
			this.Linedefs.ReadOnly = true;
			this.Linedefs.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			this.Linedefs.Width = 72;
			// 
			// Things
			// 
			this.Things.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
			dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			this.Things.DefaultCellStyle = dataGridViewCellStyle6;
			this.Things.HeaderText = "Things";
			this.Things.Name = "Things";
			this.Things.ReadOnly = true;
			this.Things.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			this.Things.Width = 64;
			// 
			// apply
			// 
			this.apply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.apply.Location = new System.Drawing.Point(303, 319);
			this.apply.Name = "apply";
			this.apply.Size = new System.Drawing.Size(90, 23);
			this.apply.TabIndex = 4;
			this.apply.Text = "OK";
			this.apply.UseVisualStyleBackColor = true;
			this.apply.Click += new System.EventHandler(this.apply_Click);
			// 
			// cancel
			// 
			this.cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancel.Location = new System.Drawing.Point(399, 319);
			this.cancel.Name = "cancel";
			this.cancel.Size = new System.Drawing.Size(90, 23);
			this.cancel.TabIndex = 5;
			this.cancel.Text = "Cancel";
			this.cancel.UseVisualStyleBackColor = true;
			this.cancel.Click += new System.EventHandler(this.cancel_Click);
			// 
			// hint
			// 
			this.hint.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.hint.BackColor = System.Drawing.SystemColors.Control;
			this.hint.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.hint.Location = new System.Drawing.Point(12, 274);
			this.hint.Multiline = true;
			this.hint.Name = "hint";
			this.hint.Size = new System.Drawing.Size(477, 68);
			this.hint.TabIndex = 7;
			this.hint.Text = resources.GetString("hint.Text");
			// 
			// TagStatisticsForm
			// 
			this.AcceptButton = this.apply;
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.CancelButton = this.cancel;
			this.ClientSize = new System.Drawing.Size(501, 348);
			this.Controls.Add(this.cancel);
			this.Controls.Add(this.apply);
			this.Controls.Add(this.hint);
			this.Controls.Add(this.dataGridView);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(120, 80);
			this.Name = "TagStatisticsForm";
			this.Opacity = 0;
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "Tag statistics";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TagStatisticsForm_FormClosing);
			((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.DataGridView dataGridView;
		private System.Windows.Forms.Button apply;
		private System.Windows.Forms.Button cancel;
		private System.Windows.Forms.TextBox hint;
		private System.Windows.Forms.DataGridViewTextBoxColumn TagColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn Label;
		private System.Windows.Forms.DataGridViewTextBoxColumn Sectors;
		private System.Windows.Forms.DataGridViewTextBoxColumn Linedefs;
		private System.Windows.Forms.DataGridViewTextBoxColumn Things;
	}
}