using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Windows;

namespace CodeImp.DoomBuilder.BuilderModes
{
	public partial class FilterSelectedThingsForm : DelayedForm
	{
		private static Size size = Size.Empty;
		private static Point location = Point.Empty;
		private ICollection<Thing> selection;
		private ThingsMode mode;

		public FilterSelectedThingsForm(ICollection<Thing> selection, ThingsMode mode) 
		{
			InitializeComponent();
			this.mode = mode;

			//apply window size and location
			if(!size.IsEmpty && !location.IsEmpty) 
			{
				this.StartPosition = FormStartPosition.Manual;
				this.Size = size;
				this.Location = location;
			}

			Setup(selection);
		}

		private void Setup(ICollection<Thing> selection) 
		{
			this.selection = selection;

			//get thing types
			Dictionary<int, int> thingcounts = new Dictionary<int, int>();
			Dictionary<int, string> thingtitles = new Dictionary<int, string>();

			foreach(Thing t in selection) 
			{
				if(!thingcounts.ContainsKey(t.Type)) 
				{
					thingcounts.Add(t.Type, 1);
					ThingTypeInfo ti = General.Map.Data.GetThingInfo(t.Type);
					thingtitles.Add(t.Type, ti.Title);
				} 
				else 
				{
					thingcounts[t.Type]++;
				}
			}

			//add data
			foreach(KeyValuePair<int, int> group in thingcounts) 
			{
				DataGridViewRow row = new DataGridViewRow();

				row.Cells.Add(new DataGridViewTextBoxCell { Value = group.Key }); //type
				row.Cells.Add(new DataGridViewTextBoxCell { Value = thingtitles[group.Key] }); //title
				row.Cells.Add(new DataGridViewTextBoxCell { Value = group.Value }); //count

				dataGridView.Rows.Add(row);
			}

			dataGridView.Sort(ThingType, ListSortDirection.Ascending);
		}

		private void apply_Click(object sender, EventArgs e) 
		{
			//get selected types
			List<int> selectedtypes = new List<int>();

			foreach(DataGridViewRow row in dataGridView.Rows)
				if(row.Selected) selectedtypes.Add((int)row.Cells[0].Value);

			//apply selection
			if(selectedtypes.Count > 0) 
			{
				foreach(Thing t in selection) 
				{
					if(!selectedtypes.Contains(t.Type)) t.Selected = false;
				}

				//update display
				mode.UpdateSelectionInfo();
				General.Interface.RedrawDisplay();
			}
			
			this.Close();
		}

		private void cancel_Click(object sender, EventArgs e) 
		{
			this.Close();
		}

		private void FilterSelectedThingsForm_FormClosing(object sender, FormClosingEventArgs e) 
		{
			size = this.Size;
			location = this.Location;
		}
		
	}
}
