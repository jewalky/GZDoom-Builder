
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

using System.Collections.Generic;

#endregion

namespace CodeImp.DoomBuilder
{
	public class ErrorLogger
	{
		#region ================== Constants

		#endregion

		#region ================== Variables
		
		private readonly List<ErrorItem> errors;
		private volatile bool changed;
		private volatile bool erroradded;
		private volatile bool warningadded;
		private object threadlock = new object(); //mxd
		
		#endregion

		#region ================== Properties
		
		public bool HasErrors { get { return (errors.Count > 0); } }
		public int ErrorsCount { get { return errors.Count; } } //mxd
		public bool HasChanged { get { return changed; } set { changed = value; } }
		public bool IsErrorAdded { get { return erroradded; } set { erroradded = value; } }
		public bool IsWarningAdded { get { return warningadded; } set { warningadded = value; } }
		
		#endregion

		#region ================== Constructor / Disposer
		
		// Constructor
		internal ErrorLogger()
		{
			errors = new List<ErrorItem>();
		}
		
		#endregion
		
		#region ================== Methods

		// This clears the errors
		public void Clear()
		{
			lock(threadlock)
			{
				changed = false;
				erroradded = false;
				warningadded = false;
				errors.Clear();

				//mxd
				General.MainWindow.SetWarningsCount(0, false);
			}
		}
		
		// This adds a new error
		public void Add(ErrorType type, string message) { Add(new ErrorItem(type, message)); }
		public void Add(ErrorItem error) //mxd
		{
			lock(threadlock)
			{
				//mxd. Don't add duplicate messages
				if(errors.Count == 0 || error.Description != errors[errors.Count - 1].Description || error.Type != errors[errors.Count - 1].Type)
				{
					errors.Add(error);
					string prefix = "";
					switch(error.Type)
					{
						case ErrorType.Error:
							erroradded = true;
							prefix = "ERROR: ";
#if DEBUG
							DebugConsole.WriteLine(DebugMessageType.ERROR, error.Description);
#endif
							break;

						case ErrorType.Warning:
							warningadded = true;
							prefix = "WARNING: ";
#if DEBUG
							DebugConsole.WriteLine(DebugMessageType.WARNING, error.Description);
#endif
							break;
					}

					changed = true;
					General.WriteLogLine(prefix + error.Description);
					General.MainWindow.SetWarningsCount(errors.Count, erroradded); //mxd
				}
				// But still blink the indicator on errors
				else if(error.Type == ErrorType.Error)
				{
					General.MainWindow.SetWarningsCount(errors.Count, true);
				}
			}
		}
		
		// This returns the list of errors
		/*internal List<ErrorItem> GetErrors()
		{
			lock(this)
			{
				List<ErrorItem> copylist = new List<ErrorItem>(errors);
				return copylist;
			}
		}*/

		//mxd. This returns the list of errors starting at given index
		internal IEnumerable<ErrorItem> GetErrors(int startindex)
		{
			if(startindex >= errors.Count) return new List<ErrorItem>();

			ErrorItem[] result = new ErrorItem[errors.Count - startindex];
			errors.CopyTo(startindex, result, 0, result.Length);

			return result;
		}
		
		#endregion
	}
}
