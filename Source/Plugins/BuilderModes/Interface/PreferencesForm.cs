
#region ================== Copyright (c) 2007 Pascal vd Heiden

/*
 * Copyright (c) 2007 Pascal vd Heiden, www.codeimp.com
 * This program is released under GNU General Public License
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 */

#endregion

#region ================== Namespaces

using System.Windows.Forms;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Windows;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	public partial class PreferencesForm : DelayedForm
	{
		#region ================== Variables

		#endregion

		#region ================== Properties

		#endregion

		#region ================== Constructor / Disposer

		// Contrustor
		public PreferencesForm()
		{
			InitializeComponent();

			// Apply current settings to interface
			heightbysidedef.SelectedIndex = General.Settings.ReadPluginSetting("changeheightbysidedef", 0);
			editnewthing.Checked = General.Settings.ReadPluginSetting("editnewthing", true);
			editnewsector.Checked = General.Settings.ReadPluginSetting("editnewsector", false);
			additiveselect.Checked = General.Settings.ReadPluginSetting("additiveselect", false);
			stitchrange.Text = General.Settings.ReadPluginSetting("stitchrange", 20).ToString();
			highlightrange.Text = General.Settings.ReadPluginSetting("highlightrange", 20).ToString();
			highlightthingsrange.Text = General.Settings.ReadPluginSetting("highlightthingsrange", 10).ToString();
			splitlinedefsrange.Text = General.Settings.ReadPluginSetting("splitlinedefsrange", 10).ToString();
			mouseselectionthreshold.Text = General.Settings.ReadPluginSetting("mouseselectionthreshold", 2).ToString();
			splitbehavior.SelectedIndex = (int)General.Settings.SplitLineBehavior; //mxd
			autoclearselection.Checked = BuilderPlug.Me.AutoClearSelection;
			visualmodeclearselection.Checked = BuilderPlug.Me.VisualModeClearSelection;
			autodragonpaste.Checked = BuilderPlug.Me.AutoDragOnPaste;
			autoaligntexturesoncreate.Checked = BuilderPlug.Me.AutoAlignTextureOffsetsOnCreate; //mxd
			dontMoveGeometryOutsideBounds.Checked = BuilderPlug.Me.DontMoveGeometryOutsideMapBoundary; //mxd
			syncSelection.Checked = BuilderPlug.Me.SyncSelection; //mxd
			switchviewmodes.Checked = General.Settings.SwitchViewModes; //mxd
			autodrawonedit.Checked = BuilderPlug.Me.AutoDrawOnEdit;
			defaultbrightness.Text = General.Settings.DefaultBrightness.ToString(); //mxd
			defaultceilheight.Text = General.Settings.DefaultCeilingHeight.ToString();//mxd
			defaultfloorheight.Text = General.Settings.DefaultFloorHeight.ToString(); //mxd
		}

		#endregion

		#region ================== Events

		// When OK is pressed on the preferences dialog
		public void OnAccept(PreferencesController controller)
		{
			// Write preferred settings
			General.Settings.WritePluginSetting("changeheightbysidedef", heightbysidedef.SelectedIndex);
			General.Settings.WritePluginSetting("editnewthing", editnewthing.Checked);
			General.Settings.WritePluginSetting("editnewsector", editnewsector.Checked);
			General.Settings.WritePluginSetting("additiveselect", additiveselect.Checked);
			General.Settings.WritePluginSetting("stitchrange", stitchrange.GetResult(0));
			General.Settings.WritePluginSetting("highlightrange", highlightrange.GetResult(0));
			General.Settings.WritePluginSetting("highlightthingsrange", highlightthingsrange.GetResult(0));
			General.Settings.WritePluginSetting("splitlinedefsrange", splitlinedefsrange.GetResult(0));
			General.Settings.WritePluginSetting("mouseselectionthreshold", mouseselectionthreshold.GetResult(0));
			General.Settings.WritePluginSetting("autoclearselection", autoclearselection.Checked);
			General.Settings.WritePluginSetting("visualmodeclearselection", visualmodeclearselection.Checked);
			General.Settings.WritePluginSetting("autodragonpaste", autodragonpaste.Checked);
			General.Settings.WritePluginSetting("autodrawonedit", autodrawonedit.Checked); //mxd
			General.Settings.WritePluginSetting("autoaligntextureoffsetsoncreate", autoaligntexturesoncreate.Checked);//mxd
			General.Settings.WritePluginSetting("dontmovegeometryoutsidemapboundary", dontMoveGeometryOutsideBounds.Checked);//mxd
			General.Settings.WritePluginSetting("syncselection", syncSelection.Checked);//mxd
			General.Settings.SwitchViewModes = switchviewmodes.Checked; //mxd
			General.Settings.SplitLineBehavior = (SplitLineBehavior)splitbehavior.SelectedIndex;//mxd

			//default sector values
			General.Settings.DefaultBrightness = General.Clamp(defaultbrightness.GetResult(192), 0, 255);
			
			int ceilHeight = defaultceilheight.GetResult(128);
			int floorHeight = defaultfloorheight.GetResult(0);
			if(ceilHeight < floorHeight) General.Swap(ref ceilHeight, ref floorHeight);

			General.Settings.DefaultCeilingHeight = ceilHeight;
			General.Settings.DefaultFloorHeight = floorHeight;
		}
		
		// When Cancel is pressed on the preferences dialog
		public void OnCancel(PreferencesController controller)
		{
		}

		#endregion

		#region ================== Methods

		// This sets up the form with the preferences controller
		public void Setup(PreferencesController controller)
		{
			// Add tab pages
			foreach(TabPage p in tabs.TabPages)
			{
				controller.AddTab(p);
			}

			// Bind events
			controller.OnAccept += OnAccept;
			controller.OnCancel += OnCancel;
		}

		#endregion

		private void label17_Click(object sender, System.EventArgs e)
		{

		}

		private void label16_Click(object sender, System.EventArgs e)
		{

		}

		private void buttonsNumericTextbox1_Load(object sender, System.EventArgs e)
		{

		}
	}
}