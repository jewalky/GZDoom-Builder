
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

/*#region ================== Namespaces

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

#endregion

namespace CodeImp.DoomBuilder
{
	internal class BinaryHeap<T> : IEnumerable<T>, ICollection<T> where T : IComparable<T>
	{
		#region ================== Variables

		// This will keep all items
		private List<T> heap;

		#endregion

		#region ================== Properties

		public int Count { get { return heap.Count; } }
		public virtual bool IsReadOnly { get { return false; } }
		public T Root { get { if(heap.Count > 0) return heap[0]; else return default(T); } }
		public T this[int index] { get { return ItemAt(index); } }

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public BinaryHeap()
		{
			// Initialize with default capacity
			heap = new List<T>();
		}

		// Constructor
		public BinaryHeap(int capacity)
		{
			// Initialize with specified capacity
			heap = new List<T>(capacity);
		}

		// Destructor
		~BinaryHeap()
		{
			// Clean up
			heap = null;
		}

		#endregion

		#region ================== Methods
		
		// Methods to find our way through the heap
		private static int ParentOf(int index) { return (index - 1) >> 1; }
		private static int LeftOf(int index) { return (index << 1) + 1; }
		private static int RightOf(int index) { return (index << 1) + 2; }
		
		// This swaps two items in place
		protected virtual void SwapItems(int index1, int index2)
		{
			// Swap items
			T tempitem = heap[index1];
			heap[index1] = heap[index2];
			heap[index2] = tempitem;
		}

		// This adds an item to the list
		// This is an O(log n) operation, where n is Count
		public virtual void Add(T item)
		{
			int index = heap.Count;
			
			// Add to the end of the heap
			heap.Add(item);
			
			// Continue until the item is at the top
			// or compares higher to the parent item
			while((index > 0) && (heap[index].CompareTo(heap[ParentOf(index)]) > 0))
			{
				// Swap with parent item
				SwapItems(index, ParentOf(index));
				index = ParentOf(index);
			}
		}

		// Finds and returns the index of an item
		// This is an O(n) operation, where n is Count
		public virtual int IndexOf(T item)
		{
			return heap.IndexOf(item);
		}
		
		// This returns the item at the given index
		// This is an O(1) operation
		public virtual T ItemAt(int index)
		{
			return heap[index];
		}
		
		// This removes an item from the list
		// This is an O(log n) operation, where n is Count
		public virtual void RemoveAt(int index)
		{
			int newindex = index;
			
			// Replace with last item
			heap[index] = heap[heap.Count - 1];
			heap.RemoveAt(heap.Count - 1);
			
			// Continue while item has at least a left child
			while(LeftOf(index) < heap.Count)
			{
				// Right childs also available?
				if(RightOf(index) < heap.Count)
				{
					// Compare with both childs
					// NOTE: Using newindex as indexer in the second line to ensure the lowest of both is chosen
					if(heap[index].CompareTo(heap[LeftOf(index)]) < 0) newindex = LeftOf(index);
					if(heap[newindex].CompareTo(heap[RightOf(index)]) < 0) newindex = RightOf(index);
				}
				// Only left child available
				else
				{
					// Compare with left child
					if(heap[index].CompareTo(heap[LeftOf(index)]) < 0) newindex = LeftOf(index);
				}

				// Item should move down?
				if(newindex != index)
				{
					// Swap the items
					SwapItems(index, newindex);
					index = newindex;
				}
				else
				{
					// Item is fine where it is, we're done
					break;
				}
			}
		}

		// This removes the root item from the list
		// This is an O(log n) operation, where n is Count
		public virtual void RemoveRoot()
		{
			// Remove the root item
			RemoveAt(0);
		}

		// This removes a specific item from the list
		// This is an O(n) operation, where n is Count
		public virtual bool Remove(T item)
		{
			// Find the item in the heap
			int index = IndexOf(item);
			if(index > -1)
			{
				// Remove the item from the heap
				RemoveAt(index);
				return true;
			}
			else
			{
				// No such item
				return false;
			}
		}

		// This clears the heap
		public virtual void Clear()
		{
			// Clear the heap
			heap.Clear();
		}
		
		// This checks if the heap contains a specific item
		// This is an O(n) operation, where n is Count
		public virtual bool Contains(T item)
		{
			return (IndexOf(item) > -1);
		}

		// This copies all items to an array
		public virtual void CopyTo(T[] array)
		{
			// Copy items
			heap.CopyTo(array);
		}
		
		// This copies all items to an array
		public virtual void CopyTo(T[] array, int arrayindex)
		{
			// Copy items
			heap.CopyTo(array, arrayindex);
		}

		// This copies all items to an array
		public virtual void CopyTo(int index, T[] array, int arrayindex, int count)
		{
			// Copy items
			heap.CopyTo(index, array, arrayindex, count);
		}
		
		// Implemented to display the list
		// This is an O(n) operation, where n is Count
		public override string ToString()
		{
			StringBuilder str = new StringBuilder(heap.Count * 5);

			// Go for all items
			for(int i = 0; i < heap.Count; i++)
			{
				// Append item to string
				if(i > 0) str.Append(", ");
				str.Append(heap[i]);
			}

			// Return the string
			return str.ToString();
		}

		// This returns an enumerator
		public IEnumerator<T> GetEnumerator()
		{
			return heap.GetEnumerator();
		}
		
		// This returns an enumerator
		IEnumerator IEnumerable.GetEnumerator()
		{
			return heap.GetEnumerator();
		}
		
		#endregion
	}
}*/
