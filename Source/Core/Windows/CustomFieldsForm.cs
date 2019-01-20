
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
using System.Windows.Forms;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Map;

#endregion

namespace CodeImp.DoomBuilder.Windows
{
	/// <summary>
	/// Dialog window that allows you to view and/or change custom UDMF fields.
	/// </summary>
	public partial class CustomFieldsForm : DelayedForm
	{
		// Keep a list of elements
		private ICollection<MapElement> elements;
		
		// Constructor
		public CustomFieldsForm()
		{
			// Initialize
			InitializeComponent();
		}

		// This shows the dialog, returns false when cancelled
		public static bool ShowDialog(IWin32Window owner, string title, string elementname, ICollection<MapElement> elements, List<UniversalFieldInfo> fixedfields)
		{
			CustomFieldsForm f = new CustomFieldsForm();
			f.Setup(title, elementname, elements, fixedfields);
			bool result = (f.ShowDialog(owner) == DialogResult.OK);
			f.Dispose();
			return result;
		}
		
		// This sets up the dialog
		public void Setup(string title, string elementname, ICollection<MapElement> elements, List<UniversalFieldInfo> fixedfields)
		{
			// Initialize
			this.elements = elements;
			this.Text = title;

			// Initialize custom fields editor
			fieldslist.Setup(elementname);

			// Fill universal fields list
			fieldslist.ListFixedFields(fixedfields);

			// Setup from first element
			MapElement fe = General.GetByIndex(elements, 0);
			fieldslist.SetValues(fe.Fields, true);
			
			// Setup from all elements
			foreach(MapElement e in elements)
				fieldslist.SetValues(e.Fields, false);
		}
		
		// OK clicked
		private void apply_Click(object sender, EventArgs e)
		{
			// Apply fields to all elements
			foreach(MapElement el in elements) fieldslist.Apply(el.Fields);
			
			// Done
			General.Map.IsChanged = true;
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		// Cancel clicked
		private void cancel_Click(object sender, EventArgs e)
		{
			// Be gone
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		// Help requested
		private void CustomFieldsForm_HelpRequested(object sender, HelpEventArgs hlpevent)
		{
			General.ShowHelp("w_customfields.html");
			hlpevent.Handled = true;
		}
	}
}
