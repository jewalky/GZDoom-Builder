namespace CodeImp.DoomBuilder.Controls
{
	partial class ArgumentBox
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
			this.components = new System.ComponentModel.Container();
			this.combobox = new System.Windows.Forms.ComboBox();
			this.button = new System.Windows.Forms.Button();
			this.scrollbuttons = new System.Windows.Forms.VScrollBar();
			this.tooltip = new System.Windows.Forms.ToolTip(this.components);
			this.SuspendLayout();
			// 
			// combobox
			// 
			this.combobox.DropDownWidth = 130;
			this.combobox.ForeColor = System.Drawing.SystemColors.HotTrack;
			this.combobox.Location = new System.Drawing.Point(0, 2);
			this.combobox.Name = "combobox";
			this.combobox.Size = new System.Drawing.Size(149, 21);
			this.combobox.TabIndex = 0;
			this.tooltip.SetToolTip(this.combobox, "Use ++ or -- prefixes to change by given value.\r\nUse +++ or --- prefixes to incre" +
					"mentally change by given value.\r\nUse < or > prefixes to decrementally or increme" +
					"ntally set given value.");
			this.combobox.Validating += new System.ComponentModel.CancelEventHandler(this.combobox_Validating);
			this.combobox.TextChanged += new System.EventHandler(this.combobox_TextChanged);
			// 
			// button
			// 
			this.button.Image = global::CodeImp.DoomBuilder.Properties.Resources.treeview;
			this.button.Location = new System.Drawing.Point(153, 0);
			this.button.Name = "button";
			this.button.Size = new System.Drawing.Size(28, 24);
			this.button.TabIndex = 1;
			this.button.UseVisualStyleBackColor = true;
			this.button.Visible = false;
			this.button.Click += new System.EventHandler(this.button_Click);
			// 
			// scrollbuttons
			// 
			this.scrollbuttons.LargeChange = 10000;
			this.scrollbuttons.Location = new System.Drawing.Point(186, -1);
			this.scrollbuttons.Maximum = 10000;
			this.scrollbuttons.Minimum = -10000;
			this.scrollbuttons.Name = "scrollbuttons";
			this.scrollbuttons.Size = new System.Drawing.Size(18, 24);
			this.scrollbuttons.TabIndex = 2;
			this.scrollbuttons.Visible = false;
			this.scrollbuttons.ValueChanged += new System.EventHandler(this.scrollbuttons_ValueChanged);
			// 
			// ArgumentBox
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.Controls.Add(this.scrollbuttons);
			this.Controls.Add(this.button);
			this.Controls.Add(this.combobox);
			this.Name = "ArgumentBox";
			this.Size = new System.Drawing.Size(268, 64);
			this.Layout += new System.Windows.Forms.LayoutEventHandler(this.ArgumentBox_Layout);
			this.Resize += new System.EventHandler(this.ArgumentBox_Resize);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ComboBox combobox;
		private System.Windows.Forms.Button button;
		private System.Windows.Forms.VScrollBar scrollbuttons;
		private System.Windows.Forms.ToolTip tooltip;
	}
}
