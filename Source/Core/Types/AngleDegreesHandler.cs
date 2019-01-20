
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
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Windows;

#endregion

namespace CodeImp.DoomBuilder.Types
{
	[TypeHandler(UniversalType.AngleDegrees, "Degrees (Integer)", true)]
	internal class AngleDegreesHandler : TypeHandler
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		protected int value;
		protected Image[] angleicons;

		#endregion

		#region ================== Properties

		public override bool IsBrowseable { get { return true; } }

		public override Image BrowseImage { get { return angleicons[General.ClampAngle(value + 22) / 45]; } }
		public override bool DynamicImage { get { return true; } }

		#endregion

		#region ================== Constructor

		public AngleDegreesHandler()
		{
			angleicons = new[]
			{
				Properties.Resources.Angle,
				Properties.Resources.Angle7,
				Properties.Resources.Angle6,
				Properties.Resources.Angle5,
				Properties.Resources.Angle4,
				Properties.Resources.Angle3,
				Properties.Resources.Angle2,
				Properties.Resources.Angle1
			};
		}

		#endregion

		#region ================== Methods

		public override void Browse(IWin32Window parent)
		{
			value = AngleForm.ShowDialog(parent, value);
		}

		public override void SetValue(object value)
		{
			// Null?
			if(value == null)
			{
				this.value = 0;
			}
			// Compatible type?
			else if((value is int) || (value is float) || (value is bool))
			{
				// Set directly
				this.value = Convert.ToInt32(value);
			}
			else
			{
				// Try parsing as string
				int result;
				if(int.TryParse(value.ToString(), NumberStyles.Integer, CultureInfo.CurrentCulture, out result))
				{
					this.value = result;
				}
				else
				{
					this.value = 0;
				}
			}
		}

		public override object GetValue()
		{
			return this.value;
		}

		public override int GetIntValue()
		{
			return this.value;
		}

		public override string GetStringValue()
		{
			return this.value.ToString();
		}

		public override object GetDefaultValue()
		{
			return 0;
		}

		#endregion
	}
}
