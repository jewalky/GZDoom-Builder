namespace CodeImp.DoomBuilder.ColorPicker
{
	partial class ToolsForm
	{
		/// <summary>
		/// Требуется переменная конструктора.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Освободить все используемые ресурсы.
		/// </summary>
		/// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
		protected override void Dispose(bool disposing) {
			if(disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Код, автоматически созданный конструктором форм Windows

		/// <summary>
		/// Обязательный метод для поддержки конструктора - не изменяйте
		/// содержимое данного метода при помощи редактора кода.
		/// </summary>
		private void InitializeComponent() {
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.cpButton = new System.Windows.Forms.ToolStripButton();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.modesmenu = new System.Windows.Forms.ToolStripMenuItem();
			this.cpMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStrip1.SuspendLayout();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolStrip1
			// 
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cpButton});
			this.toolStrip1.Location = new System.Drawing.Point(0, 24);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Size = new System.Drawing.Size(284, 25);
			this.toolStrip1.TabIndex = 0;
			this.toolStrip1.Text = "toolStrip1";
			// 
			// cpButton
			// 
			this.cpButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.cpButton.Image = global::CodeImp.DoomBuilder.ColorPicker.Properties.Resources.cp;
			this.cpButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.cpButton.Name = "cpButton";
			this.cpButton.Size = new System.Drawing.Size(23, 22);
			this.cpButton.Tag = "togglelightpannel";
			this.cpButton.Text = "Pick Sector/Light Color";
			this.cpButton.Click += new System.EventHandler(this.InvokeTaggedAction);
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.modesmenu});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(284, 24);
			this.menuStrip1.TabIndex = 1;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// modesmenu
			// 
			this.modesmenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cpMenu});
			this.modesmenu.Name = "modesmenu";
			this.modesmenu.Size = new System.Drawing.Size(55, 20);
			this.modesmenu.Text = "Modes";
			// 
			// cpMenu
			// 
			this.cpMenu.Image = global::CodeImp.DoomBuilder.ColorPicker.Properties.Resources.cp;
			this.cpMenu.Name = "cpMenu";
			this.cpMenu.Size = new System.Drawing.Size(205, 22);
			this.cpMenu.Tag = "togglelightpannel";
			this.cpMenu.Text = "Pick Sector/Light Color...";
			this.cpMenu.Click += new System.EventHandler(this.InvokeTaggedAction);
			// 
			// ToolsForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.ClientSize = new System.Drawing.Size(284, 262);
			this.Controls.Add(this.toolStrip1);
			this.Controls.Add(this.menuStrip1);
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "ToolsForm";
			this.Text = "ToolStrip";
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.ToolStripButton cpButton;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem modesmenu;
		private System.Windows.Forms.ToolStripMenuItem cpMenu;
	}
}