namespace CodeImp.DoomBuilder.Controls
{
	partial class ActionSelectorControl
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
			this.number = new CodeImp.DoomBuilder.Controls.AutoSelectTextbox();
			this.list = new System.Windows.Forms.ComboBox();
			this.numberpanel = new System.Windows.Forms.Panel();
			this.numberpanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// number
			// 
			this.number.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.number.ImeMode = System.Windows.Forms.ImeMode.Off;
			this.number.Location = new System.Drawing.Point(3, 1);
			this.number.Name = "number";
			this.number.Size = new System.Drawing.Size(43, 13);
			this.number.TabIndex = 0;
			this.number.Text = "402";
			this.number.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.number.TextChanged += new System.EventHandler(this.number_TextChanged);
			this.number.KeyDown += new System.Windows.Forms.KeyEventHandler(this.number_KeyDown);
			this.number.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.number_KeyPress);
			// 
			// list
			// 
			this.list.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
			this.list.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.list.FormattingEnabled = true;
			this.list.IntegralHeight = false;
			this.list.Location = new System.Drawing.Point(57, 0);
			this.list.MaxDropDownItems = 15;
			this.list.Name = "list";
			this.list.Size = new System.Drawing.Size(251, 21);
			this.list.TabIndex = 1;
			this.list.TabStop = false;
			this.list.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.list_DrawItem);
			this.list.SelectionChangeCommitted += new System.EventHandler(this.list_SelectionChangeCommitted);
			this.list.DropDownClosed += new System.EventHandler(this.list_DropDownClosed);
			// 
			// numberpanel
			// 
			this.numberpanel.BackColor = System.Drawing.SystemColors.Window;
			this.numberpanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.numberpanel.Controls.Add(this.number);
			this.numberpanel.Location = new System.Drawing.Point(0, 0);
			this.numberpanel.Name = "numberpanel";
			this.numberpanel.Size = new System.Drawing.Size(53, 21);
			this.numberpanel.TabIndex = 0;
			// 
			// ActionSelectorControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.BackColor = System.Drawing.Color.Transparent;
			this.Controls.Add(this.numberpanel);
			this.Controls.Add(this.list);
			this.Cursor = System.Windows.Forms.Cursors.Default;
			this.Name = "ActionSelectorControl";
			this.Size = new System.Drawing.Size(382, 21);
			this.Layout += new System.Windows.Forms.LayoutEventHandler(this.ActionSelectorControl_Layout);
			this.Resize += new System.EventHandler(this.ActionSelectorControl_Resize);
			this.numberpanel.ResumeLayout(false);
			this.numberpanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private CodeImp.DoomBuilder.Controls.AutoSelectTextbox number;
		private System.Windows.Forms.ComboBox list;
		private System.Windows.Forms.Panel numberpanel;
	}
}
