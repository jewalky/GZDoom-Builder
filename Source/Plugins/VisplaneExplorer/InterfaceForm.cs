#region === Copyright (c) 2010 Pascal van der Heiden ===

using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Windows;

#endregion

namespace CodeImp.DoomBuilder.Plugins.VisplaneExplorer
{
	public partial class InterfaceForm : DelayedForm
	{
		#region ================== Constants

		#endregion

		#region ================== mxd. Event handlers

		public event EventHandler OnOpenDoorsChanged;

		#endregion

		#region ================== Variables

		private ViewStats viewstats;
		private Point oldttposition;

		#endregion

		#region ================== Properties

		internal ViewStats ViewStats { get { return viewstats; } }
		internal bool OpenDoors { get { return cbopendoors.Checked; } } //mxd
		internal bool ShowHeatmap { get { return cbheatmap.Checked; } } //mxd

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public InterfaceForm()
		{
			InitializeComponent();
			cbopendoors.Checked = General.Settings.ReadPluginSetting("opendoors", false); //mxd
			cbheatmap.Checked = General.Settings.ReadPluginSetting("showheatmap", false); //mxd
		}

		#endregion

		#region ================== Methods

		// This adds the buttons to the toolbar
		public void AddToInterface()
		{
			General.Interface.BeginToolbarUpdate(); //mxd
			General.Interface.AddButton(statsbutton);
			General.Interface.AddButton(separator); //mxd
			General.Interface.AddButton(cbopendoors); //mxd
			General.Interface.AddButton(cbheatmap); //mxd
			General.Interface.EndToolbarUpdate(); //mxd
		}

		// This removes the buttons from the toolbar
		public void RemoveFromInterface()
		{
			General.Interface.BeginToolbarUpdate(); //mxd
			General.Interface.RemoveButton(cbheatmap); //mxd
			General.Interface.RemoveButton(cbopendoors); //mxd
			General.Interface.RemoveButton(separator); //mxd
			General.Interface.RemoveButton(statsbutton);
			General.Interface.EndToolbarUpdate(); //mxd

			//mxd. Save settings
			General.Settings.WritePluginSetting("opendoors", cbopendoors.Checked);
			General.Settings.WritePluginSetting("showheatmap", cbheatmap.Checked);
		}

		// This shows a tooltip
		public void ShowTooltip(string text, Point p)
		{
			Point sp = General.Interface.Display.PointToScreen(p);
			Point fp = (General.Interface as Form).Location;
			Point tp = new Point(sp.X - fp.X, sp.Y - fp.Y);

			if(oldttposition != tp)
			{
				tooltip.Show(text, General.Interface, tp);
				oldttposition = tp;
			}
		}

		// This hides the tooltip
		public void HideTooltip()
		{
			tooltip.Hide(General.Interface);
		}

		#endregion

		#region ================== Events

		// Selecting a type of stats to view
		private void stats_Click(object sender, EventArgs e)
		{
			foreach(ToolStripMenuItem i in statsbutton.DropDownItems)
				i.Checked = false;
			
			ToolStripMenuItem item = (ToolStripMenuItem)sender;
			viewstats = (ViewStats)int.Parse(item.Tag.ToString(), CultureInfo.InvariantCulture);
			item.Checked = true;
			statsbutton.Image = item.Image;

			General.Interface.RedrawDisplay();
		}

		//mxd
		private void cbheatmap_Click(object sender, EventArgs e)
		{
			General.Interface.RedrawDisplay();
		}

		//mxd
		private void cbopendoors_Click(object sender, EventArgs e)
		{
			if(OnOpenDoorsChanged != null) OnOpenDoorsChanged(this, EventArgs.Empty);
		}

		#endregion
	}
}
