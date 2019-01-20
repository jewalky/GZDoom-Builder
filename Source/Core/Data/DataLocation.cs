
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
using System.IO;

#endregion

namespace CodeImp.DoomBuilder.Data
{
	public struct DataLocation : IComparable<DataLocation>, IComparable, IEquatable<DataLocation>
	{
		// Constants
		public const int RESOURCE_WAD = 0;
		public const int RESOURCE_DIRECTORY = 1;
		public const int RESOURCE_PK3 = 2;
		
		// Members
		public int type;
		public string location;
		private string initiallocation; //mxd. Stores intial path inside a PK3/PK7. For display purposes only!
		private string name; //mxd
		public bool option1;
		public bool option2;
		public bool notfortesting;
		
		// Constructor
		public DataLocation(int type, string location, bool option1, bool option2, bool notfortesting)
		{
			// Initialize
			this.type = type;
			this.location = location;
			this.initiallocation = string.Empty; //mxd
			this.option1 = option1;
			this.option2 = option2;
			this.notfortesting = notfortesting;
			this.name = string.Empty; //mxd
		}

		//mxd. Constructor for WADs inside of PK3s
		internal DataLocation(int type, string location, string initiallocation, bool option1, bool option2, bool notfortesting)
		{
			// Initialize
			this.type = type;
			this.location = location;
			this.initiallocation = initiallocation;
			this.option1 = option1;
			this.option2 = option2;
			this.notfortesting = notfortesting;
			this.name = string.Empty; 
		}

		// This displays the struct as string
		public override string ToString()
		{
			// Simply show location
			return location;
		}

		//mxd. This returns short location name. May not correspond to actual file location! Use for display purposes only!
		public string GetDisplayName()
		{
			if(string.IsNullOrEmpty(name))
			{
				// Make shorter name for display purposes
				switch(type)
				{
					case RESOURCE_DIRECTORY:
						name = location.Substring(location.LastIndexOf(Path.DirectorySeparatorChar) + 1);
						break;

					case RESOURCE_WAD:
						name = (!string.IsNullOrEmpty(initiallocation) ? initiallocation : Path.GetFileName(location));
						break;

					case RESOURCE_PK3:
						name = Path.GetFileName(location);
						break;

					default:
						throw new NotImplementedException("Unknown location type: " + type);
				}
			}

			return (name ?? string.Empty);
		}

		// This compares two locations
		public int CompareTo(DataLocation other)
		{
			return string.Compare(this.location, other.location, true);
		}
		
		// This compares two locations
		public int CompareTo(object obj)
		{
			return string.Compare(this.location, ((DataLocation)obj).location, true);
		}
		
		// This compares two locations
		public bool Equals(DataLocation other)
		{
			return (this.CompareTo(other) == 0);
		}

		//mxd
		public bool IsValid()
		{
			switch(type) 
			{
				case RESOURCE_DIRECTORY:
					if(!Directory.Exists(location)) return false;
					break;

				case RESOURCE_WAD:
				case RESOURCE_PK3:
					if(!File.Exists(location)) return false;
					break;

				default:
					throw new NotImplementedException("Unknown location type: " + type);
			}

			return true;
		}
	}
}
