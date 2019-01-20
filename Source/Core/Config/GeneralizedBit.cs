using System;

namespace CodeImp.DoomBuilder.Config
{
	/// <summary>
	/// Option value in generalized types.
	/// </summary>
	public class GeneralizedBit : INumberedTitle, IComparable<GeneralizedBit>
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		// Properties
		private int index;
		private string title;

		#endregion

		#region ================== Properties

		public int Index { get { return index; } }
		public string Title { get { return title; } }

		#endregion

		#region ================== Constructor / Disposer

		// Constructor
		internal GeneralizedBit(int index, string title)
		{
			// Initialize
			this.index = index;
			this.title = title;
			
			// We have no destructor
			GC.SuppressFinalize(this);
		}

		#endregion

		#region ================== Methods

		// This presents the item as string
		public override string ToString()
		{
			return title;
		}

		// This compares against another
		public int CompareTo(GeneralizedBit other)
		{
			if(this.index < other.index) return -1;
			else if(this.index > other.index) return 1;
			else return 0;
		}
		
		#endregion
	}
}
