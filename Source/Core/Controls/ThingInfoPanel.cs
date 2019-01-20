
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

using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Data;
using CodeImp.DoomBuilder.GZBuilder.Data;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Types;
using CodeImp.DoomBuilder.GZBuilder; //mxd
using System.Collections.Generic;

#endregion

namespace CodeImp.DoomBuilder.Controls
{
	internal partial class ThingInfoPanel : UserControl
	{
		private readonly int hexenformatwidth;
		private readonly int doomformatwidth;

		// Constructor
		public ThingInfoPanel()
		{
			// Initialize
			InitializeComponent();

			// Hide stuff when in Doom format
			hexenformatwidth = infopanel.Width;
			doomformatwidth = infopanel.Width - 190;
		}

		// This shows the info
		public void ShowInfo(Thing t)
		{
			// Show/hide stuff depending on format
			bool hasArgs = General.Map.FormatInterface.HasActionArgs;
			arglbl1.Visible = hasArgs;
			arglbl2.Visible = hasArgs;
			arglbl3.Visible = hasArgs;
			arglbl4.Visible = hasArgs;
			arglbl5.Visible = hasArgs;
			arg1.Visible = hasArgs;
			arg2.Visible = hasArgs;
			arg3.Visible = hasArgs;
			arg4.Visible = hasArgs;
			arg5.Visible = hasArgs;
			infopanel.Width = (hasArgs ? hexenformatwidth : doomformatwidth);

			//mxd
			action.Visible = General.Map.FormatInterface.HasThingAction;
			labelaction.Visible = General.Map.FormatInterface.HasThingAction;

			// Move panel
			spritepanel.Left = infopanel.Left + infopanel.Width + infopanel.Margin.Right + spritepanel.Margin.Left;
			flagsPanel.Left = spritepanel.Left + spritepanel.Width + spritepanel.Margin.Right + flagsPanel.Margin.Left; //mxd
			
			// Lookup thing info
			ThingTypeInfo ti = General.Map.Data.GetThingInfo(t.Type);

			// Get thing action information
			LinedefActionInfo act;
			if(General.Map.Config.LinedefActions.ContainsKey(t.Action)) act = General.Map.Config.LinedefActions[t.Action];
			else if(t.Action == 0) act = new LinedefActionInfo(0, "None", true, false);
			else act = new LinedefActionInfo(t.Action, "Unknown", false, false);
			string actioninfo = act.ToString();
			
			// Determine z info to show
			t.DetermineSector();
			string zinfo;
			if(ti.AbsoluteZ || t.Sector == null)
			{
				zinfo = t.Position.z.ToString(CultureInfo.InvariantCulture) + " (abs.)"; //mxd
			}
			else
			{
				// Hangs from ceiling?
				if(ti.Hangs)
					zinfo = t.Position.z + " (" + ((float)Math.Round(Sector.GetCeilingPlane(t.Sector).GetZ(t.Position) - t.Position.z - ti.Height, General.Map.FormatInterface.VertexDecimals)).ToString(CultureInfo.InvariantCulture) + ")"; //mxd
				else
					zinfo = t.Position.z + " (" + ((float)Math.Round(Sector.GetFloorPlane(t.Sector).GetZ(t.Position) + t.Position.z, General.Map.FormatInterface.VertexDecimals)).ToString(CultureInfo.InvariantCulture) + ")"; //mxd
			}

			// Thing info
			infopanel.Text = " Thing " + t.Index + " ";
			type.Text = t.Type + " - " + ti.Title;
			if(ti.IsObsolete) type.Text += " - OBSOLETE"; //mxd
			action.Text = actioninfo;
			bool displayclassname = !string.IsNullOrEmpty(ti.ClassName) && !ti.ClassName.StartsWith("$"); //mxd
			labelclass.Enabled = displayclassname; //mxd
			classname.Enabled = displayclassname; //mxd
			classname.Text = (displayclassname ? ti.ClassName : "--"); //mxd
			position.Text = t.Position.x.ToString(CultureInfo.InvariantCulture) + ", " + t.Position.y.ToString(CultureInfo.InvariantCulture) + ", " + zinfo;
			tag.Text = t.Tag + (General.Map.Options.TagLabels.ContainsKey(t.Tag) ? " - " + General.Map.Options.TagLabels[t.Tag] : string.Empty);
			angle.Text = t.AngleDoom + "\u00B0";
			anglecontrol.Angle = t.AngleDoom;
			anglecontrol.Left = angle.Right + 1;
			
			// Sprite
			if(ti.Sprite.ToLowerInvariant().StartsWith(DataManager.INTERNAL_PREFIX) && (ti.Sprite.Length > DataManager.INTERNAL_PREFIX.Length))
			{
				spritename.Text = "";
				spritetex.Image = General.Map.Data.GetSpriteImage(ti.Sprite).GetBitmap();
			}
			else if((ti.Sprite.Length <= 8) && (ti.Sprite.Length > 0))
			{
				spritename.Text = ti.Sprite;
				spritetex.Image = General.Map.Data.GetSpriteImage(ti.Sprite).GetPreview();
			}
			else
			{
				spritename.Text = "";
				spritetex.Image = null;
			}

			// Arguments
			ArgumentInfo[] arginfo = ((t.Action == 0 && ti.Args[0] != null) ? ti.Args : act.Args); //mxd

            //mxd. ACS script argument names
            bool isacsscript = (Array.IndexOf(GZGeneral.ACS_SPECIALS, t.Action) != -1);
            bool isarg0str = (General.Map.UDMF && t.Fields.ContainsKey("arg0str"));
            string arg0str = isarg0str ? t.Fields.GetValue("arg0str", string.Empty) : string.Empty;
            ScriptItem scriptitem = null;

            //mxd. Set default label colors
            arg1.ForeColor = SystemColors.ControlText;
            arglbl1.ForeColor = SystemColors.ControlText;

            // Named script?
            if (isacsscript && isarg0str && General.Map.NamedScripts.ContainsKey(arg0str.ToLowerInvariant()))
            {
                scriptitem = General.Map.NamedScripts[arg0str.ToLowerInvariant()];
            }
            // Script number?
            else if (isacsscript && General.Map.NumberedScripts.ContainsKey(t.Args[0]))
            {
                scriptitem = General.Map.NumberedScripts[t.Args[0]];
                arg0str = (scriptitem.HasCustomName ? scriptitem.Name : scriptitem.Index.ToString());
            }

            // Apply script args?
            Label[] arglabels = { arglbl1, arglbl2, arglbl3, arglbl4, arglbl5 };
            Label[] args = { arg1, arg2, arg3, arg4, arg5 };

            if (scriptitem != null)
            {
                int first;
                string[] argnames = scriptitem.GetArgumentsDescriptions(t.Action, out first);
                for (int i = 0; i < first; i++)
                {
                    arglabels[i].Text = (isarg0str ? arginfo[i].TitleStr : arginfo[i].Title) + ":";
                    arglabels[i].Enabled = arginfo[i].Used;
                    args[i].Enabled = arginfo[i].Used;
                }

                for (int i = first; i < argnames.Length; i++)
                {
                    if (!string.IsNullOrEmpty(argnames[i]))
                    {
                        arglabels[i].Text = argnames[i] + ":";
                        arglabels[i].Enabled = true;
                        args[i].Enabled = true;
                    }
                    else
                    {
                        arglabels[i].Text = (isarg0str ? arginfo[i].TitleStr : arginfo[i].Title) + ":";
                        arglabels[i].Enabled = arginfo[i].Used;
                        args[i].Enabled = arginfo[i].Used;
                    }
                }
            }
            else
            {
                for (int i = 0; i < arginfo.Length; i++)
                {
                    arglabels[i].Text = (isarg0str ? arginfo[i].TitleStr : arginfo[i].Title) + ":";
                    arglabels[i].Enabled = arginfo[i].Used;
                    args[i].Enabled = arginfo[i].Used;
                }

                // Special cases: unknown script name/index
                if (isacsscript)
                {
                    arglbl1.Text = "Unknown script " + (isarg0str ? "name" : "number") + ":";
                    arg1.ForeColor = Color.DarkRed;
                    arglbl1.ForeColor = Color.DarkRed;
                }
            }

            //mxd. Set argument value and label
            if (isarg0str) arg1.Text = arg0str;
            else SetArgumentText(act.Args[0], arg1, t.Args[0]);
            SetArgumentText(arginfo[1], arg2, t.Args[1]);
			SetArgumentText(arginfo[2], arg3, t.Args[2]);
			SetArgumentText(arginfo[3], arg4, t.Args[3]);
			SetArgumentText(arginfo[4], arg5, t.Args[4]);

			//mxd. Flags
			flags.Items.Clear();
			Dictionary<string, string> flagsrename = ti.FlagsRename;
			foreach(KeyValuePair<string, string> group in General.Map.Config.ThingFlags)
			{
				if(t.Flags.ContainsKey(group.Key) && t.Flags[group.Key])
				{
					ListViewItem lvi = (flagsrename != null && flagsrename.ContainsKey(group.Key)) 
						? new ListViewItem(flagsrename[group.Key]) { ForeColor = SystemColors.HotTrack } 
						: new ListViewItem(group.Value);
					lvi.Checked = true;
					flags.Items.Add(lvi);
				}
			}

			//mxd. Flags panel visibility and size
			flagsPanel.Visible = (flags.Items.Count > 0);
			if(flags.Items.Count > 0)
			{
				Rectangle rect = flags.GetItemRect(0);
				int itemspercolumn = 1;
				
				// Check how many items per column we have...
				for(int i = 1; i < flags.Items.Count; i++)
				{
					if(flags.GetItemRect(i).X != rect.X) break;
					itemspercolumn++;
				}

				flags.Width = rect.Width * (int)Math.Ceiling(flags.Items.Count / (float)itemspercolumn);
				flagsPanel.Width = flags.Width + flags.Left * 2;
			}

			// Show the whole thing
			this.Show();
			//this.Update(); // ano - don't think this is needed, and is slow
		}

		//mxd
		private static void SetArgumentText(ArgumentInfo info, Label label, int value) 
		{
			TypeHandler th = General.Types.GetArgumentHandler(info);
			th.SetValue(value);
			label.Text = th.GetStringValue();
		}

		// When visible changed
		protected override void OnVisibleChanged(EventArgs e)
		{
			// Hiding panels
			if(!this.Visible)
			{
				spritetex.Image = null;
			}

			// Call base
			base.OnVisibleChanged(e);
		}
	}
}
