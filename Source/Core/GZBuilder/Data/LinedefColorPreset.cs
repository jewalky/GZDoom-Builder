#region ================== Namespaces

using System.Collections.Generic;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;

#endregion

namespace CodeImp.DoomBuilder.GZBuilder.Data
{
	public class LinedefColorPreset
	{
		#region ================== Properties

		public string Name;
		public PixelColor Color;
		public bool Enabled;
		public int Action;
		public int Activation; //Hexen activation type
		public readonly List<string> Flags;
		public readonly List<string> RestrictedFlags;

		#endregion

		#region ================== Constructors

		public LinedefColorPreset(string name, PixelColor linecolor) 
		{
			Name = name;
			Color = linecolor;
			Flags = new List<string>();
			RestrictedFlags = new List<string>();
			Enabled = true;
		}

		public LinedefColorPreset(string name, PixelColor linecolor, int action, int activation, List<string> flags, List<string> restrictedFlags, bool enabled) 
		{
			Name = name;
			Color = linecolor;
			Action = action;
			Activation = activation;
			Flags = flags;
			RestrictedFlags = restrictedFlags;
			Enabled = enabled;
		}

		public LinedefColorPreset(LinedefColorPreset other) 
		{
			Name = other.Name;
			Color = other.Color;
			Action = other.Action;
			Activation = other.Activation;
			Enabled = other.Enabled;
			Flags = new List<string>(other.Flags);
			RestrictedFlags = new List<string>(other.RestrictedFlags);
		}

		#endregion

		#region ================== Methods

		public bool Matches(Linedef l)
		{
			if(!Enabled) return false;
			
			// Check action; -1 means Any Action
			if(Action != 0) 
			{
				if((Action == -1 && l.Action == 0) || (Action != -1 && l.Action != Action))
					return false;
			}

			// Check activation; -1 means Any Activation
			if(Activation != 0) 
			{
				if(!General.Map.UDMF && (l.Activate != Activation || (Activation == -1 && l.Activate == 0)))
					return false;
			}

			// Check flags
			if(Flags.Count > 0) foreach(string s in Flags) if(!l.IsFlagSet(s)) return false;

			// Check flags, which should be disabled
			if(RestrictedFlags.Count > 0) foreach(string s in RestrictedFlags) if(l.IsFlagSet(s)) return false;

			return true;
		}

		public bool IsValid()
		{
			return Action != 0 || Flags.Count > 0 || RestrictedFlags.Count > 0 || Activation != 0;
		}

		public override string ToString() 
		{
			return Name;
		}

		#endregion
	}
}
