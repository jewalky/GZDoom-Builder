
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
using System.Globalization;

#endregion

namespace CodeImp.DoomBuilder.Config
{
	/// <summary>
	/// Option in generalized types.
	/// </summary>
	public class GeneralizedOption
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		// Properties
		private string name;
		private List<GeneralizedBit> bits;
		private int bitstep; //mxd
		public int BitsStep { get { return bitstep; } } // mxd. Each subsequent value is incremented  by this number
		
		#endregion

		#region ================== Properties

		public string Name { get { return name; } }
		public List<GeneralizedBit> Bits { get { return bits; } }

		#endregion

		#region ================== Constructor / Disposer

		// Constructor
		internal GeneralizedOption(string structure, string cat, string name, IDictionary bitslist)
		{
			string fullpath;
			
			// Determine path
			if(cat.Length > 0) fullpath = structure + "." + cat;
				else fullpath = structure;

			// Initialize
			this.name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name);
			this.bits = new List<GeneralizedBit>();

			// Go for all bits
			foreach(DictionaryEntry de in bitslist)
			{
				// Check if the item key is numeric
				int index;
				if(int.TryParse(de.Key.ToString(), NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, CultureInfo.InvariantCulture, out index))
				{
					// Add to list
					this.bits.Add(new GeneralizedBit(index, de.Value.ToString()));
				}
				else
				{
					General.ErrorLogger.Add(ErrorType.Warning, "Structure \"" + fullpath + "." + name + "\" contains invalid entries. The keys must be numeric.");
				}
			}
			
			// Sort the list
			bits.Sort();

			//mxd. Determine and check increment steps
			if(bits.Count > 1)
			{
				// Use the second bit as the structure's step
				bitstep = bits[1].Index; 
				
				// Check the rest of the values
				for(int i = 1; i < bits.Count; i++)
				{
					if(bits[i].Index - bits[i - 1].Index != bitstep)
						General.ErrorLogger.Add(ErrorType.Warning, "Structure \"" + fullpath + "." + name + "\" contains options with mixed increments (option \"" + bits[i].Title + "\" increment (" + (bits[i - 1].Index - bits[i].Index) + ") doesn't match the structure increment (" + bitstep + ")).");
				}
			}
			
			// We have no destructor
			GC.SuppressFinalize(this);
		}

		#endregion

		#region ================== Methods

		// This presents the item as string
		public override string ToString()
		{
			return name;
		}
		
		#endregion
	}
}
