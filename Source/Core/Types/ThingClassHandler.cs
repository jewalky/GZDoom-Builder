
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

using System.Drawing;
using CodeImp.DoomBuilder.Config;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Windows;

#endregion

namespace CodeImp.DoomBuilder.Types
{
	[TypeHandler(UniversalType.ThingClass, "Thing Class", true)]
	internal class ThingClassHandler : TypeHandler
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		private string value = "";

		#endregion

		#region ================== Properties

		public override bool IsBrowseable { get { return true; } }

		public override Image BrowseImage { get { return Properties.Resources.List; } }
		
		#endregion

		#region ================== Methods

		public override void Browse(IWin32Window parent)
		{
			int tid = 0;
			
			// Find the thing with this class name
			foreach(ThingTypeInfo t in General.Map.Data.ThingTypes)
			{
				if((string.Compare(t.ClassName, value, true) == 0)) //mxd
				{
					tid = t.Index;
					break;
				}
			}
			
			ThingBrowserForm f = new ThingBrowserForm(tid);
			if(f.ShowDialog(Form.ActiveForm) == DialogResult.OK) 
			{
				// Find the class name for this thing
				ThingTypeInfo t = General.Map.Data.GetThingInfo(f.SelectedType);
				this.value = !string.IsNullOrEmpty(t.ClassName) ? t.ClassName : ""; //mxd
			}
			
			f.Dispose();
		}

		public override void SetValue(object value)
		{
			if(value != null)
				this.value = value.ToString();
			else
				this.value = "";
		}

		public override object GetValue()
		{
			return this.value;
		}

		public override string GetStringValue()
		{
			return this.value;
		}

		public override object GetDefaultValue()
		{
			return string.Empty;
		}

		#endregion
	}
}
