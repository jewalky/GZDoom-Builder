
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
using System.Windows.Forms;

#endregion

namespace CodeImp.DoomBuilder.Windows
{
	public class PreferencesController
	{
		#region ================== Delegates

		public delegate void AcceptDelegate(PreferencesController controller);
		public delegate void CancelDelegate(PreferencesController controller);

		public event AcceptDelegate OnAccept;
		public event CancelDelegate OnCancel;

		#endregion

		#region ================== Variables

		private PreferencesForm form;
		private bool allowaddtab;

		#endregion

		#region ================== Properties

		internal bool AllowAddTab { get { return allowaddtab; } set { allowaddtab = value; } }

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		internal PreferencesController(PreferencesForm form)
		{
			// Initialize
			this.form = form;
		}

		// Destructor
		/*~PreferencesController()
		{
			form = null;
			OnAccept = null;
			OnCancel = null;
		}*/

		#endregion

		#region ================== Methods

		// This adds a preferences tab
		public void AddTab(TabPage tab)
		{
			if(!allowaddtab) throw new InvalidOperationException("Tab pages can only be added when the dialog is being initialized");

			form.AddTabPage(tab);
		}
		
		// This raises the OnAccept event
		public void RaiseAccept()
		{
			if(OnAccept != null) OnAccept(this);
		}

		// This raises the OnCancel event
		public void RaiseCancel()
		{
			if(OnCancel != null) OnCancel(this);
		}

		#endregion
	}
}
