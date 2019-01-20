#region ================== Namespaces

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Controls;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Windows;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes.Interface
{
	public partial class PastePropertiesOptionsForm : DelayedForm
	{
		#region ================== Variables

		private static Size size = Size.Empty;
		private static Point location = Point.Empty;
		private Dictionary<object, CheckboxArrayControl> typecontrols;

		#endregion

		#region ================== Constructor / Setup

		public PastePropertiesOptionsForm() 
		{
			InitializeComponent();

			// Apply window size and location
			if(!size.IsEmpty && !location.IsEmpty)
			{
				this.StartPosition = FormStartPosition.Manual;
				this.Size = size;
				this.Location = location;
			}
		}

		public bool Setup(MapElementType targetmapelementtype) { return Setup(new List<MapElementType> { targetmapelementtype }); }
		public bool Setup(IEnumerable<MapElementType> targetmapelementtypes)
		{
			// Create collections
			typecontrols = new Dictionary<object, CheckboxArrayControl>();
			var tabcontrols = new Dictionary<TabPage, CheckboxArrayControl>();
			
			// Add appropriate controls
			foreach(MapElementType t in targetmapelementtypes)
			{
				switch(t)
				{
					case MapElementType.THING:
						if(BuilderPlug.Me.CopiedThingProps != null)
						{
							typecontrols.Add(ThingProperties.CopySettings, thingflags);
							tabcontrols.Add(things, thingflags);
						}
						break;

					case MapElementType.SECTOR:
						if(BuilderPlug.Me.CopiedSectorProps != null)
						{
							typecontrols.Add(SectorProperties.CopySettings, sectorflags);
							tabcontrols.Add(sectors, sectorflags);
						}
						break;

					case MapElementType.LINEDEF:
					case MapElementType.SIDEDEF:
						if(BuilderPlug.Me.CopiedSidedefProps != null || BuilderPlug.Me.CopiedLinedefProps != null)
						{
							typecontrols.Add(LinedefProperties.CopySettings, lineflags);
							typecontrols.Add(SidedefProperties.CopySettings, sideflags);
							tabcontrols.Add(linedefs, lineflags);
							tabcontrols.Add(sidedefs, sideflags);
						}
						break;

					case MapElementType.VERTEX:
						if(BuilderPlug.Me.CopiedVertexProps != null)
						{
							typecontrols.Add(VertexProperties.CopySettings, vertexflags);
							tabcontrols.Add(vertices, vertexflags);
						}
						break;

					default:
						throw new NotImplementedException("Unknown map element type: " + t);
				}
			}

			// Got anything to show?
			if(typecontrols.Count == 0)
			{
				General.Interface.DisplayStatus(StatusType.Warning, "No copied properties to apply!");
				return false;
			}

			// Fill flags
			FillFlags();

			// Select proper tab
			if(!ShowTabs(tabcontrols))
			{
				General.Interface.DisplayStatus(StatusType.Warning, "Current map format doesn't support any properties for selected map elements!");
				return false;
			}

			return true;
		}

		#endregion

		#region ================== Methods

		private bool ShowTabs(Dictionary<TabPage, CheckboxArrayControl> pageslist)
		{
			List<TabPage> toshow = new List<TabPage>();
			foreach(TabPage page in tabcontrol.TabPages)
			{
				if(pageslist.ContainsKey(page) && pageslist[page].Checkboxes.Count > 0) toshow.Add(page); 
			}

			if(toshow.Count == 0) return false;

			tabcontrol.TabPages.Clear();
			tabcontrol.TabPages.AddRange(toshow.ToArray());
			tabcontrol.SelectTab(toshow[0]);

			return true;
		}

		private void FillFlags()
		{
			// Fill flags
			foreach(KeyValuePair<object, CheckboxArrayControl> group in typecontrols)
			{
				FieldInfo[] props = group.Key.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
				foreach(FieldInfo prop in props)
				{
					foreach(Attribute attr in Attribute.GetCustomAttributes(prop))
					{
						if(attr.GetType() == typeof(FieldDescription))
						{
							FieldDescription fd = (FieldDescription)attr;
							if(fd.SupportsCurrentMapFormat)
							{
								group.Value.Add(fd.Description, prop.Name).Checked = (bool)prop.GetValue(group.Key);
							}
							break;
						}
					}
				}

				if(group.Value.Checkboxes.Count > 0) group.Value.PositionCheckboxes();
			}
		}

		#endregion

		#region ================== Events

		private void apply_Click(object sender, EventArgs e) 
		{
			foreach(KeyValuePair<object, CheckboxArrayControl> group in typecontrols) 
			{
				FieldInfo[] props = group.Key.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
				var fields = new Dictionary<string, FieldInfo>(props.Length);
				for(int i = 0; i < props.Length; i++) fields[props[i].Name] = props[i];
				foreach(CheckBox cb in group.Value.Checkboxes) fields[cb.Tag.ToString()].SetValue(group.Key, cb.Checked);
			}

			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void cancel_Click(object sender, EventArgs e) 
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		private void enableall_Click(object sender, EventArgs e)
		{
			var cc = tabcontrol.SelectedTab.Controls[0] as CheckboxArrayControl;
			if(cc == null) return; //just a piece of boilerplate...
			bool enable = !cc.Checkboxes[0].Checked;
			foreach(var cb in cc.Checkboxes) cb.Checked = enable;
		}

		private void PastePropertiesOptionsForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			size = this.Size;
			location = this.Location;
		}

		#endregion
	}
}