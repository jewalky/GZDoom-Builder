using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Editing;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Map;

namespace CodeImp.DoomBuilder.Windows
{
    public partial class ThingStatisticsForm : DelayedForm
    {
        private static Size size = Size.Empty;
        private static Point location = Point.Empty;

        public ThingStatisticsForm() 
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

        private void Setup() 
		{
            Dictionary<int, int> thingcounts = new Dictionary<int, int>();
            Dictionary<int, string> thingtitles = new Dictionary<int, string>();
            Dictionary<int, string> thingclasses = new Dictionary<int, string>();
            dataGridView.Rows.Clear();

            foreach(ThingTypeInfo ti in General.Map.Data.ThingTypes) 
			{
                thingcounts.Add(ti.Index, 0);
                thingtitles.Add(ti.Index, ti.Title);
                thingclasses.Add(ti.Index, ti.ClassName);
            }

            foreach(Thing t in General.Map.Map.Things) 
			{
                if(thingcounts.ContainsKey(t.Type)) 
				{
                    thingcounts[t.Type]++;
                } 
				else 
				{
                    thingcounts.Add(t.Type, 1);
                    thingtitles.Add(t.Type, "Unknown thing");
                    thingclasses.Add(t.Type, "-");
                }
            }

            //add rows
            foreach(KeyValuePair<int, int> group in thingcounts) 
			{
                if(hideUnused.Checked && group.Value == 0) continue;
                
                DataGridViewRow row = new DataGridViewRow();

                row.Cells.Add(new DataGridViewTextBoxCell { Value = group.Key }); //type
                row.Cells.Add(new DataGridViewTextBoxCell { Value = thingtitles[group.Key] }); //title
                row.Cells.Add(new DataGridViewTextBoxCell { Value = thingclasses[group.Key] }); //class
                row.Cells.Add(new DataGridViewTextBoxCell { Value = group.Value }); //count

                dataGridView.Rows.Add(row);
            }

            dataGridView.Sort(ThingType, ListSortDirection.Ascending);
        }

        private static List<Thing> GetThingsByType(int type) 
		{
            List<Thing> list = new List<Thing>();
            foreach(Thing t in General.Map.Map.Things)
                if(t.Type == type) list.Add(t);

            return list;
        }

        private static void ShowSelection(List<Vector2D> points) 
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

        private void dataGridView_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e) 
		{
            if(e.RowIndex == -1) return;
			if(e.Button == MouseButtons.Left) //select
			{ 
                List<Thing> list = GetThingsByType((int)dataGridView.Rows[e.RowIndex].Cells[0].Value);
                if(list.Count > 0) 
				{
                    General.Map.Map.ClearSelectedThings();

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
                    General.Editing.ChangeMode("ThingsMode");
                    ShowSelection(points);
                }
            } 
			else if(e.Button == MouseButtons.Right) //edit
			{ 
                List<Thing> list = GetThingsByType((int)dataGridView.Rows[e.RowIndex].Cells[0].Value);
                if(list.Count > 0) 
				{
                    General.MainWindow.ShowEditThings(list);
                    General.Map.Map.Update();
                    Setup();
                }
            }
        }

        private void hideUnused_CheckedChanged(object sender, EventArgs e)
        {
            Setup();
        }

        private void apply_Click(object sender, EventArgs e) 
		{
            this.Close();
        }

        private void ThingStatisticsForm_FormClosing(object sender, FormClosingEventArgs e) 
		{
            size = this.Size;
            location = this.Location;
        }
    }
}
