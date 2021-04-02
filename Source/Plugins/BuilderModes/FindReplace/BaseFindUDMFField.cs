#region ================== Copyright (c) 2021 Boris Iwanski

/*
 * Copyright (c) 2021 Boris Iwanski
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
using System.Text.RegularExpressions;
using CodeImp.DoomBuilder.Map;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	class BaseFindUDMFField : BaseFindMapElement
	{
		#region ================== Properties

		public override string UsageHint
		{
			get
			{
				return "Usage: field [value]" + Environment.NewLine
					+ "Supported wildcards (for both field and value):" + Environment.NewLine
					+ "* - zero or more characters" + Environment.NewLine
					+ "? - one character";
			}
		}

		#endregion

		#region ================== Methods

		public override bool CanReplace()
		{
			return false;
		}

		public override bool DetermineVisiblity()
		{
			return General.Map.UDMF;
		}

		/// <summary>
		/// Gets map elements with matching fields
		/// </summary>
		/// <param name="key">Field name</param>
		/// <param name="value">Field value</param>
		/// <param name="list">List of map elements to check</param>
		/// <returns></returns>
		protected FindReplaceObject[] GetObjects(string input, ICollection<MapElement> list)
		{
			if (string.IsNullOrWhiteSpace(input))
				return new FindReplaceObject[] { };

			List<FindReplaceObject> objs = new List<FindReplaceObject>();

			string key;
			string value;

			input = input.Trim();

			if (input.IndexOf(' ') == -1)
			{
				key = input;
				value = string.Empty;
			}
			else
			{
				key = input.Substring(0, input.IndexOf(' '));
				value = input.Substring(input.IndexOf(' ')).Trim();
			}

			Regex keyre = new Regex(WildCardToRegular(key));
			Regex valuere = new Regex(WildCardToRegular(value));

			foreach(MapElement me in list)
			{
				foreach(KeyValuePair<string, UniValue> kvp in me.Fields)
				{
					if (keyre.IsMatch(kvp.Key))
					{
						bool matching = true;
						string fieldvalue = kvp.Value.Value.ToString();

						if (!string.IsNullOrEmpty(value))
						{
							if (!valuere.IsMatch(fieldvalue))
								matching = false;
						}

						if(matching)
							objs.Add(new FindReplaceObject(me, me.GetType().Name + " " + me.Index.ToString() + ". " + kvp.Key + ": " + fieldvalue));
					}
				}
			}

			return objs.ToArray();
		}

		/// <summary>
		/// Turns a wildcard string into an regular expression. Taken from https://stackoverflow.com/a/30300521 (by user Dmitry Bychenko)
		/// </summary>
		/// <param name="value">String with wildcards</param>
		/// <returns>String of regular expression</returns>
		private static string WildCardToRegular(string value)
		{
			return "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
		}

		#endregion
	}
}
