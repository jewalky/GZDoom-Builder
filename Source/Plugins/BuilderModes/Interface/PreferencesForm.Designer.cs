namespace CodeImp.DoomBuilder.BuilderModes
{
	partial class PreferencesForm
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
			this.tabs = new System.Windows.Forms.TabControl();
			this.taboptions = new System.Windows.Forms.TabPage();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.label15 = new System.Windows.Forms.Label();
			this.label14 = new System.Windows.Forms.Label();
			this.defaultfloorheight = new CodeImp.DoomBuilder.Controls.ButtonsNumericTextbox();
			this.label13 = new System.Windows.Forms.Label();
			this.defaultceilheight = new CodeImp.DoomBuilder.Controls.ButtonsNumericTextbox();
			this.label12 = new System.Windows.Forms.Label();
			this.defaultbrightness = new CodeImp.DoomBuilder.Controls.ButtonsNumericTextbox();
			this.label11 = new System.Windows.Forms.Label();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.switchviewmodes = new System.Windows.Forms.CheckBox();
			this.autodrawonedit = new System.Windows.Forms.CheckBox();
			this.syncSelection = new System.Windows.Forms.CheckBox();
			this.dontMoveGeometryOutsideBounds = new System.Windows.Forms.CheckBox();
			this.autoaligntexturesoncreate = new System.Windows.Forms.CheckBox();
			this.autodragonpaste = new System.Windows.Forms.CheckBox();
			this.visualmodeclearselection = new System.Windows.Forms.CheckBox();
			this.autoclearselection = new System.Windows.Forms.CheckBox();
			this.editnewthing = new System.Windows.Forms.CheckBox();
			this.editnewsector = new System.Windows.Forms.CheckBox();
			this.additiveselect = new System.Windows.Forms.CheckBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.mouseselectionthreshold = new CodeImp.DoomBuilder.Controls.ButtonsNumericTextbox();
			this.label16 = new System.Windows.Forms.Label();
			this.label17 = new System.Windows.Forms.Label();
			this.splitlinedefsrange = new CodeImp.DoomBuilder.Controls.ButtonsNumericTextbox();
			this.stitchrange = new CodeImp.DoomBuilder.Controls.ButtonsNumericTextbox();
			this.highlightthingsrange = new CodeImp.DoomBuilder.Controls.ButtonsNumericTextbox();
			this.highlightrange = new CodeImp.DoomBuilder.Controls.ButtonsNumericTextbox();
			this.label8 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.splitbehavior = new System.Windows.Forms.ComboBox();
			this.label10 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.heightbysidedef = new System.Windows.Forms.ComboBox();
			this.tabs.SuspendLayout();
			this.taboptions.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabs
			// 
			this.tabs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tabs.Controls.Add(this.taboptions);
			this.tabs.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.tabs.Location = new System.Drawing.Point(12, 12);
			this.tabs.Name = "tabs";
			this.tabs.Padding = new System.Drawing.Point(24, 3);
			this.tabs.SelectedIndex = 0;
			this.tabs.Size = new System.Drawing.Size(677, 454);
			this.tabs.TabIndex = 0;
			// 
			// taboptions
			// 
			this.taboptions.Controls.Add(this.groupBox4);
			this.taboptions.Controls.Add(this.groupBox3);
			this.taboptions.Controls.Add(this.groupBox2);
			this.taboptions.Controls.Add(this.groupBox1);
			this.taboptions.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.taboptions.Location = new System.Drawing.Point(4, 22);
			this.taboptions.Name = "taboptions";
			this.taboptions.Padding = new System.Windows.Forms.Padding(3);
			this.taboptions.Size = new System.Drawing.Size(669, 428);
			this.taboptions.TabIndex = 0;
			this.taboptions.Text = "Editing";
			this.taboptions.UseVisualStyleBackColor = true;
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.Add(this.label15);
			this.groupBox4.Controls.Add(this.label14);
			this.groupBox4.Controls.Add(this.defaultfloorheight);
			this.groupBox4.Controls.Add(this.label13);
			this.groupBox4.Controls.Add(this.defaultceilheight);
			this.groupBox4.Controls.Add(this.label12);
			this.groupBox4.Controls.Add(this.defaultbrightness);
			this.groupBox4.Controls.Add(this.label11);
			this.groupBox4.Location = new System.Drawing.Point(6, 300);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(272, 122);
			this.groupBox4.TabIndex = 2;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = " Default sector settings";
			// 
			// label15
			// 
			this.label15.AutoSize = true;
			this.label15.Location = new System.Drawing.Point(221, 28);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(27, 13);
			this.label15.TabIndex = 26;
			this.label15.Text = "m.u.";
			// 
			// label14
			// 
			this.label14.AutoSize = true;
			this.label14.Location = new System.Drawing.Point(221, 58);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(27, 13);
			this.label14.TabIndex = 20;
			this.label14.Text = "m.u.";
			// 
			// defaultfloorheight
			// 
			this.defaultfloorheight.AllowDecimal = false;
			this.defaultfloorheight.AllowExpressions = false;
			this.defaultfloorheight.AllowNegative = true;
			this.defaultfloorheight.AllowRelative = false;
			this.defaultfloorheight.ButtonStep = 5;
			this.defaultfloorheight.ButtonStepBig = 10F;
			this.defaultfloorheight.ButtonStepFloat = 1F;
			this.defaultfloorheight.ButtonStepSmall = 0.1F;
			this.defaultfloorheight.ButtonStepsUseModifierKeys = false;
			this.defaultfloorheight.ButtonStepsWrapAround = false;
			this.defaultfloorheight.Location = new System.Drawing.Point(156, 23);
			this.defaultfloorheight.Name = "defaultfloorheight";
			this.defaultfloorheight.Size = new System.Drawing.Size(59, 24);
			this.defaultfloorheight.StepValues = null;
			this.defaultfloorheight.TabIndex = 0;
			// 
			// label13
			// 
			this.label13.Location = new System.Drawing.Point(41, 28);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(110, 14);
			this.label13.TabIndex = 24;
			this.label13.Text = "Default floor height:";
			this.label13.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// defaultceilheight
			// 
			this.defaultceilheight.AllowDecimal = false;
			this.defaultceilheight.AllowExpressions = false;
			this.defaultceilheight.AllowNegative = true;
			this.defaultceilheight.AllowRelative = false;
			this.defaultceilheight.ButtonStep = 5;
			this.defaultceilheight.ButtonStepBig = 10F;
			this.defaultceilheight.ButtonStepFloat = 1F;
			this.defaultceilheight.ButtonStepSmall = 0.1F;
			this.defaultceilheight.ButtonStepsUseModifierKeys = false;
			this.defaultceilheight.ButtonStepsWrapAround = false;
			this.defaultceilheight.Location = new System.Drawing.Point(156, 53);
			this.defaultceilheight.Name = "defaultceilheight";
			this.defaultceilheight.Size = new System.Drawing.Size(59, 24);
			this.defaultceilheight.StepValues = null;
			this.defaultceilheight.TabIndex = 1;
			// 
			// label12
			// 
			this.label12.Location = new System.Drawing.Point(41, 58);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(110, 14);
			this.label12.TabIndex = 22;
			this.label12.Text = "Default ceiling height:";
			this.label12.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// defaultbrightness
			// 
			this.defaultbrightness.AllowDecimal = false;
			this.defaultbrightness.AllowExpressions = false;
			this.defaultbrightness.AllowNegative = false;
			this.defaultbrightness.AllowRelative = false;
			this.defaultbrightness.ButtonStep = 5;
			this.defaultbrightness.ButtonStepBig = 10F;
			this.defaultbrightness.ButtonStepFloat = 1F;
			this.defaultbrightness.ButtonStepSmall = 0.1F;
			this.defaultbrightness.ButtonStepsUseModifierKeys = false;
			this.defaultbrightness.ButtonStepsWrapAround = false;
			this.defaultbrightness.Location = new System.Drawing.Point(156, 83);
			this.defaultbrightness.Name = "defaultbrightness";
			this.defaultbrightness.Size = new System.Drawing.Size(59, 24);
			this.defaultbrightness.StepValues = null;
			this.defaultbrightness.TabIndex = 2;
			// 
			// label11
			// 
			this.label11.Location = new System.Drawing.Point(41, 88);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(110, 14);
			this.label11.TabIndex = 20;
			this.label11.Text = "Default brightness:";
			this.label11.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.switchviewmodes);
			this.groupBox3.Controls.Add(this.autodrawonedit);
			this.groupBox3.Controls.Add(this.syncSelection);
			this.groupBox3.Controls.Add(this.dontMoveGeometryOutsideBounds);
			this.groupBox3.Controls.Add(this.autoaligntexturesoncreate);
			this.groupBox3.Controls.Add(this.autodragonpaste);
			this.groupBox3.Controls.Add(this.visualmodeclearselection);
			this.groupBox3.Controls.Add(this.autoclearselection);
			this.groupBox3.Controls.Add(this.editnewthing);
			this.groupBox3.Controls.Add(this.editnewsector);
			this.groupBox3.Controls.Add(this.additiveselect);
			this.groupBox3.Location = new System.Drawing.Point(284, 104);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(379, 317);
			this.groupBox3.TabIndex = 3;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = " Options ";
			// 
			// switchviewmodes
			// 
			this.switchviewmodes.AutoSize = true;
			this.switchviewmodes.Location = new System.Drawing.Point(13, 287);
			this.switchviewmodes.Name = "switchviewmodes";
			this.switchviewmodes.Size = new System.Drawing.Size(317, 17);
			this.switchviewmodes.TabIndex = 10;
			this.switchviewmodes.Text = "Switch view modes when switching to the same Classic Mode";
			this.switchviewmodes.UseVisualStyleBackColor = true;
			// 
			// autodrawonedit
			// 
			this.autodrawonedit.AutoSize = true;
			this.autodrawonedit.Location = new System.Drawing.Point(13, 24);
			this.autodrawonedit.Name = "autodrawonedit";
			this.autodrawonedit.Size = new System.Drawing.Size(353, 30);
			this.autodrawonedit.TabIndex = 0;
			this.autodrawonedit.Text = "Start drawing when Edit pressed over empty space in Classic modes\r\nInsert new thi" +
    "ng when Edit pressed over empty space in Things mode";
			this.autodrawonedit.UseVisualStyleBackColor = true;
			// 
			// syncSelection
			// 
			this.syncSelection.AutoSize = true;
			this.syncSelection.Location = new System.Drawing.Point(13, 262);
			this.syncSelection.Name = "syncSelection";
			this.syncSelection.Size = new System.Drawing.Size(295, 17);
			this.syncSelection.TabIndex = 9;
			this.syncSelection.Text = "Synchronize selection between Visual and Classic modes";
			this.syncSelection.UseVisualStyleBackColor = true;
			// 
			// dontMoveGeometryOutsideBounds
			// 
			this.dontMoveGeometryOutsideBounds.AutoSize = true;
			this.dontMoveGeometryOutsideBounds.Location = new System.Drawing.Point(13, 237);
			this.dontMoveGeometryOutsideBounds.Name = "dontMoveGeometryOutsideBounds";
			this.dontMoveGeometryOutsideBounds.Size = new System.Drawing.Size(323, 17);
			this.dontMoveGeometryOutsideBounds.TabIndex = 8;
			this.dontMoveGeometryOutsideBounds.Text = "Don\'t move selection if any part of it is outside of map boundary";
			this.dontMoveGeometryOutsideBounds.UseVisualStyleBackColor = true;
			// 
			// autoaligntexturesoncreate
			// 
			this.autoaligntexturesoncreate.AutoSize = true;
			this.autoaligntexturesoncreate.Location = new System.Drawing.Point(13, 212);
			this.autoaligntexturesoncreate.Name = "autoaligntexturesoncreate";
			this.autoaligntexturesoncreate.Size = new System.Drawing.Size(233, 17);
			this.autoaligntexturesoncreate.TabIndex = 7;
			this.autoaligntexturesoncreate.Text = "Auto-align textures of newly created linedefs";
			this.autoaligntexturesoncreate.UseVisualStyleBackColor = true;
			// 
			// autodragonpaste
			// 
			this.autodragonpaste.AutoSize = true;
			this.autodragonpaste.Location = new System.Drawing.Point(13, 187);
			this.autodragonpaste.Name = "autodragonpaste";
			this.autodragonpaste.Size = new System.Drawing.Size(201, 17);
			this.autodragonpaste.TabIndex = 6;
			this.autodragonpaste.Text = "Automatically drag selection on paste";
			this.autodragonpaste.UseVisualStyleBackColor = true;
			// 
			// visualmodeclearselection
			// 
			this.visualmodeclearselection.AutoSize = true;
			this.visualmodeclearselection.Location = new System.Drawing.Point(13, 162);
			this.visualmodeclearselection.Name = "visualmodeclearselection";
			this.visualmodeclearselection.Size = new System.Drawing.Size(231, 17);
			this.visualmodeclearselection.TabIndex = 5;
			this.visualmodeclearselection.Text = "Automatically clear selection in Visual Mode";
			this.visualmodeclearselection.UseVisualStyleBackColor = true;
			// 
			// autoclearselection
			// 
			this.autoclearselection.AutoSize = true;
			this.autoclearselection.Location = new System.Drawing.Point(13, 137);
			this.autoclearselection.Name = "autoclearselection";
			this.autoclearselection.Size = new System.Drawing.Size(241, 17);
			this.autoclearselection.TabIndex = 4;
			this.autoclearselection.Text = "Automatically clear selection in Classic Modes";
			this.autoclearselection.UseVisualStyleBackColor = true;
			// 
			// editnewthing
			// 
			this.editnewthing.AutoSize = true;
			this.editnewthing.Location = new System.Drawing.Point(13, 62);
			this.editnewthing.Name = "editnewthing";
			this.editnewthing.Size = new System.Drawing.Size(248, 17);
			this.editnewthing.TabIndex = 1;
			this.editnewthing.Text = "Edit thing properties when inserting a new thing";
			this.editnewthing.UseVisualStyleBackColor = true;
			// 
			// editnewsector
			// 
			this.editnewsector.AutoSize = true;
			this.editnewsector.Location = new System.Drawing.Point(13, 87);
			this.editnewsector.Name = "editnewsector";
			this.editnewsector.Size = new System.Drawing.Size(253, 17);
			this.editnewsector.TabIndex = 2;
			this.editnewsector.Text = "Edit sector properties after drawing a new sector";
			this.editnewsector.UseVisualStyleBackColor = true;
			// 
			// additiveselect
			// 
			this.additiveselect.AutoSize = true;
			this.additiveselect.Location = new System.Drawing.Point(13, 112);
			this.additiveselect.Name = "additiveselect";
			this.additiveselect.Size = new System.Drawing.Size(207, 17);
			this.additiveselect.TabIndex = 3;
			this.additiveselect.Text = "Additive selecting without holding Shift";
			this.additiveselect.UseVisualStyleBackColor = true;
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.mouseselectionthreshold);
			this.groupBox2.Controls.Add(this.label16);
			this.groupBox2.Controls.Add(this.label17);
			this.groupBox2.Controls.Add(this.splitlinedefsrange);
			this.groupBox2.Controls.Add(this.stitchrange);
			this.groupBox2.Controls.Add(this.highlightthingsrange);
			this.groupBox2.Controls.Add(this.highlightrange);
			this.groupBox2.Controls.Add(this.label8);
			this.groupBox2.Controls.Add(this.label2);
			this.groupBox2.Controls.Add(this.label3);
			this.groupBox2.Controls.Add(this.label9);
			this.groupBox2.Controls.Add(this.label5);
			this.groupBox2.Controls.Add(this.label6);
			this.groupBox2.Controls.Add(this.label4);
			this.groupBox2.Controls.Add(this.label7);
			this.groupBox2.Location = new System.Drawing.Point(6, 104);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(272, 190);
			this.groupBox2.TabIndex = 1;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = " Ranges ";
			// 
			// mouseselectionthreshold
			// 
			this.mouseselectionthreshold.AllowDecimal = false;
			this.mouseselectionthreshold.AllowExpressions = false;
			this.mouseselectionthreshold.AllowNegative = false;
			this.mouseselectionthreshold.AllowRelative = false;
			this.mouseselectionthreshold.ButtonStep = 5;
			this.mouseselectionthreshold.ButtonStepBig = 10F;
			this.mouseselectionthreshold.ButtonStepFloat = 1F;
			this.mouseselectionthreshold.ButtonStepSmall = 0.1F;
			this.mouseselectionthreshold.ButtonStepsUseModifierKeys = false;
			this.mouseselectionthreshold.ButtonStepsWrapAround = false;
			this.mouseselectionthreshold.Location = new System.Drawing.Point(156, 141);
			this.mouseselectionthreshold.Name = "mouseselectionthreshold";
			this.mouseselectionthreshold.Size = new System.Drawing.Size(59, 24);
			this.mouseselectionthreshold.StepValues = null;
			this.mouseselectionthreshold.TabIndex = 16;
			this.mouseselectionthreshold.Load += new System.EventHandler(this.buttonsNumericTextbox1_Load);
			// 
			// label16
			// 
			this.label16.AutoSize = true;
			this.label16.Location = new System.Drawing.Point(221, 146);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(33, 13);
			this.label16.TabIndex = 18;
			this.label16.Text = "pixels";
			this.label16.Click += new System.EventHandler(this.label16_Click);
			// 
			// label17
			// 
			this.label17.AutoSize = true;
			this.label17.Location = new System.Drawing.Point(13, 146);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(133, 13);
			this.label17.TabIndex = 17;
			this.label17.Text = "Mouse selection threshold:";
			this.label17.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// splitlinedefsrange
			// 
			this.splitlinedefsrange.AllowDecimal = false;
			this.splitlinedefsrange.AllowExpressions = false;
			this.splitlinedefsrange.AllowNegative = false;
			this.splitlinedefsrange.AllowRelative = false;
			this.splitlinedefsrange.ButtonStep = 5;
			this.splitlinedefsrange.ButtonStepBig = 10F;
			this.splitlinedefsrange.ButtonStepFloat = 1F;
			this.splitlinedefsrange.ButtonStepSmall = 0.1F;
			this.splitlinedefsrange.ButtonStepsUseModifierKeys = false;
			this.splitlinedefsrange.ButtonStepsWrapAround = false;
			this.splitlinedefsrange.Location = new System.Drawing.Point(156, 111);
			this.splitlinedefsrange.Name = "splitlinedefsrange";
			this.splitlinedefsrange.Size = new System.Drawing.Size(59, 24);
			this.splitlinedefsrange.StepValues = null;
			this.splitlinedefsrange.TabIndex = 3;
			// 
			// stitchrange
			// 
			this.stitchrange.AllowDecimal = false;
			this.stitchrange.AllowExpressions = false;
			this.stitchrange.AllowNegative = false;
			this.stitchrange.AllowRelative = false;
			this.stitchrange.ButtonStep = 5;
			this.stitchrange.ButtonStepBig = 10F;
			this.stitchrange.ButtonStepFloat = 1F;
			this.stitchrange.ButtonStepSmall = 0.1F;
			this.stitchrange.ButtonStepsUseModifierKeys = false;
			this.stitchrange.ButtonStepsWrapAround = false;
			this.stitchrange.Location = new System.Drawing.Point(156, 81);
			this.stitchrange.Name = "stitchrange";
			this.stitchrange.Size = new System.Drawing.Size(59, 24);
			this.stitchrange.StepValues = null;
			this.stitchrange.TabIndex = 2;
			// 
			// highlightthingsrange
			// 
			this.highlightthingsrange.AllowDecimal = false;
			this.highlightthingsrange.AllowExpressions = false;
			this.highlightthingsrange.AllowNegative = false;
			this.highlightthingsrange.AllowRelative = false;
			this.highlightthingsrange.ButtonStep = 5;
			this.highlightthingsrange.ButtonStepBig = 10F;
			this.highlightthingsrange.ButtonStepFloat = 1F;
			this.highlightthingsrange.ButtonStepSmall = 0.1F;
			this.highlightthingsrange.ButtonStepsUseModifierKeys = false;
			this.highlightthingsrange.ButtonStepsWrapAround = false;
			this.highlightthingsrange.Location = new System.Drawing.Point(156, 51);
			this.highlightthingsrange.Name = "highlightthingsrange";
			this.highlightthingsrange.Size = new System.Drawing.Size(59, 24);
			this.highlightthingsrange.StepValues = null;
			this.highlightthingsrange.TabIndex = 1;
			// 
			// highlightrange
			// 
			this.highlightrange.AllowDecimal = false;
			this.highlightrange.AllowExpressions = false;
			this.highlightrange.AllowNegative = false;
			this.highlightrange.AllowRelative = false;
			this.highlightrange.ButtonStep = 5;
			this.highlightrange.ButtonStepBig = 10F;
			this.highlightrange.ButtonStepFloat = 1F;
			this.highlightrange.ButtonStepSmall = 0.1F;
			this.highlightrange.ButtonStepsUseModifierKeys = false;
			this.highlightrange.ButtonStepsWrapAround = false;
			this.highlightrange.Location = new System.Drawing.Point(156, 21);
			this.highlightrange.Name = "highlightrange";
			this.highlightrange.Size = new System.Drawing.Size(59, 24);
			this.highlightrange.StepValues = null;
			this.highlightrange.TabIndex = 0;
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(221, 116);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(33, 13);
			this.label8.TabIndex = 15;
			this.label8.Text = "pixels";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(33, 86);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(113, 13);
			this.label2.TabIndex = 4;
			this.label2.Text = "Stitch geometry within:";
			this.label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(221, 86);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(33, 13);
			this.label3.TabIndex = 6;
			this.label3.Text = "pixels";
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(47, 115);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(99, 13);
			this.label9.TabIndex = 13;
			this.label9.Text = "Split linedefs within:";
			this.label9.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(20, 26);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(127, 13);
			this.label5.TabIndex = 7;
			this.label5.Text = "Highlight geometry within:";
			this.label5.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(221, 55);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(33, 13);
			this.label6.TabIndex = 12;
			this.label6.Text = "pixels";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(221, 26);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(33, 13);
			this.label4.TabIndex = 9;
			this.label4.Text = "pixels";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(36, 56);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(112, 13);
			this.label7.TabIndex = 10;
			this.label7.Text = "Highlight things within:";
			this.label7.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.splitbehavior);
			this.groupBox1.Controls.Add(this.label10);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.heightbysidedef);
			this.groupBox1.Location = new System.Drawing.Point(6, 6);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(657, 92);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = " Behavior ";
			// 
			// splitbehavior
			// 
			this.splitbehavior.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.splitbehavior.FormattingEnabled = true;
			this.splitbehavior.Items.AddRange(new object[] {
            "Interpolate texture coordinates",
            "Duplicate texture coordinates",
            "Reset X coordinate, duplicate Y coordinate",
            "Reset X and Y coordinates"});
			this.splitbehavior.Location = new System.Drawing.Point(342, 55);
			this.splitbehavior.Name = "splitbehavior";
			this.splitbehavior.Size = new System.Drawing.Size(309, 21);
			this.splitbehavior.TabIndex = 1;
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(203, 58);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(120, 13);
			this.label10.TabIndex = 1;
			this.label10.Text = "When splitting a linedef:";
			this.label10.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(9, 22);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(308, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "When sector height changes are used on a wall in Visual Mode:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// heightbysidedef
			// 
			this.heightbysidedef.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.heightbysidedef.FormattingEnabled = true;
			this.heightbysidedef.Items.AddRange(new object[] {
            "Do nothing",
            "Change the ceiling height",
            "Change the floor height",
            "Change both floor and ceiling height"});
			this.heightbysidedef.Location = new System.Drawing.Point(342, 19);
			this.heightbysidedef.Name = "heightbysidedef";
			this.heightbysidedef.Size = new System.Drawing.Size(309, 21);
			this.heightbysidedef.TabIndex = 0;
			// 
			// PreferencesForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.ClientSize = new System.Drawing.Size(701, 478);
			this.Controls.Add(this.tabs);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "PreferencesForm";
			this.ShowIcon = false;
			this.Text = "PreferencesForm";
			this.tabs.ResumeLayout(false);
			this.taboptions.ResumeLayout(false);
			this.groupBox4.ResumeLayout(false);
			this.groupBox4.PerformLayout();
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TabControl tabs;
		private System.Windows.Forms.TabPage taboptions;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox heightbysidedef;
		private System.Windows.Forms.CheckBox editnewsector;
		private System.Windows.Forms.CheckBox editnewthing;
		private System.Windows.Forms.CheckBox additiveselect;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.CheckBox autoclearselection;
		private System.Windows.Forms.CheckBox visualmodeclearselection;
		private CodeImp.DoomBuilder.Controls.ButtonsNumericTextbox splitlinedefsrange;
		private CodeImp.DoomBuilder.Controls.ButtonsNumericTextbox stitchrange;
		private CodeImp.DoomBuilder.Controls.ButtonsNumericTextbox highlightthingsrange;
		private CodeImp.DoomBuilder.Controls.ButtonsNumericTextbox highlightrange;
		private System.Windows.Forms.CheckBox autodragonpaste;
		private System.Windows.Forms.ComboBox splitbehavior;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.CheckBox autoaligntexturesoncreate;
		private System.Windows.Forms.CheckBox dontMoveGeometryOutsideBounds;
		private System.Windows.Forms.CheckBox syncSelection;
		private System.Windows.Forms.GroupBox groupBox4;
		private CodeImp.DoomBuilder.Controls.ButtonsNumericTextbox defaultbrightness;
		private System.Windows.Forms.Label label11;
		private CodeImp.DoomBuilder.Controls.ButtonsNumericTextbox defaultfloorheight;
		private System.Windows.Forms.Label label13;
		private CodeImp.DoomBuilder.Controls.ButtonsNumericTextbox defaultceilheight;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.Label label15;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.CheckBox autodrawonedit;
		private System.Windows.Forms.CheckBox switchviewmodes;
		private Controls.ButtonsNumericTextbox mouseselectionthreshold;
		private System.Windows.Forms.Label label16;
		private System.Windows.Forms.Label label17;
	}
}