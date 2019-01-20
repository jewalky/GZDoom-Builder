
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
using CodeImp.DoomBuilder.IO;
using CodeImp.DoomBuilder.Types;

#endregion

namespace CodeImp.DoomBuilder.Config
{
	public class UniversalFieldInfo : IComparable<UniversalFieldInfo>
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		// Properties
		private string name;
		private int type;
		private object defaultvalue;
		private EnumList enumlist;

		#endregion

		#region ================== Properties

		public string Name { get { return name; } }
		public int Type { get { return type; } }
		public object Default { get { return defaultvalue; } }
		public EnumList Enum { get { return enumlist; } }

		#endregion

		#region ================== Constructor / Disposer

		// Constructor
		internal UniversalFieldInfo(string path, string name, string configname, Configuration cfg, IDictionary<string, EnumList> enums)
		{
			string setting = "universalfields." + path + "." + name;

			// Initialize
			this.name = name.ToLowerInvariant();

			// Read type
			this.type = cfg.ReadSetting(setting + ".type", int.MinValue);
			this.defaultvalue = cfg.ReadSettingObject(setting + ".default", null);

			//mxd. Check type
			if(this.type == int.MinValue)
			{
				General.ErrorLogger.Add(ErrorType.Warning, "No type is defined for universal field \"" + name + "\" defined in \"" + configname + "\". Integer type will be used.");
				this.type = (int)UniversalType.Integer;
			}

			TypeHandler th = General.Types.GetFieldHandler(this);
			if(th is NullHandler)
			{
				General.ErrorLogger.Add(ErrorType.Warning, "Universal field \"" + name + "\" defined in \"" + configname + "\" has unknown type " + this.type + ". String type will be used instead.");
				this.type = (int)UniversalType.String;
				if(this.defaultvalue == null) this.defaultvalue = "";
			}

			//mxd. Default value is missing? Get it from typehandler
			if(this.defaultvalue == null) this.defaultvalue = th.GetDefaultValue();
			
			// Read enum
			object enumsetting = cfg.ReadSettingObject(setting + ".enum", null);
			if(enumsetting != null)
			{
				// Reference to existing enums list?
				if(enumsetting is string)
				{
					// Link to it
					enumlist = enums[enumsetting.ToString()];
				}
				else if(enumsetting is IDictionary)
				{
					// Make list
					enumlist = new EnumList(enumsetting as IDictionary);
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

		// This compares against another field
		public int CompareTo(UniversalFieldInfo other)
		{
			return string.Compare(this.name, other.name);
		}

		#endregion
	}
}
