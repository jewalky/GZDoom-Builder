#region ================== Namespaces

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Editing;

#endregion

namespace CodeImp.DoomBuilder.Windows
{
	public partial class TagStatisticsForm : DelayedForm
	{
		private static Size size = Size.Empty;
		private static Point location = Point.Empty;

		#region ================== Constructor

		public TagStatisticsForm() 
		{
			InitializeComponent();

			//apply window size and location
			if(!size.IsEmpty && !location.IsEmpty)
			{
				this.StartPosition = FormStartPosition.Manual;
				this.Size = size;
				this.Location = location;
			}

			Setup();
		}

		#endregion

		#region ================== Methods

		private void Setup() 
		{
			//collect all tags
			HashSet<int> tags = new HashSet<int>();
			Dictionary<int, int> sectorsCountByTag = new Dictionary<int, int>();
			Dictionary<int, int> linedefsCountByTag = new Dictionary<int, int>();
			Dictionary<int, int> thingsCountByTag = new Dictionary<int, int>();

			//collect used tags from sectors...
			foreach(Sector s in General.Map.Map.Sectors) 
			{
				if(s.Tag == 0) continue;
				tags.UnionWith(s.Tags);
				foreach(int tag in s.Tags)
				{
					if(!sectorsCountByTag.ContainsKey(tag)) sectorsCountByTag.Add(tag, 0);
					sectorsCountByTag[tag]++;
				}
			}

			//...and linedefs...
			if(General.Map.FormatInterface.HasLinedefTag) 
			{
				foreach(Linedef l in General.Map.Map.Linedefs) 
				{
					if(l.Tag == 0) continue;
					tags.UnionWith(l.Tags);
					foreach(int tag in l.Tags)
					{
						if(!linedefsCountByTag.ContainsKey(tag)) linedefsCountByTag.Add(tag, 0);
						linedefsCountByTag[tag]++;
					}
				}
			} 
			else 
			{
				Linedefs.Visible = false;
			}

			//...and things...
			if(General.Map.FormatInterface.HasThingTag) 
			{
				foreach(Thing t in General.Map.Map.Things) 
				{
					if(t.Tag == 0) continue;
					if(!tags.Contains(t.Tag)) tags.Add(t.Tag);

					if(!thingsCountByTag.ContainsKey(t.Tag)) thingsCountByTag.Add(t.Tag, 0);
					thingsCountByTag[t.Tag]++;
				}
			} 
			else 
			{
				Things.Visible = false;
			}

			//create rows
			dataGridView.Rows.Clear();
			foreach(int tag in tags) 
			{
				AddRow(tag,
					General.Map.Options.TagLabels.ContainsKey(tag) ? General.Map.Options.TagLabels[tag] : string.Empty,
					sectorsCountByTag.ContainsKey(tag) ? sectorsCountByTag[tag] : 0,
					linedefsCountByTag.ContainsKey(tag) ? linedefsCountByTag[tag] : 0,
					thingsCountByTag.ContainsKey(tag) ? thingsCountByTag[tag] : 0);
			}

			dataGridView.Sort(TagColumn, ListSortDirection.Ascending);
		}

		private void AddRow(int tag, string label, int sectorsCount, int linesCount, int thingsCount) 
		{
			DataGridViewRow row = new DataGridViewRow();

			row.Cells.Add(new DataGridViewTextBoxCell { Value = tag });
			row.Cells.Add(new DataGridViewTextBoxCell { Value = label });
			row.Cells.Add(new DataGridViewTextBoxCell { Value = sectorsCount });
			row.Cells.Add(new DataGridViewTextBoxCell { Value = linesCount });
			row.Cells.Add(new DataGridViewTextBoxCell { Value = thingsCount });

			dataGridView.Rows.Add(row);
		}

		private static List<Sector> GetSectorsWithTag(int tag, int count) 
		{
			List<Sector> list = new List<Sector>();
			foreach(Sector s in General.Map.Map.Sectors) 
			{
				if(s.Tags.Contains(tag))
				{
					list.Add(s);
					if(list.Count == count) break;
				}
			}

			return list;
		}

		private static List<Linedef> GetLinedefsWithTag(int tag, int count) 
		{
			List<Linedef> list = new List<Linedef>();
			foreach(Linedef l in General.Map.Map.Linedefs) 
			{
				if(l.Tags.Contains(tag))
				{
					list.Add(l);
					if(list.Count == count) break;
				}
			}

			return list;
		}

		private static List<Thing> GetThingsWithTag(int tag, int count) 
		{
			List<Thing> list = new List<Thing>();
			foreach(Thing t in General.Map.Map.Things) 
			{
				if(t.Tag == tag) 
				{
					list.Add(t);
					if(list.Count == count) break;
				}
			}

			return list;
		}

		private static void ShowSelection(IEnumerable<Vector2D> points) 
		{
			RectangleF area = MapSet.CreateEmptyArea();
			
			// Make a view area from the points
			foreach(Vector2D p in points) area = MapSet.IncreaseArea(area, p);

			// Make the area square, using the largest side
			if(area.Width > area.Height) 
			{
				float delta = area.Width - area.Height;
				area.Y -= delta * 0.5f;
				area.Height += delta;
			} 
			else 
			{
				float delta = area.Height - area.Width;
				area.X -= delta * 0.5f;
				area.Width += delta;
			}

			// Add padding
			area.Inflate(100f, 100f);

			// Zoom to area
			ClassicMode mode = General.Editing.Mode as ClassicMode;
			if(mode != null) mode.CenterOnArea(area, 0.6f);
		}

		#endregion

		#region ================== Events

		private void apply_Click(object sender, EventArgs e) 
		{
			// Refill TagLabels with table data
			dataGridView.Sort(TagColumn, ListSortDirection.Ascending);
			General.Map.Options.TagLabels.Clear();

			foreach(DataGridViewRow row in dataGridView.Rows) 
			{
				string label = (string)row.Cells[1].Value;
				if(!string.IsNullOrEmpty(label))
					General.Map.Options.TagLabels.Add((int)row.Cells[0].Value, label);
			}
			
			this.Close();
		}

		private void cancel_Click(object sender, EventArgs e) 
		{
			this.Close();
		}

		private void dataGridView_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e) 
		{
			if(e.ColumnIndex < 2 || e.RowIndex == -1) return;
			
			//select 
			if(e.Button == MouseButtons.Left) 
			{
				int tag = (int)dataGridView.Rows[e.RowIndex].Cells[0].Value;

				if(e.ColumnIndex == 2) //sectors
				{
					// Deselect everything
					General.Map.Map.ClearAllSelected();
					
					List<Sector> list = GetSectorsWithTag(tag, (int)dataGridView.Rows[e.RowIndex].Cells[2].Value);
					if(list.Count > 0) 
					{
						List<Vector2D> points = new List<Vector2D>();
						General.Editing.ChangeMode("SectorsMode");
						ClassicMode mode = (ClassicMode)General.Editing.Mode;

						foreach(Sector s in list) 
						{
							mode.SelectMapElement(s);

							foreach(Sidedef sd in s.Sidedefs) 
							{
								points.Add(sd.Line.Start.Position);
								points.Add(sd.Line.End.Position);
							}
						}

						ShowSelection(points);
					}
					else
					{
						General.Interface.RedrawDisplay();
					}
				} 
				else if(e.ColumnIndex == 3) //linedefs
				{
					// Deselect everything
					General.Map.Map.ClearAllSelected();
					
					List<Linedef> list = GetLinedefsWithTag(tag, (int)dataGridView.Rows[e.RowIndex].Cells[3].Value);
					if(list.Count > 0) 
					{
						General.Editing.ChangeMode("LinedefsMode");
						List<Vector2D> points = new List<Vector2D>();
						foreach(Linedef l in list) 
						{
							l.Selected = true;
							points.Add(l.Start.Position);
							points.Add(l.End.Position);
						}

						General.Map.Map.Update();
						ShowSelection(points);
					}
					else
					{
						General.Interface.RedrawDisplay();
					}
				} 
				else if(e.ColumnIndex == 4) //things
				{
					// Deselect everything
					General.Map.Map.ClearAllSelected();
					
					List<Thing> list = GetThingsWithTag(tag, (int)dataGridView.Rows[e.RowIndex].Cells[4].Value);
					if(list.Count > 0)
					{
						General.Editing.ChangeMode("ThingsMode");
						List<Vector2D> points = new List<Vector2D>();
						foreach(Thing t in list)
						{
							t.Selected = true;

							Vector2D p = t.Position;
							points.Add(p);
							points.Add(p + new Vector2D(t.Size * 2.0f, t.Size * 2.0f));
							points.Add(p + new Vector2D(t.Size * 2.0f, -t.Size * 2.0f));
							points.Add(p + new Vector2D(-t.Size * 2.0f, t.Size * 2.0f));
							points.Add(p + new Vector2D(-t.Size * 2.0f, -t.Size * 2.0f));
						}

						General.Map.Map.Update();
						ShowSelection(points);
					}
					else
					{
						General.Interface.RedrawDisplay();
					}
				}
			} 
			else if(e.Button == MouseButtons.Right) //open properties window
			{
				dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Selected = true;
				int tag = (int)dataGridView.Rows[e.RowIndex].Cells[0].Value;

				if(e.ColumnIndex == 2) //sectors
				{ 
					List<Sector> list = GetSectorsWithTag(tag, (int)dataGridView.Rows[e.RowIndex].Cells[2].Value);
					if(list.Count > 0) 
					{
						General.MainWindow.ShowEditSectors(list);
						General.Map.Map.Update();
						Setup();
					}
				} 
				else if(e.ColumnIndex == 3) //linedefs
				{ 
					List<Linedef> list = GetLinedefsWithTag(tag, (int)dataGridView.Rows[e.RowIndex].Cells[3].Value);
					if(list.Count > 0) 
					{
						General.MainWindow.ShowEditLinedefs(list);
						General.Map.Map.Update();
						Setup();
					}
				} 
				else if(e.ColumnIndex == 4) //things
				{ 
					List<Thing> list = GetThingsWithTag(tag, (int)dataGridView.Rows[e.RowIndex].Cells[4].Value);
					if(list.Count > 0) 
					{
						General.MainWindow.ShowEditThings(list);
						General.Map.Map.Update();
						Setup();
					}
				}
			}
		}

		private void TagStatisticsForm_FormClosing(object sender, FormClosingEventArgs e) 
		{
			size = this.Size;
			location = this.Location;
		}

		#endregion
	}
}
