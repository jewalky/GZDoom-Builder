
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

#endregion

namespace CodeImp.DoomBuilder.Types
{
	internal class NullHandler : TypeHandler
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		private object value = 0;

		#endregion

		#region ================== Properties

		#endregion

		#region ================== Methods

		public override void SetValue(object value)
		{
			if(value != null)
				this.value = value;
			else
				this.value = 0;
		}

		public override object GetValue()
		{
			return this.value.ToString();
		}

		public override int GetIntValue()
		{
			int result;
			if(int.TryParse(this.value.ToString(), out result)) return result;
			return 0;
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
