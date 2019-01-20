using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Windows;

namespace CodeImp.DoomBuilder.SoundPropagationMode
{
	public partial class ReverbsPickerForm : DelayedForm
	{
		private struct ReverbListItem
		{
			private readonly string name;
			public readonly int Arg0;
			public readonly int Arg1;

			public ReverbListItem(string name, int arg0, int arg1) 
			{
				this.name = name + " (" + arg0 + " " + arg1 + ")";
				Arg0 = arg0;
				Arg1 = arg1;
			}

			public override string ToString() 
			{
				return name;
			}
		}

		private static string previousenvironmentname;
		
		public ReverbsPickerForm(Thing t) 
		{
			InitializeComponent();

			// Fill the list
			foreach(KeyValuePair<string, KeyValuePair<int, int>> reverb in General.Map.Data.Reverbs)
			{
				list.Items.Add(new ReverbListItem(reverb.Key, reverb.Value.Key, reverb.Value.Value));
			}

			// Select suitable item
			foreach(var item in list.Items) 
			{
				ReverbListItem rli = (ReverbListItem)item;
				if(rli.Arg0 == t.Args[0] && rli.Arg1 == t.Args[1]) 
				{
					list.SelectedItem = item;
					break;
				}
			}

			// Select previously selected item?
			if(!string.IsNullOrEmpty(previousenvironmentname) && list.SelectedItem == null)
			{
				foreach(ReverbListItem item in list.Items)
				{
					if(item.ToString() == previousenvironmentname)
					{
						list.SelectedItem = item;
						break;
					}
				}
			}

			// Dormant?
			cbactiveenv.Checked = !BuilderPlug.ThingDormant(t);
			list.Focus();
		}

		public void ApplyTo(Thing t) 
		{
			if(list.SelectedItem == null) return;
			ReverbListItem rli = (ReverbListItem)list.SelectedItem;
			t.Args[0] = rli.Arg0;
			t.Args[1] = rli.Arg1;
			BuilderPlug.SetThingDormant(t, !cbactiveenv.Checked);
		}

		private void list_MouseDoubleClick(object sender, MouseEventArgs e) 
		{
			if(list.SelectedItem != null) accept_Click(accept, EventArgs.Empty);
		}

		private void accept_Click(object sender, EventArgs e) 
		{
			if(list.SelectedItem != null) previousenvironmentname = list.SelectedItem.ToString();
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void list_SelectedIndexChanged(object sender, EventArgs e)
		{
			accept.Enabled = (list.SelectedItem != null);
		}
	}
}
