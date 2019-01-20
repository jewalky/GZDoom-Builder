
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

#endregion

namespace CodeImp.DoomBuilder.Types
{
	public sealed class TypeHandlerAttribute : Attribute
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		private readonly int index;
		private readonly string name;
		private Type type;
		private readonly bool customusable;
		
		#endregion

		#region ================== Properties

		public int Index { get { return index; } }
		public string Name { get { return name; } }
		public bool IsCustomUsable { get { return customusable; } }
		public Type Type { get { return type; } set { type = value; } }
		
		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public TypeHandlerAttribute(UniversalType index, string name, bool customusable)
		{
			// Initialize
			this.index = (int)index;
			this.name = name;
			this.customusable = customusable;
		}

		#endregion

		#region ================== Methods

		// String representation
		public override string ToString()
		{
			return name;
		}
		
		#endregion
	}
}
