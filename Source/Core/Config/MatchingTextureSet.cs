
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
using System.Collections.Generic;
using System.Text;
using CodeImp.DoomBuilder.Data;
using System.Text.RegularExpressions;

#endregion

namespace CodeImp.DoomBuilder.Config
{
	public sealed class MatchingTextureSet : TextureSet, IFilledTextureSet, IComparable<MatchingTextureSet>
	{
		#region ================== Variables
		
		// Never stored, only used at run-time
		private Regex regex;
		
		// Matching textures and flats
		private List<ImageData> textures;
		private List<ImageData> flats;
		
		#endregion

		#region ================== Properties

		public ICollection<ImageData> Textures { get { return textures; } }
		public ICollection<ImageData> Flats { get { return flats; } } //mxd

		#endregion
		
		#region ================== Constructor / Destructor
		
		// New texture set for quick matching
		public MatchingTextureSet(ICollection<string> filters)
		{
			this.filters = new List<string>(filters);
			
			// Setup
			Setup();
		}
		
		// Texture set from defined set
		internal MatchingTextureSet(DefinedTextureSet definedset)
		{
			// Copy the name
			this.name = definedset.Name;
			
			// Copy the filters
			this.filters = new List<string>(definedset.Filters);
			
			// Setup
			Setup();
		}
		
		#endregion
		
		#region ================== Methods
		
		// This sets up the object
		private void Setup()
		{
			// Make the regex string that handles all filters
			StringBuilder regexstr = new StringBuilder("");
			foreach(string s in this.filters)
			{
				// Make sure filter is in uppercase
				string ss = s.ToUpperInvariant();

				// Escape regex characters
				ss = ss.Replace("+", "\\+");
				ss = ss.Replace("\\", "\\\\");
				ss = ss.Replace("|", "\\|");
				ss = ss.Replace("{", "\\{");
				ss = ss.Replace("[", "\\[");
				ss = ss.Replace("(", "\\(");
				ss = ss.Replace(")", "\\)");
				ss = ss.Replace("^", "\\^");
				ss = ss.Replace("$", "\\$");
				ss = ss.Replace(".", "\\.");
				ss = ss.Replace("#", "\\#");
				ss = ss.Replace(" ", "\\ ");

				// Replace the ? with the regex code for single character
				ss = ss.Replace("?", ".");

				// Replace the * with the regex code for optional multiple characters
				ss = ss.Replace("*", ".*?");

				// When a filter has already added, insert a conditional OR operator
				if(regexstr.Length > 0) regexstr.Append("|");

				// Open group without backreferencing
				regexstr.Append("(?:");

				// Must be start of string
				regexstr.Append("\\A");

				// Add the filter
				regexstr.Append(ss);

				// Must be end of string
				regexstr.Append("\\Z");

				// Close group
				regexstr.Append(")");
			}

			// No filters added? Then make a never-matching regex
			if(this.filters.Count == 0) regexstr.Append("\\Z\\A");
			
			// Make the regex
			regex = new Regex(regexstr.ToString(), RegexOptions.Compiled |
												   RegexOptions.CultureInvariant);

			// Initialize collections
			textures = new List<ImageData>();
			flats = new List<ImageData>();
		}
		
		// This matches a name against the regex and adds a texture to
		// the list if it matches. Returns true when matched and added.
		internal bool AddTexture(ImageData image)
		{
			// Check against regex
			if(regex.IsMatch(image.Name.ToUpperInvariant()))
			{
				// Matches! Add it.
				textures.Add(image);
				return true;
			}
			else
			{
				// Doesn't match
				return false;
			}
		}

		// This matches a name against the regex and adds a flat to
		// the list if it matches. Returns true when matched and added.
		internal bool AddFlat(ImageData image)
		{
			// Check against regex
			if(regex.IsMatch(image.Name.ToUpperInvariant()))
			{
				// Matches! Add it.
				flats.Add(image);
				return true;
			}
			else
			{
				// Doesn't match
				return false;
			}
		}
		
		// This only checks if the given image is a match
		internal bool IsMatch(ImageData image)
		{
			return regex.IsMatch(image.Name.ToUpperInvariant());
		}

		// This only checks if the given texture name is a match (mxd)
		public bool IsMatch(string texturename) 
		{
			return regex.IsMatch(texturename.ToUpperInvariant());
		}

		// This compares it for sorting
		public int CompareTo(MatchingTextureSet other)
		{
			return string.Compare(this.name, other.name);
		}
		
		#endregion
	}
}
