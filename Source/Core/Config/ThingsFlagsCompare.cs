
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
using CodeImp.DoomBuilder.Map;

#endregion

namespace CodeImp.DoomBuilder.Config
{
	//mxd
	public class ThingFlagsCompareGroup
	{
		public readonly string Name;
		public readonly bool IsOptional; // When set to true, group flags won't be considered as required for a thing to show up ingame by CheckUnusedThings error check and ThingFlagsCompare.CheckThingEditFormFlags() method.
		public readonly Dictionary<string, ThingFlagsCompare> Flags;

		public ThingFlagsCompareGroup(Configuration cfg, string name)
		{
			Name = name;
			Flags = new Dictionary<string, ThingFlagsCompare>();
			IsOptional = cfg.ReadSetting("thingflagscompare." + name + ".optional", false);

			IDictionary dic = cfg.ReadSetting("thingflagscompare." + name, new Hashtable());
			foreach(DictionaryEntry de in dic)
			{
				if(de.Value != null && !(de.Value is IDictionary)) continue; // flag either has no value, or is defined as block
				string flag = de.Key.ToString();

				// Duplicate flags check
				if(Flags.ContainsKey(flag))
					General.ErrorLogger.Add(ErrorType.Warning, "ThingFlagsCompare flag \"" + flag + "\" is double defined in the \"" + name + "\" group");

				Flags[flag] = new ThingFlagsCompare(cfg, name, flag);
			}
		}

		// Compares flags group of the two things.
		public ThingFlagsCompareResult Compare(Thing t1, Thing t2)
		{
			ThingFlagsCompareResult result = new ThingFlagsCompareResult();
			foreach(ThingFlagsCompare tfc in Flags.Values)
			{
				// Current flag doesn't overlap when required flag does not overlap
				if(!string.IsNullOrEmpty(tfc.RequiredFlag) && !GetFlag(tfc.RequiredFlag).Compare(t1, t2))
				{
					result.Result = -1;
					continue;
				}

				// Compare current flag
				bool flagoverlaps = tfc.Compare(t1, t2);

				// Ignore this group when whole group doens't match or required flag is not set
				if(!flagoverlaps && tfc.IgnoreGroupWhenUnset) return new ThingFlagsCompareResult { Result = 0 };

				// If current flag overlaps, check IgnoredGroup and RequiredGroup settings
				if(flagoverlaps)
				{
					result.Result = 1;
					
					foreach(string s in tfc.IgnoredGroups)
					{
						if(!result.IgnoredGroups.Contains(s)) result.IgnoredGroups.Add(s);
					}

					if(tfc.RequiredGroups.Count > 0)
					{
						foreach(string s in tfc.RequiredGroups)
						{
							if(result.IgnoredGroups.Contains(s)) result.IgnoredGroups.Remove(s);
							if(!result.RequiredGroups.Contains(s)) result.RequiredGroups.Add(s);
						}
					}
				}
			}

			return result;
		}

		public ThingFlagsCompare GetFlag(string flag)
		{
			// Check our flags
			if(Flags.ContainsKey(flag)) return Flags[flag];

			// Check other groups
			foreach(ThingFlagsCompareGroup group in General.Map.Config.ThingFlagsCompare.Values)
			{
				if(group != this && group.Flags.ContainsKey(flag)) return group.Flags[flag];
			}

			// Fial...
			return null;
		}
	}

	//mxd
	public class ThingFlagsCompareResult
	{
		public readonly HashSet<string> IgnoredGroups;
		public readonly HashSet<string> RequiredGroups;
		
		// -1 if group does not overlap
		//	0 if group should be ignored
		//	1 if group overlaps
		public int Result;

		public ThingFlagsCompareResult()
		{
			Result = -1;
			IgnoredGroups = new HashSet<string>();
			RequiredGroups = new HashSet<string>();
		}
	}
	
	public class ThingFlagsCompare
	{
		private enum CompareMethod
		{
			Equal,
			And
		};

		#region ================== Constants

		#endregion

		#region ================== Variables

		private readonly string flag;
		private readonly HashSet<string> requiredgroups; //mxd. This flag only works if at least one flag is set in the "requiredgroup"
		private readonly HashSet<string> ignoredgroups; //mxd. If this flag is set, flags from ignoredgroup can be... well... ignored!
		private string requiredflag; //mxd. This flag only works if requiredflag is set.
		private readonly bool ingnorethisgroupwhenunset; //mxd
		private readonly CompareMethod comparemethod;
		private readonly bool invert;
		private readonly char[] comma = new[] {','};

		#endregion

		#region ================== Properties

		public string Flag { get { return flag; } }
		public HashSet<string> RequiredGroups { get { return requiredgroups; } } //mxd
		public HashSet<string> IgnoredGroups { get { return ignoredgroups; } } //mxd
		public string RequiredFlag { get { return requiredflag; } internal set { requiredflag = value; } } //mxd
		public bool IgnoreGroupWhenUnset { get { return ingnorethisgroupwhenunset; } } //mxd

		#endregion

		#region ================== Constructor / Disposer

		// Constructor
		public ThingFlagsCompare(Configuration cfg, string group, string flag)
		{
			string cfgpath = "thingflagscompare." + group + "." + flag;
			this.flag = flag;

			string cm = cfg.ReadSetting(cfgpath + ".comparemethod", "and");
			switch(cm)
			{
				default:
					General.ErrorLogger.Add(ErrorType.Warning, "Unrecognized value \"" + cm + "\" for comparemethod in " + cfgpath + " in game configuration " + cfg.ReadSetting("game", "<unnamed game>") + ". Defaulting to \"and\".");
					goto case "and";
				case "and":
					comparemethod = CompareMethod.And;
					break;
				case "equal":
					comparemethod = CompareMethod.Equal;
					break;
			}

			invert = cfg.ReadSetting(cfgpath + ".invert", false);
			
			//mxd
			requiredgroups = new HashSet<string>();
			string[] requiredgroupsarr = cfg.ReadSetting(cfgpath + ".requiredgroups", string.Empty).Split(comma, StringSplitOptions.RemoveEmptyEntries);
			foreach(string s in requiredgroupsarr)
				if(!requiredgroups.Contains(s)) requiredgroups.Add(s);

			//mxd
			ignoredgroups = new HashSet<string>();
			string[] ignoredgroupsarr = cfg.ReadSetting(cfgpath + ".ignoredgroups", string.Empty).Split(comma, StringSplitOptions.RemoveEmptyEntries);
			foreach(string s in ignoredgroupsarr)
				if(!ignoredgroups.Contains(s)) ignoredgroups.Add(s);

			requiredflag = cfg.ReadSetting(cfgpath + ".requiredflag", string.Empty); //mxd
			ingnorethisgroupwhenunset = cfg.ReadSetting(cfgpath + ".ingnorethisgroupwhenunset", false); //mxd
			
			// We have no destructor
			GC.SuppressFinalize(this);
		}

		#endregion

		#region ================== Methods

		// Compares the flag of the two things.
		public bool Compare(Thing t1, Thing t2)
		{
			//mxd. Get flags
			bool t1flag = (invert ? !t1.IsFlagSet(flag) : t1.IsFlagSet(flag));
			bool t2flag = (invert ? !t2.IsFlagSet(flag) : t2.IsFlagSet(flag));

			//mxd. Ignore the flag when ingnorethisgroupwhenunset is set and both flags are unset
			if(!t1flag && !t2flag && ingnorethisgroupwhenunset) return false;

			//mxd. Compare them
			switch(comparemethod)
			{
				case CompareMethod.And: return t1flag && t2flag;
				case CompareMethod.Equal: return t1flag == t2flag;
				default: throw new NotImplementedException("Unknown compare method!");
			}
		}

		//mxd
		public static List<string> CheckFlags(HashSet<string> flags)
		{
			Dictionary<string, HashSet<string>> flagspergroup = new Dictionary<string, HashSet<string>>(General.Map.Config.ThingFlagsCompare.Count);
			HashSet<string> ignoredgroups = new HashSet<string>();

			// Gather flags per group
			foreach(KeyValuePair<string, ThingFlagsCompareGroup> group in General.Map.Config.ThingFlagsCompare)
			{
				flagspergroup.Add(group.Key, new HashSet<string>());

				foreach(ThingFlagsCompare flag in group.Value.Flags.Values)
				{
					if(IsFlagSet(flags, flag.flag, flag.invert) && 
						(string.IsNullOrEmpty(flag.requiredflag) || IsFlagSet(flags, flag.requiredflag, group.Value.GetFlag(flag.requiredflag).invert)))
					{
						flagspergroup[group.Key].Add(flag.Flag);
						foreach(string s in flag.ignoredgroups)
							if(!ignoredgroups.Contains(s)) ignoredgroups.Add(s);
					}
					else if(flag.ingnorethisgroupwhenunset)
					{
						flagspergroup.Remove(group.Key);
						break;
					}
				}
			}

			// Check required dependancies
			foreach(KeyValuePair<string, HashSet<string>> group in flagspergroup)
			{
				foreach(string flag in group.Value)
				{
					foreach(string s in General.Map.Config.ThingFlagsCompare[group.Key].Flags[flag].requiredgroups)
					{
						if(ignoredgroups.Contains(s)) ignoredgroups.Remove(s);
					}
				}
			}

			// Get rid of ignoredgroups
			foreach(string s in ignoredgroups) flagspergroup.Remove(s);
			
			// Return message
			List<string> result = new List<string>();

			foreach(KeyValuePair<string, HashSet<string>> group in flagspergroup)
			{
				if(group.Value.Count == 0 && !General.Map.Config.ThingFlagsCompare[group.Key].IsOptional)
				{
					switch(group.Key) 
					{
						case "skills":
							result.Add("Thing is not used in any skill level.");
							break;
						case "gamemodes":
							result.Add("Thing is not used in any game mode.");
							break;
						case "classes":
							result.Add("Thing is not used by any class.");
							break;
						default:
							result.Add("At least one \"" + group.Key + "\" flag should be set.");
							break;
					}
				}
			}

			return result;
		}

		//mxd
		private static bool IsFlagSet(HashSet<string> flags, string flag, bool invert)
		{
			bool result = flags.Contains(flag);
			return (invert ? !result : result);
		}

		#endregion
	}
}

