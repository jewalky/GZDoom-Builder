#region ================== Copyright (c) 2020 Boris Iwanski

/*
 * This program is free software: you can redistribute it and/or modify
 *
 * it under the terms of the GNU General Public License as published by
 * 
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 * 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
 * 
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.If not, see<http://www.gnu.org/licenses/>.
 */

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Types;

namespace CodeImp.DoomBuilder.UDBScript
{
	public class ScriptOption
	{
		public string name;
		public string description;
		public int type;
		public object defaultvalue;
		public object value;
		public TypeHandler typehandler;
		private IDictionary enumvalues;

		public static readonly UniversalType[] ValidTypes =
		{
			UniversalType.Integer,
			UniversalType.Float,
			UniversalType.String,
			UniversalType.Boolean,
			UniversalType.LinedefType,
			UniversalType.SectorEffect,
			UniversalType.Texture,
			UniversalType.Flat,
			UniversalType.AngleDegrees,
			UniversalType.AngleRadians,
			UniversalType.Color,
			UniversalType.EnumOption,
			UniversalType.SectorTag,
			UniversalType.ThingTag,
			UniversalType.LinedefTag,
			//UniversalType.EnumStrings,
			UniversalType.AngleDegreesFloat,
			UniversalType.ThingType,
			UniversalType.ThingClass,
			UniversalType.RandomInteger,
			UniversalType.RandomFloat,
			UniversalType.AngleByte,
			UniversalType.ThingRadius,
			UniversalType.ThingHeight,
			UniversalType.PolyobjectNumber
		};

		public ScriptOption(string name, string description, int type, IDictionary enumvalues, object defaultvalue)
		{
			this.name = name;
			this.description = description;
			this.type = type;
			this.defaultvalue = this.value = defaultvalue;
			this.enumvalues = enumvalues;

			typehandler = General.Types.GetFieldHandler(type, defaultvalue);

			FillEnumList();
		}

		/// <summary>
		/// Reloads the type handler. This is necessary so that changed enums (like sector tags) are updated
		/// </summary>
		public void ReloadTypeHandler()
		{
			object tmpvalue = typehandler.GetValue();

			// This only needs to be done if it's an enum
			if (typehandler.IsEnumerable)
			{
				typehandler = General.Types.GetFieldHandler(type, defaultvalue);
				typehandler.SetValue(tmpvalue);
				FillEnumList();
			}
		}

		/// <summary>
		/// Fills the enum list
		/// </summary>
		private void FillEnumList()
		{
			if (enumvalues != null)
			{
				EnumList el = typehandler.GetEnumList();

				foreach (DictionaryEntry de in enumvalues)
				{
					if (de.Value == null)
						el.Add(new EnumItem(de.Key.ToString(), de.Key.ToString()));
					else
						el.Add(new EnumItem(de.Key.ToString(), de.Value.ToString()));
				}
			}
		}
	}
}
