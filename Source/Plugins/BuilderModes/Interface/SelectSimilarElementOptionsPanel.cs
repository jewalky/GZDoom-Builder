using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Controls;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Windows;

namespace CodeImp.DoomBuilder.BuilderModes
{
	public partial class SelectSimilarElementOptionsPanel : DelayedForm
	{
		private static Point location = Point.Empty;
		private static readonly object[] flags = {
			                                new SectorPropertiesCopySettings(),
			                                new LinedefPropertiesCopySettings(),
			                                new SidedefPropertiesCopySettings(),
			                                new ThingPropertiesCopySettings(),
			                                new VertexPropertiesCopySettings()
		                                };

		private BaseClassicMode mode;
		private List<TabPage> showntabs;
		private readonly Dictionary<CheckboxArrayControl, object> typecontrols;
		
		public SelectSimilarElementOptionsPanel() 
		{
			InitializeComponent();

			//apply window size and location
			if(!location.IsEmpty) 
			{
				this.StartPosition = FormStartPosition.Manual;
				this.Location = location;
			}

			//create a collection
			typecontrols = new Dictionary<CheckboxArrayControl, object>
			{
				{ sectorflags, flags[0] },
				{ lineflags, flags[1] },
				{ sideflags, flags[2] },
				{ thingflags, flags[3] },
				{ vertexflags, flags[4] }
			};
		}

		public bool Setup(BaseClassicMode mode) 
		{
			this.mode = mode;

			//which tabs should we display?
			TabPage[] activetabs = null;
			if(General.Editing.Mode is ThingsMode)
			{
				if(General.Map.Map.GetSelectedThings(true).Count == 0) return SetupFailed("This action requires selection...");
				activetabs = new[] { things };
			} 
			else if(General.Editing.Mode is VerticesMode && General.Map.UDMF) 
			{
				if(General.Map.Map.GetSelectedVertices(true).Count == 0) return SetupFailed("This action requires selection...");
				activetabs = new[] { vertices };
			} 
			else if(General.Editing.Mode is LinedefsMode) 
			{
				if(General.Map.Map.GetSelectedLinedefs(true).Count == 0) return SetupFailed("This action requires selection...");
				activetabs = new[] { linedefs, sidedefs };
			} 
			else if(mode is SectorsMode) 
			{
				if(General.Map.Map.GetSelectedSectors(true).Count == 0) return SetupFailed("This action requires selection...");
				activetabs = new[] { sectors };
			}

			if(activetabs == null) return SetupFailed("This action doesn't support current editing mode...");

			//fill flags
			showntabs = new List<TabPage>();
			int maxheight = int.MinValue;
			foreach(TabPage page in activetabs) 
			{
				CheckboxArrayControl curControl = page.Controls[0] as CheckboxArrayControl;
				if(curControl == null) continue; //just a piece of boilerplate...

				FieldInfo[] props = typecontrols[curControl].GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
				foreach(var prop in props) 
				{
					foreach(Attribute attr in Attribute.GetCustomAttributes(prop)) 
					{
						if(attr.GetType() == typeof(FieldDescription)) 
						{
							FieldDescription fd = (FieldDescription)attr;
							if(fd.SupportsCurrentMapFormat)
								curControl.Add(fd.Description, prop.Name).Checked = (bool)prop.GetValue(typecontrols[curControl]);
						}
					}
				}

				if(curControl.Checkboxes.Count > 0)
				{
					curControl.PositionCheckboxes();
					showntabs.Add(page);

					// Store height
					maxheight = Math.Max(maxheight, curControl.GetHeight());
				}
			}

			// Apply height
			if(maxheight != int.MinValue)
			{
				this.Height += maxheight - activetabs[0].Controls[0].Height;
			}

			// Got anything to show?
			if(showntabs.Count == 0) return SetupFailed("This action doesn't support current editing mode...");

			// Hide unused tab pages
			tabControl.TabPages.Clear();
			tabControl.TabPages.AddRange(showntabs.ToArray());
			tabControl.SelectTab(showntabs[0]);

			return true;
		}

		private static bool SetupFailed(string message)
		{
			General.Interface.DisplayStatus(StatusType.Warning, message);
			return false;
		}

		private void SelectSimilarElementOptionsPanel_FormClosing(object sender, FormClosingEventArgs e) 
		{
			location = this.Location;
		}

		private void enableall_Click(object sender, EventArgs e) 
		{
			CheckboxArrayControl curControl = tabControl.SelectedTab.Controls[0] as CheckboxArrayControl;
			if(curControl == null) return; //just a piece of boilerplate...
			bool enable = !curControl.Checkboxes[0].Checked;
			foreach(var cb in curControl.Checkboxes) cb.Checked = enable;
		}

		private void cancel_Click(object sender, EventArgs e) 
		{
			this.Close();
		}

		private void apply_Click(object sender, EventArgs e) 
		{
			//save flags states
			foreach(TabPage page in showntabs)
			{
				CheckboxArrayControl curControl = page.Controls[0] as CheckboxArrayControl;
				if(curControl == null) continue; //just a piece of boilerplate...

				FieldInfo[] props = typecontrols[curControl].GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
				var fields = new Dictionary<string, FieldInfo>(props.Length);
				for(int i = 0; i < props.Length; i++) fields[props[i].Name] = props[i];
				foreach(CheckBox cb in curControl.Checkboxes) fields[cb.Tag.ToString()].SetValue(typecontrols[curControl], cb.Checked);
			}

			//perform selection
			if(mode is ThingsMode) 
			{
				ICollection<Thing> selected = General.Map.Map.GetSelectedThings(true);
				ICollection<Thing> unselected = General.Map.Map.GetSelectedThings(false);

				foreach(Thing target in unselected) 
				{
					foreach(Thing source in selected) 
					{
						if(PropertiesComparer.PropertiesMatch((ThingPropertiesCopySettings) typecontrols[thingflags], source, target))
							mode.SelectMapElement(target);
					}
				}
			} 
			else if(mode is LinedefsMode) 
			{
				ICollection<Linedef> selected = General.Map.Map.GetSelectedLinedefs(true);
				ICollection<Linedef> unselected = General.Map.Map.GetSelectedLinedefs(false);

				foreach(Linedef target in unselected) 
				{
					foreach(Linedef source in selected) 
					{
						if(PropertiesComparer.PropertiesMatch((LinedefPropertiesCopySettings)typecontrols[lineflags], (SidedefPropertiesCopySettings)typecontrols[sideflags], source, target))
							mode.SelectMapElement(target);
					}
				}
			} 
			else if(mode is SectorsMode) 
			{
				ICollection<Sector> selected = General.Map.Map.GetSelectedSectors(true);
				ICollection<Sector> unselected = General.Map.Map.GetSelectedSectors(false);

				foreach(Sector target in unselected) 
				{
					foreach(Sector source in selected) 
					{
						if(PropertiesComparer.PropertiesMatch((SectorPropertiesCopySettings)typecontrols[sectorflags], source, target))
							mode.SelectMapElement(target);
					}
				}
			} 
			else if(mode is VerticesMode) 
			{
				ICollection<Vertex> selected = General.Map.Map.GetSelectedVertices(true);
				ICollection<Vertex> unselected = General.Map.Map.GetSelectedVertices(false);

				foreach(Vertex target in unselected) 
				{
					foreach(Vertex source in selected) 
					{
						if(PropertiesComparer.PropertiesMatch((VertexPropertiesCopySettings)typecontrols[vertexflags], source, target))
							mode.SelectMapElement(target);
					}
				}

			}

			mode.UpdateSelectionInfo();
			General.Interface.RedrawDisplay();
			this.Close();
		}
	}
}
