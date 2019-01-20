#region ================== Namespaces

using System;
using System.Collections.Generic;
using System.Globalization;

#endregion

namespace CodeImp.DoomBuilder.ZDoom
{
	internal class DecorateCategoryInfo
	{
		#region ================== Properties

		public List<string> Category;
		public Dictionary<string, List<string>> Properties;

		#endregion

		#region ================== Constructor

		public DecorateCategoryInfo()
		{
			Category = new List<string>(1);
			Properties = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
		}

		#endregion

		#region ================== Methods

		public string GetPropertyValueString(string propname, int valueindex, string defaultvalue) { return GetPropertyValueString(propname, valueindex, defaultvalue, true); }
		public string GetPropertyValueString(string propname, int valueindex, string defaultvalue, bool stripquotes)
		{
			if(Properties.ContainsKey(propname) && (Properties[propname].Count > valueindex))
				return (stripquotes ? ZDTextParser.StripQuotes(Properties[propname][valueindex]) : Properties[propname][valueindex]);
			return defaultvalue;
		}

		public bool GetPropertyValueBool(string propname, int valueindex, bool defaultvalue)
		{
			string str = GetPropertyValueString(propname, valueindex, string.Empty, false).ToLowerInvariant();
			return (string.IsNullOrEmpty(str) ? defaultvalue : str == "true");
		}

		public int GetPropertyValueInt(string propname, int valueindex, int defaultvalue)
		{
			string str = GetPropertyValueString(propname, valueindex, string.Empty, false);

			// It can be negative...
			if(str == "-" && Properties.Count > valueindex + 1)
				str += GetPropertyValueString(propname, valueindex + 1, String.Empty, false);

			int intvalue;
			return (int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out intvalue) ? intvalue : defaultvalue);
		}

		public float GetPropertyValueFloat(string propname, int valueindex, float defaultvalue)
		{
			string str = GetPropertyValueString(propname, valueindex, string.Empty, false);

			// It can be negative...
			if(str == "-" && Properties.Count > valueindex + 1)
				str += GetPropertyValueString(propname, valueindex + 1, string.Empty, false);

			float fvalue;
			return (float.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out fvalue) ? fvalue : defaultvalue);
		}

		#endregion
	}
}
