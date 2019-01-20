
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

namespace CodeImp.DoomBuilder.BuilderModes
{
	// NOTE: An ErrorChecker may NEVER modify the map, because it runs multithreaded.
	// Do not even change element properties such as 'marked' and 'selected'!
	public class ErrorChecker : IComparable<ErrorChecker>
	{
		#region ================== Variables
		
		private int lastprogress;
		private int totalprogress = -1;
		protected ErrorCheckerAttribute attribs;
		
		#endregion
		
		#region ================== Properties
		
		public int TotalProgress { get { return totalprogress; } }
		public virtual bool SkipCheck { get { return false; } } //mxd

		#endregion
		
		#region ================== Constructor / Destructor
		
		// Constructor
		// Override this to determine and set the total progress
		public ErrorChecker()
		{
			// Initialize
			object[] attrs = this.GetType().GetCustomAttributes(typeof(ErrorCheckerAttribute), true);
			attribs = (ErrorCheckerAttribute)attrs[0];
		}
		
		#endregion
		
		#region ================== Methods
		
		// Override this to run your check
		// Use a Sleep and Try/Catch to handle thread interruption
		public virtual void Run()
		{
			if(totalprogress < 0) throw new InvalidOperationException("You must set the total progress through the SetTotalProgress method before this check can be performed!");
			lastprogress = 0;
		}
		
		// This submits a result to show in the results list
		protected void SubmitResult(ErrorResult result)
		{
			BuilderPlug.Me.ErrorCheckForm.SubmitResult(result);
		}
		
		// This reports a change in progress
		protected void AddProgress(int amount)
		{
			// Make changes
			if(amount < 0) amount = 0;
			if((lastprogress + amount) > totalprogress) throw new InvalidOperationException("Cannot exceed total progress amount!");
			lastprogress += amount;
			BuilderPlug.Me.ErrorCheckForm.AddProgressValue(amount);
		}
		
		// This sets the total progress
		protected void SetTotalProgress(int totalprogress)
		{
			if(totalprogress < 0) throw new ArgumentOutOfRangeException("Total progress cannot be less than zero!");
			this.totalprogress = totalprogress;
		}
		
		// This is for sorting by cost
		public int CompareTo(ErrorChecker other)
		{
			return (other.attribs.Cost - this.attribs.Cost);
		}
		
		#endregion
	}
}
