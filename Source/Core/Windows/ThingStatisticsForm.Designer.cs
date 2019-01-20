namespace CodeImp.DoomBuilder.Windows
{
    partial class ThingStatisticsForm
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
			this.apply = new System.Windows.Forms.Button();
			this.dataGridView = new System.Windows.Forms.DataGridView();
			this.ThingType = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ThingTitle = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ThingClassName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ThingCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.hideUnused = new System.Windows.Forms.CheckBox();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
			this.SuspendLayout();
			// 
			// apply
			// 
			this.apply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.apply.Location = new System.Drawing.Point(369, 319);
			this.apply.Name = "apply";
			this.apply.Size = new System.Drawing.Size(120, 23);
			this.apply.TabIndex = 7;
			this.apply.Text = "Close";
			this.apply.UseVisualStyleBackColor = true;
			this.apply.Click += new System.EventHandler(this.apply_Click);
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
            this.ThingType,
            this.ThingTitle,
            this.ThingClassName,
            this.ThingCount});
			this.dataGridView.Location = new System.Drawing.Point(12, 12);
			this.dataGridView.MultiSelect = false;
			this.dataGridView.Name = "dataGridView";
			this.dataGridView.ReadOnly = true;
			this.dataGridView.RowHeadersVisible = false;
			this.dataGridView.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.dataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dataGridView.Size = new System.Drawing.Size(477, 301);
			this.dataGridView.TabIndex = 6;
			this.dataGridView.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridView_CellMouseClick);
			// 
			// ThingType
			// 
			this.ThingType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			this.ThingType.DefaultCellStyle = dataGridViewCellStyle2;
			this.ThingType.HeaderText = "Type";
			this.ThingType.Name = "ThingType";
			this.ThingType.ReadOnly = true;
			this.ThingType.Width = 56;
			// 
			// ThingTitle
			// 
			this.ThingTitle.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.ThingTitle.DefaultCellStyle = dataGridViewCellStyle3;
			this.ThingTitle.HeaderText = "Title";
			this.ThingTitle.Name = "ThingTitle";
			this.ThingTitle.ReadOnly = true;
			// 
			// ThingClassName
			// 
			this.ThingClassName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			this.ThingClassName.DefaultCellStyle = dataGridViewCellStyle4;
			this.ThingClassName.HeaderText = "Class Name";
			this.ThingClassName.Name = "ThingClassName";
			this.ThingClassName.ReadOnly = true;
			this.ThingClassName.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			this.ThingClassName.Width = 88;
			// 
			// ThingCount
			// 
			this.ThingCount.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
			dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			this.ThingCount.DefaultCellStyle = dataGridViewCellStyle5;
			this.ThingCount.HeaderText = "Count";
			this.ThingCount.Name = "ThingCount";
			this.ThingCount.ReadOnly = true;
			this.ThingCount.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			this.ThingCount.Width = 60;
			// 
			// hideUnused
			// 
			this.hideUnused.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.hideUnused.AutoSize = true;
			this.hideUnused.Location = new System.Drawing.Point(13, 323);
			this.hideUnused.Name = "hideUnused";
			this.hideUnused.Size = new System.Drawing.Size(123, 17);
			this.hideUnused.TabIndex = 8;
			this.hideUnused.Text = "Hide Unused Things";
			this.hideUnused.UseVisualStyleBackColor = true;
			this.hideUnused.CheckedChanged += new System.EventHandler(this.hideUnused_CheckedChanged);
			// 
			// ThingStatisticsForm
			// 
			this.AcceptButton = this.apply;
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.ClientSize = new System.Drawing.Size(501, 348);
			this.Controls.Add(this.hideUnused);
			this.Controls.Add(this.apply);
			this.Controls.Add(this.dataGridView);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MinimizeBox = false;
			this.Name = "ThingStatisticsForm";
			this.Opacity = 0;
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "Thing statistics";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ThingStatisticsForm_FormClosing);
			((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button apply;
		private System.Windows.Forms.DataGridView dataGridView;
        private System.Windows.Forms.CheckBox hideUnused;
		private System.Windows.Forms.DataGridViewTextBoxColumn ThingType;
		private System.Windows.Forms.DataGridViewTextBoxColumn ThingTitle;
		private System.Windows.Forms.DataGridViewTextBoxColumn ThingClassName;
		private System.Windows.Forms.DataGridViewTextBoxColumn ThingCount;
    }
}