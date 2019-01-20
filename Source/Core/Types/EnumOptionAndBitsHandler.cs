#region ================== Namespaces

using System.Windows.Forms;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Windows;

#endregion

namespace CodeImp.DoomBuilder.Types
{
	[TypeHandler(UniversalType.EnumOptionAndBits, "Options and Bits", false)]
	internal class EnumOptionAndBitsHandler : EnumBitsHandler
	{
		#region ================== Variables

		private EnumList flags;

		#endregion

		#region ================== Methods

		// When set up for an argument
		public override void SetupArgument(TypeHandlerAttribute attr, ArgumentInfo arginfo)
		{
			base.SetupArgument(attr, arginfo);

			// Keep flags list reference
			flags = arginfo.Flags;
		}
		
		public override void Browse(IWin32Window parent)
		{
			value = BitFlagsAndOptionsForm.ShowDialog(parent, list, flags, value);
		}

		#endregion
	}
}
