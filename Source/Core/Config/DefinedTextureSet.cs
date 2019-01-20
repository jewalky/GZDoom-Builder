
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
using CodeImp.DoomBuilder.IO;
using System.Collections.Specialized;

#endregion

namespace CodeImp.DoomBuilder.Config
{
	internal sealed class DefinedTextureSet : TextureSet
	{
		#region ================== Variables
		
		#endregion
		
		#region ================== Constructor / Destructor
		
		// Texture set from configuration
		public DefinedTextureSet(Configuration cfg, string path)
		{
			// Read the name
			name = cfg.ReadSetting(path + ".name", "Unnamed Set");
			
			// Read the filters
			IDictionary dic = cfg.ReadSetting(path, new Hashtable());
			filters = new List<string>(dic.Count);
			foreach(DictionaryEntry de in dic)
			{
				// If not the name of this texture set, add value as filter
				if(de.Key.ToString() != "name") filters.Add(de.Value.ToString().ToUpperInvariant());
			}
		}
		
		// New texture set constructor
		public DefinedTextureSet(string name)
		{
			this.name = name;
			this.filters = new List<string>();
		}
		
		#endregion
		
		#region ================== Methods
		
		// This writes the texture set to configuration
		internal void WriteToConfig(Configuration cfg, string path)
		{
			// Fill structure
			IDictionary dic = new ListDictionary();
			
			// Add name
			dic.Add("name", name);
			
			for(int i = 0; i < filters.Count; i++)
			{
				// Add filters
				dic.Add("filter" + i, filters[i].ToUpperInvariant());
			}
			
			// Write to config
			cfg.WriteSetting(path, dic);
		}
		
		// Duplication
		internal DefinedTextureSet Copy()
		{
			// Make a copy
			DefinedTextureSet s = new DefinedTextureSet(this.name);
			s.filters = new List<string>(this.filters);
			return s;
		}
		
		// This applies the filters and name of one set to this one
		internal void Apply(TextureSet set)
		{
			this.name = set.Name;
			this.filters = new List<string>(set.Filters);
		}
		
		#endregion
	}
}
