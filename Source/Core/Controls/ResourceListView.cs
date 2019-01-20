
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
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

#endregion

namespace CodeImp.DoomBuilder.Controls
{
	internal class ResourceListView : ListView
	{
		#region ================== Constants

		private const string DRAG_TYPE = "ReorderItems";

		#endregion

		#region ================== Variables

		// List of items
		private readonly List<ListViewItem> dragitems;

		#endregion

		#region ================== Properties

		// Disable sorting
		public new SortOrder Sorting { get { return SortOrder.None; } set { base.Sorting = SortOrder.None; } }

		#endregion

		#region ================== Constructor

		// Constructor
		public ResourceListView(): base()
		{
			// List for dragged items
			dragitems = new List<ListViewItem>();
		}

		#endregion

		#region ================== Overrides

		// When items are dropped
		protected override void OnDragDrop(DragEventArgs e)
		{
			// Pass on to base
			base.OnDragDrop(e);

			// Leave when no items being dragged
			if(dragitems.Count == 0) return;

			// Determine where to insert
			Point cp = base.PointToClient(new Point(e.X, e.Y));
			ListViewItem insertatitem = base.GetItemAt(cp.X, cp.Y);

			// Leave when nowhere to insert or same as selected item
			if((insertatitem == null) || (dragitems.Contains(insertatitem))) return;
			
			// Leave when item is grayed
			if(insertatitem.ForeColor != SystemColors.WindowText) return;
			
			// Begin updating
			base.BeginUpdate();
			
			// Determine index where to insert
			int dropindex = insertatitem.Index;
			if(dropindex > dragitems[0].Index) dropindex++;

			// Deselect items
			DeselectAll();

			// Insert items
			for(int i = dragitems.Count - 1; i >= 0; i--)
			{
				// Insert a copy of the item here
				base.Items.Insert(dropindex, (ListViewItem)dragitems[i].Clone());
				base.Items[dropindex].Selected = true;
			}

			// Remove old items
			foreach(ListViewItem lvi in dragitems)
			{
				// Remove item from list
				base.Items.Remove(lvi);
			}

			// Done
			base.EndUpdate();
			dragitems.Clear();
		}
		
		// When items are dragged over
		protected override void OnDragOver(DragEventArgs e)
		{
			//mxd. Check if valid extenal data is present
			if(e.Data.GetDataPresent(DataFormats.FileDrop)) 
			{
				e.Effect = DragDropEffects.Copy;
				return;
			}

			// Check if our data format is present
			if(!e.Data.GetDataPresent(DataFormats.Text))
			{
				e.Effect = DragDropEffects.None;
				return;
			}

			// Check if the data matches our data
			String text = (String)e.Data.GetData(DRAG_TYPE.GetType());
			if(String.Compare(text, DRAG_TYPE + this.Name, StringComparison.Ordinal) == 0)
			{
				// Determine where to insert
				Point cp = base.PointToClient(new Point(e.X, e.Y));
				ListViewItem insertatitem = base.GetItemAt(cp.X, cp.Y);
				if(insertatitem == null)
				{
					// Cannot insert here
					e.Effect = DragDropEffects.None;
					return;
				}

				// Item is one of the items being dragged?
				if(dragitems.Contains(insertatitem))
				{
					// Show move possibility, but dont do anything
					e.Effect = DragDropEffects.Move;
					insertatitem.EnsureVisible();
					return;
				}

				// Check if item is grayed
				if(insertatitem.ForeColor != SystemColors.WindowText)
				{
					// Cannot insert here
					e.Effect = DragDropEffects.None;
					insertatitem.EnsureVisible();
					return;
				}
				
				// Pass on to base
				base.OnDragOver(e);

				// Can insert here
				e.Effect = DragDropEffects.Move;
				insertatitem.EnsureVisible();

				// Determine index where to insert
				int dropindex = insertatitem.Index;
				if(dropindex > dragitems[0].Index) dropindex++;

				// Begin updating
				base.BeginUpdate();

				// Deselect items
				DeselectAll();

				// Insert items
				for(int i = dragitems.Count - 1; i >= 0; i--)
				{
					// Insert a copy of the item here
					base.Items.Insert(dropindex, (ListViewItem)dragitems[i].Clone());
					base.Items[dropindex].Selected = true;
				}

				// Remove old items
				foreach(ListViewItem lvi in dragitems)
				{
					// Remove item from list
					base.Items.Remove(lvi);
				}

				// Copy selected items to the list
				dragitems.Clear();
				foreach(ListViewItem lvi in base.SelectedItems) dragitems.Add(lvi);
				
				// Done
				base.EndUpdate();
			}
			else
			{
				// Cannot insert here
				e.Effect = DragDropEffects.None;	
			}
		}		

		// When items are first dragged over
		protected override void OnDragEnter(DragEventArgs e)
		{
			// Pass on to base
			base.OnDragEnter(e);

			//mxd. Check if valid extenal data is present
			if(e.Data.GetDataPresent(DataFormats.FileDrop)) 
			{
				e.Effect = DragDropEffects.Copy;
				return;
			}

			// Check if our data format is present
			if(!e.Data.GetDataPresent(DataFormats.Text))
			{
				// No effect
				e.Effect = DragDropEffects.None;
				return;
			}

			// Check if the data matches our data
			String text = (String)e.Data.GetData(DRAG_TYPE.GetType());
			if(text.CompareTo(DRAG_TYPE + base.Name) == 0)
			{
				// We're moving these items
				e.Effect = DragDropEffects.Move;
			}
			else
			{
				// No effect
				e.Effect = DragDropEffects.None;	
			}
		}

		// When items are first dragged
		protected override void OnItemDrag(ItemDragEventArgs e)
		{
			// Pass on to base
			base.OnItemDrag(e);

			// Anything selected?
			if(base.SelectedItems.Count > 0)
			{
				// Go for all selected items
				for(int i = base.SelectedItems.Count - 1; i >= 0; i--)
				{
					// Item grayed? Then abort!
					if(base.SelectedItems[i].ForeColor != SystemColors.WindowText)
						return;
				}

				// Copy selected items to the list
				dragitems.Clear();
				foreach(ListViewItem lvi in base.SelectedItems) dragitems.Add(lvi);

				// Start drag operation
				base.DoDragDrop(DRAG_TYPE + base.Name, DragDropEffects.Move);
			}
		}

		#endregion

		#region ================== Methods

		// This deselects all items
		private void DeselectAll()
		{
			// Go for all selected items
			for(int i = base.SelectedItems.Count - 1; i >= 0; i--)
			{
				// Item grayed? Then abort!
				base.SelectedItems[i].Selected = false;
			}
		}

		#endregion
	}
}
