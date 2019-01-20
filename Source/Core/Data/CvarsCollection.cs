#region ================== Namespaces

using System;
using System.Collections.Generic;
using CodeImp.DoomBuilder.Rendering;

#endregion

namespace CodeImp.DoomBuilder.Data
{
	internal class CvarsCollection
	{
		#region ================== Variables

		internal readonly Dictionary<string, int> Integers;
		internal readonly Dictionary<string, float> Floats;
		internal readonly Dictionary<string, PixelColor> Colors;
		internal readonly Dictionary<string, bool> Booleans;
		internal readonly Dictionary<string, string> Strings;
		private readonly HashSet<string> allnames;

		#endregion

		#region ================== Constructor

		public CvarsCollection()
		{
			Integers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
			Floats = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);
			Colors = new Dictionary<string, PixelColor>(StringComparer.OrdinalIgnoreCase);
			Booleans = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
			Strings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			allnames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		}

		#endregion

		#region ================== Methods

		public bool AddValue(string name, int value)
		{
			if(allnames.Contains(name)) return false;
			allnames.Add(name);
			Integers.Add(name, value);
			return true;
		}

		public bool AddValue(string name, float value)
		{
			if(allnames.Contains(name)) return false;
			allnames.Add(name);
			Floats.Add(name, value);
			return true;
		}

		public bool AddValue(string name, PixelColor value)
		{
			if(allnames.Contains(name)) return false;
			allnames.Add(name);
			Colors.Add(name, value);
			return true;
		}

		public bool AddValue(string name, bool value)
		{
			if(allnames.Contains(name)) return false;
			allnames.Add(name);
			Booleans.Add(name, value);
			return true;
		}

		public bool AddValue(string name, string value)
		{
			if(allnames.Contains(name)) return false;
			allnames.Add(name);
			Strings.Add(name, value);
			return true;
		}

		#endregion
	}
}
