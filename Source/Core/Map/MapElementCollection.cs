
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
using System.Collections;
using System.Collections.Generic;

#endregion

namespace CodeImp.DoomBuilder.Map
{
	public sealed class MapElementCollection<T> : ICollection<T> where T: MapElement
	{
		// Members
		private int numitems;
		private T[] array;
		
		// Constructor
		public MapElementCollection(ref T[] array, int numitems)
		{
			this.numitems = numitems;
			this.array = array;
		}
		
		public void Add(T item)
		{
			throw new NotSupportedException();
		}
		
		public void Clear()
		{
			throw new NotSupportedException();
		}
		
		public bool Contains(T item)
		{
			throw new NotSupportedException();
		}
		
		public void CopyTo(T[] array, int arrayIndex)
		{
			this.array.CopyTo(array, arrayIndex);
		}
		
		public int Count { get { return numitems; } }
		
		public bool IsReadOnly { get { return true; } }
		
		public bool Remove(T item)
		{
			throw new NotSupportedException();
		}
		
		public IEnumerator<T> GetEnumerator()
		{
			throw new NotSupportedException("This array is locked for modification and cannot currently be enumerated.");
		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotSupportedException("This array is locked for modification and cannot currently be enumerated.");
		}
	}
}
