
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
using System.Globalization;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Map;

#endregion

namespace CodeImp.DoomBuilder.Types
{
	[TypeHandler(UniversalType.SectorTag, "Sector Tag", true)]
	internal class SectorTagHandler : IntegerHandler
	{
		#region ================== Variables

		private EnumList list;
		protected EnumItem value;
		private EnumItem defaultvalue;

		#endregion

		#region ================== Properties

		public override bool IsEnumerable { get { return true; } }

		#endregion

		#region ================== Setup

		// When set up for an argument
		public override void SetupArgument(TypeHandlerAttribute attr, ArgumentInfo arginfo) 
		{
			defaultvalue = new EnumItem("0", "0");
			base.SetupArgument(attr, arginfo);

			// Create enum list reference
			list = CreateEnumList();

			// Add default value
			list.Insert(0, defaultvalue);
		}

		// When set up for a universal field
		public override void SetupField(TypeHandlerAttribute attr, UniversalFieldInfo fieldinfo) 
		{
			defaultvalue = (fieldinfo != null ? new EnumItem(fieldinfo.Default.ToString(), fieldinfo.Default.ToString()) : new EnumItem("0", "0")); //mxd
			base.SetupField(attr, fieldinfo);

			// Create enum list reference
			list = CreateEnumList();
		}

		//mxd
		protected virtual EnumList CreateEnumList() 
		{
			// Collect tags
			List<int> tags = new List<int>();
			HashSet<int> tagshash = new HashSet<int>();
			EnumList taglist = new EnumList();

			if(General.Map.Map != null)
			{
				foreach(Sector s in General.Map.Map.Sectors)
				{
					if(s.Tag == 0 || tagshash.IsSupersetOf(s.Tags)) continue;
					tags.AddRange(s.Tags);
					foreach(int i in s.Tags) tagshash.Add(i);
				}

				// Now sort them in descending order
				tags.Sort((a, b) => -1 * a.CompareTo(b));

				// Create enum items
				foreach(int tag in tags)
				{
					if(General.Map.Options.TagLabels.ContainsKey(tag)) // Tag labels
						taglist.Add(new EnumItem(tag.ToString(), tag + ": " + General.Map.Options.TagLabels[tag]));
					else
						taglist.Add(new EnumItem(tag.ToString(), tag.ToString()));
				}
			}

			return taglist;
		}

		#endregion

		#region ================== Methods

		public override void SetValue(object value) 
		{
			this.value = null;

			// Input null?
			if(value == null) 
			{
				this.value = new EnumItem("0", "0");
			} 
			else 
			{
				// Compatible type?
				if((value is int) || (value is float) || (value is bool)) 
				{
					int intvalue = Convert.ToInt32(value);

					// First try to match the value against the enum values
					foreach(EnumItem item in list) 
					{
						// Matching value?
						if(item.GetIntValue() == intvalue) 
						{
							// Set this value
							this.value = item;
						}
					}
				}

				// No match found yet?
				if(this.value == null) 
				{
					// First try to match the value against the enum values
					foreach(EnumItem item in list) 
					{
						// Matching value?
						if(item.Value == value.ToString()) 
						{
							// Set this value
							this.value = item;
						}
					}
				}

				// No match found yet?
				if(this.value == null) 
				{
					// Try to match against the titles
					foreach(EnumItem item in list) 
					{
						// Matching value?
						if(item.Title.ToLowerInvariant() == value.ToString().ToLowerInvariant()) 
						{
							// Set this value
							this.value = item;
						}
					}
				}

				// Still no match found?
				if(this.value == null) 
				{
					// Make a dummy value
					this.value = new EnumItem(value.ToString(), value.ToString());
					this.value = new EnumItem(this.value.GetIntValue().ToString(CultureInfo.InvariantCulture), value.ToString());
				}
			}
		}

		//mxd
		public override void ApplyDefaultValue() 
		{
			value = defaultvalue;
		}

		public override object GetValue() 
		{
			return GetIntValue();
		}

		public override int GetIntValue() 
		{
			if(this.value != null) 
			{
				// Parse the value to integer
				int result;
				return (int.TryParse(this.value.Value, NumberStyles.Integer, 
					CultureInfo.InvariantCulture, out result) ? result : 0);
			} 
			return 0;
		}

		public override string GetStringValue() 
		{
			return (this.value != null ? this.value.Title : "0: No Tag");
		}

		public override object GetDefaultValue()
		{
			return defaultvalue;
		}

		// This returns an enum list
		public override EnumList GetEnumList() 
		{
			return list;
		}

		// This returns the type to display for fixed fields
		// Must be a custom usable type
		public override TypeHandlerAttribute GetDisplayType() 
		{
			return General.Types.GetAttribute((int)UniversalType.Integer);
		}

		#endregion
	}
}
