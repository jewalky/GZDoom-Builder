#region ================== Namespaces

using System;
using System.Drawing;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Windows;

#endregion

namespace CodeImp.DoomBuilder.Types
{
	[TypeHandler(UniversalType.AngleByte, "Byte Angle", true)]
	internal class AngleByteHandler : AngleDegreesHandler
	{
		#region ================== Properties

		public override Image BrowseImage { get { return angleicons[General.ClampAngle((int)Math.Round((float)value / 256 * 360) + 22) / 45]; } }

		#endregion

		#region ================== Methods

		public override void Browse(IWin32Window parent) 
		{
			value = (int)Math.Round((float)AngleForm.ShowDialog(parent, (int)Math.Round((float)value / 256 * 360)) / 360 * 256);
		}

		#endregion
	}
}
