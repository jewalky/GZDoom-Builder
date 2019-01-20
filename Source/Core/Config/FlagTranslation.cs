
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

namespace CodeImp.DoomBuilder.Config
{
	public class FlagTranslation : IComparable<FlagTranslation>
	{
		#region ================== Variables

		private int flag;
		private List<string> fields;
		private List<bool> values;

		#endregion

		#region ================== Properties

		public int Flag { get { return flag; } }
		public List<string> Fields { get { return fields; } }
		public List<bool> FieldValues { get { return values; } }

		#endregion

		#region ================== Constructor

		// Constructor
		public FlagTranslation(DictionaryEntry de)
		{
			// Initialize
			this.fields = new List<string>();
			this.values = new List<bool>();
			
			// Set the flag
			if(!int.TryParse(de.Key.ToString(), out flag))
				General.ErrorLogger.Add(ErrorType.Warning, "Invalid flag translation key in configuration. The key must be numeric.");

			// Set the fields
			string[] fieldstrings = de.Value.ToString().Split(',');
			foreach(string f in fieldstrings)
			{
				string ft = f.Trim();
				if(ft.StartsWith("!"))
				{
					fields.Add(ft.Substring(1).Trim());
					values.Add(false);
				}
				else
				{
					fields.Add(ft);
					values.Add(true);
				}
			}
		}

		#endregion

		#region ================== Methods

		// String representation
		public override string ToString()
		{
			return flag.ToString();
		}

		// Comparer (highest first)
		public int CompareTo(FlagTranslation other)
		{
			return other.flag - this.flag;
		}
		
		#endregion
	}
}
