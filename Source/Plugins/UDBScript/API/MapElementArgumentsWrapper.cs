#region ================== Copyright (c) 2021 Boris Iwanski

/*
 * This program is free software: you can redistribute it and/or modify
 *
 * it under the terms of the GNU General Public License as published by
 * 
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 * 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
 * 
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.If not, see<http://www.gnu.org/licenses/>.
 */

#endregion

#region ================== Namespaces

using System.Collections;
using System.Collections.Generic;
using CodeImp.DoomBuilder.Map;

#endregion

namespace CodeImp.DoomBuilder.UDBScript.Wrapper
{
	public sealed class MapElementArgumentsWrapper : IEnumerable<int>
	{
		#region ================== Variables

		private MapElement element;

		#endregion

		#region ================== Properties

		public int this[int i]
		{
			get
			{
				if (element is Thing) return ((Thing)element).Args[i];
				else if (element is Linedef) return ((Linedef)element).Args[i];
				else return 0;
			}
			set
			{
				if (element is Thing) ((Thing)element).Args[i] = value;
				else if (element is Linedef) ((Linedef)element).Args[i] = value;
			}
		}

		public int length
		{
			get
			{
				if (element is Thing) return ((Thing)element).Args.Length;
				else if (element is Linedef) return ((Linedef)element).Args.Length;
				else return 0;
			}
		}

		#endregion

		#region ================== Constructors

		public MapElementArgumentsWrapper(MapElement element)
		{
			this.element = element;
		}

		#endregion

		#region ================== Methods

		public IEnumerator<int> GetEnumerator()
		{
			if(element is Thing)
			{
				foreach (int i in ((Thing)element).Args)
					yield return ((Thing)element).Args[i];
			}
			else if (element is Linedef)
			{
				foreach (int i in ((Linedef)element).Args)
					yield return ((Linedef)element).Args[i];
			}

			yield return 0;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}
