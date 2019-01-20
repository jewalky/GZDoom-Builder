
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

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using CodeImp.DoomBuilder.IO;

#endregion

namespace CodeImp.DoomBuilder.Data
{
	public sealed class DataLocationList : List<DataLocation>
	{
		#region ================== Constructors

		// This creates a new list
		public DataLocationList()
		{
		}

		// This makes a copy of a list
		public DataLocationList(IEnumerable<DataLocation> list) : base(list)
		{
		}

		// This creates a list from a configuration structure
		internal DataLocationList(Configuration cfg, string path)
		{
			// Go for all items in the map info
			IDictionary resinfo = cfg.ReadSetting(path, new ListDictionary());
			foreach(DictionaryEntry rl in resinfo)
			{
				// Item is a structure?
				IDictionary rlinfo = rl.Value as IDictionary;
				if(rlinfo != null)
				{
					// Create resource location
					DataLocation res = new DataLocation();

					// Copy information from Configuration to ResourceLocation
					if(rlinfo.Contains("type") && (rlinfo["type"] is int)) res.type = (int)rlinfo["type"];
					if(rlinfo.Contains("location") && (rlinfo["location"] is string)) res.location = (string)rlinfo["location"];
					if(rlinfo.Contains("option1") && (rlinfo["option1"] is int)) res.option1 = General.Int2Bool((int)rlinfo["option1"]);
					if(rlinfo.Contains("option2") && (rlinfo["option2"] is int)) res.option2 = General.Int2Bool((int)rlinfo["option2"]);
					if(rlinfo.Contains("notfortesting") && (rlinfo["notfortesting"] is int)) res.notfortesting = General.Int2Bool((int)rlinfo["notfortesting"]);

					// Add resource
					Add(res);
				}
			}
		}
		
		#endregion

		#region ================== Methods

		// This merges two lists together
		public static DataLocationList Combined(DataLocationList a, DataLocationList b)
		{
			DataLocationList result = new DataLocationList(a);

			//mxd. In case of duplicates, keep the last entry
			foreach(DataLocation dl in b)
			{
				result.Remove(dl);
				result.Add(dl);
			}

			return result;
		}

		// This writes the list to configuration
		internal void WriteToConfig(Configuration cfg, string path)
		{
			// Fill structure
			IDictionary resinfo = new ListDictionary();
			for(int i = 0; i < this.Count; i++)
			{
				// Create structure for resource
				IDictionary rlinfo = new ListDictionary();
				rlinfo.Add("type", this[i].type);
				rlinfo.Add("location", this[i].location);
				rlinfo.Add("option1", General.Bool2Int(this[i].option1));
				rlinfo.Add("option2", General.Bool2Int(this[i].option2));
				rlinfo.Add("notfortesting", General.Bool2Int(this[i].notfortesting));

				// Add structure
				resinfo.Add("resource" + i.ToString(CultureInfo.InvariantCulture), rlinfo);
			}
			
			// Write to config
			cfg.WriteSetting(path, resinfo);
		}

		//mxd
		public bool IsValid()
		{
			foreach(DataLocation location in this) if(!location.IsValid()) return false;
			return true;
		}
		
		#endregion
	}
}
