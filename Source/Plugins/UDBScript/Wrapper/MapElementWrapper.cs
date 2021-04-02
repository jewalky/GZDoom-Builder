#region ================== Copyright (c) 2021 Boris Iwanski

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

#region ================== Namespaces

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Types;

#endregion

namespace CodeImp.DoomBuilder.UDBScript.Wrapper
{
	internal abstract class MapElementWrapper
	{
		#region ================== Variables

		private MapElement element;

		#endregion

		#region ================== Properties

		/// <summary>
		/// UDMF fields. It's an object with the fields as properties.
		/// ```
		/// s.fields.comment = 'This is a comment';
		/// s.fields['comment'] = 'This is a comment'; // Also  works
		/// s.fields.xscalefloor = 2.0;
		/// t.score = 100;
		/// ```
		/// It is also possible to define new fields:
		/// ```
		/// s.fields.user_myboolfield = true;
		/// ```
		/// There are some restrictions, though:
		/// - it only works for fields that are not in the base UDMF standard, since those are handled directly in the respective class
		/// - it does not work for flags. While they are technically also UDMF fields, they are handled in the `flags` field of the respective class (where applicable)
		/// - JavaScript does not distinguish between integer and floating point numbers, it only has floating point numbers (of double precision). For fields where UDB knows that they are integers this it not a problem, since it'll automatically convert the floating point numbers to integers (dropping the fractional part). However, if you need to specify an integer value for an unknown or custom field you have to work around this limitation, using the `UniValue` class:
		/// ```
		/// s.fields.user_myintfield = new UniValue(0, 25); // Sets the 'user_myintfield' field to an integer value of 25
		/// ```
		/// </summary>
		public ExpandoObject fields
		{
			get
			{
				if (element.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException(element.GetType() + " is disposed, the fields property can not be accessed.");

				dynamic eo = new ExpandoObject();
				IDictionary<string, object> o = eo as IDictionary<string, object>;

				foreach (KeyValuePair<string, UniValue> f in element.Fields)
					o.Add(f.Key, f.Value.Value);

				// Create event that gets called when a property is changed. This sets the flag
				((INotifyPropertyChanged)eo).PropertyChanged += new PropertyChangedEventHandler((sender, ea) =>	{
					PropertyChangedEventArgs pcea = ea as PropertyChangedEventArgs;
					IDictionary<string, object> so = sender as IDictionary<string, object>;

					string pname = pcea.PropertyName;
					object newvalue = null;

					// If this property was changed, but doesn't exist, then it was deleted and we should not do anything
					if (!so.ContainsKey(pname))
						return;

					if (pname != pname.ToLowerInvariant())
						throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("UDMF field names must be lowercase");

					// Make sure the given field is not a flag (at least for now)
					if((element is Linedef && General.Map.Config.LinedefFlags.Keys.Contains(pname)) ||
						(element is Sidedef && General.Map.Config.SidedefFlags.Keys.Contains(pname)) ||
						(element is Sector && General.Map.Config.SectorFlags.Keys.Contains(pname)) ||
						(element is Thing && General.Map.Config.ThingFlags.Keys.Contains(pname))
					)
					{
						throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("You are trying to modify a flag through the UDMF fields. Please use the 'flags' property instead.");
					}

					if (element.Fields.ContainsKey(pname)) // Field already exists
					{
						if (so[pname] != null)
						{
							object oldvalue = element.Fields[pname].Value;

							if (so[pname] is double && ((oldvalue is int) || (oldvalue is double)))
							{
								if (oldvalue is int)
									newvalue = Convert.ToInt32((double)so[pname]);
								else if (oldvalue is double)
									newvalue = (double)so[pname];
							}
							else if (so[pname] is string && oldvalue is string)
							{
								newvalue = (string)so[pname];
							}
							else if (so[pname] is bool && oldvalue is bool)
							{
								newvalue = (bool)so[pname];
							}
							else
								throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("UDMF field '" + pname + "' is of incompatible type for value " + so[pname]);
						}
					}
					else // Property name doesn't exist yet
					{
						List<UniversalFieldInfo> ufis = null;

						// Get known UDMF fields for the element type
						if (element is Sector)
							ufis = General.Map.Config.SectorFields;
						else if (element is Thing)
							ufis = General.Map.Config.ThingFields;
						else if (element is Linedef)
							ufis = General.Map.Config.LinedefFields;
						else if (element is Sidedef)
							ufis = General.Map.Config.SidedefFields;
						else if (element is Vertex)
							ufis = General.Map.Config.VertexFields;
						else
							throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("element is of unsupported type");

						// Check if there's a known field with the name
						UniversalFieldInfo ufi = ufis.Where(e => e.Name == pname).FirstOrDefault();

						if (ufi == null) // Not a known UniversalField, so no further checks needed
						{
							if (so[pname] is UniValue) // Special handling when a UniValue is given. This is important when the user wants to supply an int, since they don't exist in JS
								newvalue = GetConvertedUniValue((UniValue)so[pname]);
							else
								newvalue = so[pname];
						}
						else
						{
							if (so[pname] is double && ufi.Default is double)
								newvalue = (double)so[pname];
							else if (so[pname] is double && ufi.Default is int)
								newvalue = Convert.ToInt32((double)so[pname]);
							else if (so[pname] is bool && ufi.Default is bool)
								newvalue = (bool)so[pname];
							else if (so[pname] is string && ufi.Default is string)
								newvalue = (string)so[pname];
							else
								throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("UDMF field '" + pname + "' is of incompatible type for value " + so[pname]);
						}
					}

					element.Fields.BeforeFieldsChange();

					if (newvalue == null) // Remove the field when null was passed
					{
						element.Fields.Remove(pname);
						so.Remove(pname);
					}
					else if (newvalue is double)
						UniFields.SetFloat(element.Fields, pname, (double)newvalue);
					else if (newvalue is int)
						UniFields.SetInteger(element.Fields, pname, (int)newvalue);
					else if (newvalue is string)
						UniFields.SetString(element.Fields, pname, (string)newvalue, string.Empty);
					else if (newvalue is bool)
						element.Fields[pname] = new UniValue(UniversalType.Boolean, (bool)newvalue);

					AfterFieldsUpdate();
				});

				return eo;
			}
		}

		#endregion

		#region ================== Constructors

		internal MapElementWrapper(MapElement element)
		{
			this.element = element;
		}

		#endregion

		#region ================== Methods

		internal abstract void AfterFieldsUpdate();

		internal static object GetVectorFromObject(object data, bool allow3d)
		{
			if (data is Vector2D)
				return (Vector2D)data;
			else if (data.GetType().IsArray)
			{
				object[] vals = (object[])data;

				// Make sure all values in the array are doubles
				foreach (object v in vals)
					if (!(v is double))
						throw new CantConvertToVectorException("Values in array must be numbers.");

				if (vals.Length == 2)
					return new Vector2D((double)vals[0], (double)vals[1]);
				if (vals.Length == 3)
					return new Vector3D((double)vals[0], (double)vals[1], (double)vals[2]);
			}
			else if(data is ExpandoObject)
			{
				IDictionary<string, object> eo = data as IDictionary<string, object>;
				double x = double.NaN;
				double y = double.NaN;
				double z = double.NaN;

				if(eo.ContainsKey("x"))
				{
					try
					{
						x = Convert.ToDouble(eo["x"]);
					}
					catch (Exception e)
					{
						throw new CantConvertToVectorException("Can not convert 'x' property of data: " + e.Message);
					}
				}

				if (eo.ContainsKey("y"))
				{
					try
					{
						y = Convert.ToDouble(eo["y"]);
					}
					catch (Exception e)
					{
						throw new CantConvertToVectorException("Can not convert 'y' property of data: " + e.Message);
					}
				}

				if (eo.ContainsKey("z"))
				{
					try
					{
						z = Convert.ToDouble(eo["z"]);
					}
					catch (Exception e)
					{
						throw new CantConvertToVectorException("Can not convert 'z' property of data: " + e.Message);
					}
				}

				if (allow3d)
				{
					if (x != double.NaN && y != double.NaN && z == double.NaN)
						return new Vector2D(x, y);
					else if (x != double.NaN && y != double.NaN && z != double.NaN)
						return new Vector3D(x, y, z);
				}
				else
				{
					if (x != double.NaN && y != double.NaN)
						return new Vector2D(x, y);
				}
			}

			if (allow3d)
				throw new CantConvertToVectorException("Data must be a Vector2D, Vector3D, or an array of numbers.");
			else
				throw new CantConvertToVectorException("Data must be a Vector2D, or an array of numbers.");
		}

		internal object GetConvertedUniValue(UniValue uv)
		{
			switch ((UniversalType)uv.Type)
			{
				case UniversalType.AngleRadians:
				case UniversalType.AngleDegreesFloat:
				case UniversalType.Float:
					return Convert.ToDouble(uv.Value);
				case UniversalType.AngleDegrees:
				case UniversalType.AngleByte: //mxd
				case UniversalType.Color:
				case UniversalType.EnumBits:
				case UniversalType.EnumOption:
				case UniversalType.Integer:
				case UniversalType.LinedefTag:
				case UniversalType.LinedefType:
				case UniversalType.SectorEffect:
				case UniversalType.SectorTag:
				case UniversalType.ThingTag:
				case UniversalType.ThingType:
					return Convert.ToInt32(uv.Value);
				case UniversalType.Boolean:
					return Convert.ToBoolean(uv.Value);
				case UniversalType.Flat:
				case UniversalType.String:
				case UniversalType.Texture:
				case UniversalType.EnumStrings:
				case UniversalType.ThingClass:
					return Convert.ToString(uv.Value);
			}

			return null;
		}

		internal Type GetTypeFromUniversalType(int type)
		{
			switch ((UniversalType)type)
			{
				case UniversalType.AngleRadians:
				case UniversalType.AngleDegreesFloat:
				case UniversalType.Float:
					return typeof(double);
				case UniversalType.AngleDegrees:
				case UniversalType.AngleByte: //mxd
				case UniversalType.Color:
				case UniversalType.EnumBits:
				case UniversalType.EnumOption:
				case UniversalType.Integer:
				case UniversalType.LinedefTag:
				case UniversalType.LinedefType:
				case UniversalType.SectorEffect:
				case UniversalType.SectorTag:
				case UniversalType.ThingTag:
				case UniversalType.ThingType:
					return typeof(int);
				case UniversalType.Boolean:
					return typeof(bool);
				case UniversalType.Flat:
				case UniversalType.String:
				case UniversalType.Texture:
				case UniversalType.EnumStrings:
				case UniversalType.ThingClass:
					return typeof(string);
			}

			return null;
		}

		#endregion
	}
}
