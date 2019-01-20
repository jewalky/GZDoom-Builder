
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
using System.Windows.Forms;
using CodeImp.DoomBuilder.GZBuilder;
using CodeImp.DoomBuilder.Map;
using System.Drawing;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Types;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	[FindReplace("Linedef Action and Arguments", BrowseButton = true)]
	internal class FindLinedefTypes : BaseFindLinedef
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		#endregion

		#region ================== Properties

		public override Image BrowseImage { get { return Properties.Resources.List; } }
		public override string UsageHint { get { return "Usage: action [arg1 [arg2 [arg3 [arg4 [arg5]]]]]" + Environment.NewLine
					+ "Arg value can be \"*\" (any value)" + Environment.NewLine
					+ "Arg1 can be script name when searching for ACS specials"; } }
		
		#endregion

		#region ================== Constructor / Destructor

		#endregion

		#region ================== Methods

		// This is called when the browse button is pressed
		public override string Browse(string initialvalue)
		{
			int num;
			int.TryParse(initialvalue, out num);
			return General.Interface.BrowseLinedefActions(BuilderPlug.Me.FindReplaceForm, num, true).ToString();
		}

		//mxd. This is called when the browse replace button is pressed
		public override string BrowseReplace(string initialvalue)
		{
			int num;
			int.TryParse(initialvalue, out num);
			return General.Interface.BrowseLinedefActions(BuilderPlug.Me.FindReplaceForm, num).ToString();
		}


		// This is called to perform a search (and replace)
		// Returns a list of items to show in the results list
		// replacewith is null when not replacing
		public override FindReplaceObject[] Find(string value, bool withinselection, bool replace, string replacewith, bool keepselection)
		{
			List<FindReplaceObject> objs = new List<FindReplaceObject>();

			// Interpret the replacement
			int replaceaction = 0;
			string replacearg0str = string.Empty; //mxd
			int[] replaceargs = null; //mxd
			if(replace)
			{
				string[] replaceparts = replacewith.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				
				// If it cannot be interpreted, set replacewith to null (not replacing at all)
				if(replaceparts.Length == 0) replacewith = null; //mxd
				if(!int.TryParse(replaceparts[0], out replaceaction)) replacewith = null;
				if(replaceaction < 0) replacewith = null;
				if(replaceaction > Int16.MaxValue) replacewith = null;
				if(replacewith == null)
				{
					MessageBox.Show("Invalid replace value for this search type!", "Find and Replace", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return objs.ToArray();
				}

				//mxd. Now try parsing the args
				if(replaceparts.Length > 1)
				{
					replaceargs = new[] { int.MinValue, int.MinValue, int.MinValue, int.MinValue, int.MinValue };
					int i = 1;

					//mxd. Named script search support...
					if(General.Map.UDMF)
					{
                        string possiblescriptname = replaceparts[1].Trim();
						int tmp;
						if(!string.IsNullOrEmpty(possiblescriptname) && possiblescriptname != "*" && !int.TryParse(possiblescriptname, out tmp)) 
						{
                            replacearg0str = possiblescriptname.Replace("\"", "");
							i = 2;
						}
					}

					for(; i < replaceparts.Length && i < replaceargs.Length + 1; i++) 
					{
						int argout;
						if(replaceparts[i].Trim() == "*") continue; //mxd. Any arg value support
						if(int.TryParse(replaceparts[i].Trim(), out argout)) replaceargs[i - 1] = argout;
					}
				}
			}

			// Interpret the number given
			int action;
			string[] parts = value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

			//For the search, the user may make the following queries:
			//	action arg0 arg1 arg2 arg3 arg4
			//	action arg0str arg1 arg2 arg3 arg4
			//
			//this allows users to search for lines that contain actions with specific arguments.
			//useful for locating script lines

			if(int.TryParse(parts[0], out action))
			{
				int[] args = null;
				string arg0str = string.Empty; //mxd

                bool isacs = Array.IndexOf(GZGeneral.ACS_SPECIALS, action) != -1;

                //parse the arg values out
                if (parts.Length > 1) 
				{
					args = new[] { int.MinValue, int.MinValue, int.MinValue, int.MinValue, int.MinValue };
					int i = 1;

                    //mxd. Named script search support...
                    if (General.Map.UDMF)
					{
                        // [ZZ] edit: we can enclose number with "" to signify a named script called "1".
                        //      this is achieved by trying to parse the number before removing "'s.
                        string possiblescriptname = parts[1].Trim();
                        int tmp;
                        if (!string.IsNullOrEmpty(possiblescriptname) && possiblescriptname != "*" && !int.TryParse(possiblescriptname, out tmp))
                        {
                            arg0str = possiblescriptname.Replace("\"", "").ToLowerInvariant();
                            i = 2;
                        }
					}

					for(; i < parts.Length && i < args.Length + 1; i++)
					{
						int argout;
						if(parts[i].Trim() == "*") continue; //mxd. Any arg value support
						if(int.TryParse(parts[i].Trim(), out argout)) args[i - 1] = argout;
					}
				}

				//mxd
				HashSet<int> expectedbits = GetGeneralizedBits(action);

				// Where to search?
				ICollection<Linedef> list = withinselection ? General.Map.Map.GetSelectedLinedefs(true) : General.Map.Map.Linedefs;

				// Go for all linedefs
				foreach(Linedef l in list)
				{
					// Action matches? -1 means any action (mxd)
					if((action == -1 && l.Action == 0) || (action > -1 && (l.Action != action && !BitsMatch(l.Action, expectedbits)))) continue;

					bool match = true;
					string argtext = "";

					//if args were specified, then process them
					if(args != null) 
					{
						int x = 0;
						argtext = ". Args: ";

						//mxd. Check script name...
						if(!string.IsNullOrEmpty(arg0str))
						{
							string s = l.Fields.GetValue("arg0str", string.Empty);
							if(s.ToLowerInvariant() != arg0str)
								match = false;
							else
								argtext += "\"" + s + "\"";

							x = 1;
						}

						for(; x < args.Length; x++)
						{
							if(args[x] != int.MinValue && args[x] != l.Args[x]) 
							{
								match = false;
								break;
							}
							argtext += (x == 0 ? "" : ", ") + l.Args[x];
						}
					}
					//mxd. Add action args
					else
					{
						List<string> argslist = new List<string>();
						
						// Process args, drop trailing zeroes
						for(int i = l.Args.Length - 1; i > -1; i--)
						{
							if(l.Args[i] == 0 && argslist.Count == 0) continue; // Skip tail zeroes
							argslist.Insert(0, l.Args[i].ToString());
						}

						// Process arg0str...
						//if(Array.IndexOf(GZGeneral.ACS_SPECIALS, l.Action) != -1)
						{
							string s = l.Fields.GetValue("arg0str", string.Empty);
							if(!string.IsNullOrEmpty(s))
							{
								if(argslist.Count > 0)
									argslist[0] = "\"" + s + "\"";
								else
									argslist.Add("\"" + s + "\"");
							}
						}

						// Create args string
						if(argslist.Count > 0)
						{
							argtext = ". Args: " + string.Join(", ", argslist.ToArray());
						}
					}

					if(match) 
					{
                        // Replace
                        LinedefActionInfo info = General.Map.Config.GetLinedefActionInfo(l.Action);

                        if (replace)
						{
							l.Action = replaceaction;
                            info = General.Map.Config.GetLinedefActionInfo(l.Action);

                            //mxd. Replace args as well?
                            if (replaceargs != null)
							{
								int i = 0;
								if(!string.IsNullOrEmpty(replacearg0str) && info.Args[0].Str) // [ZZ] make sure that arg0str is supported for this special.
								{
									l.Fields["arg0str"] = new UniValue(UniversalType.String, replacearg0str);
									i = 1;
								}

								for(; i < replaceargs.Length; i++)
								{
									if(replaceargs[i] != int.MinValue) l.Args[i] = replaceargs[i];
								}
							}
						}

						// Add to list
						if(!info.IsNull)
							objs.Add(new FindReplaceObject(l, "Linedef " + l.Index + " (" + info.Title + argtext + ")"));
						else
							objs.Add(new FindReplaceObject(l, "Linedef " + l.Index + " (Action " + l.Action + argtext + ")"));
					}
				}
			}

			return objs.ToArray();
		}

		//mxd
		private static HashSet<int> GetGeneralizedBits(int action) 
		{
			if(!General.Map.Config.GeneralizedActions) return new HashSet<int>();
			HashSet<int> bits = new HashSet<int>();

			foreach(GeneralizedCategory cat in General.Map.Config.GenActionCategories) 
			{
				if((action < cat.Offset) || (action >= (cat.Offset + cat.Length))) continue;
				int actionbits = action - cat.Offset;
				foreach(GeneralizedOption option in cat.Options) 
				{
					foreach(GeneralizedBit bit in option.Bits) 
					{
						if(bit.Index > 0 && (actionbits & bit.Index) == bit.Index)
							bits.Add(bit.Index);
					}
				}
			}

			return bits;
		}

		//mxd
		private static bool BitsMatch(int action, HashSet<int> expectedbits) 
		{
			if(!General.Map.Config.GeneralizedActions || expectedbits.Count == 0) return false;

			HashSet<int> bits = GetGeneralizedBits(action);
			if(bits.Count == 0) return false;

			foreach(int bit in expectedbits) 
			{
				if(bits.Contains(bit)) return true;
			}

			return false;
		}
		
		#endregion
	}
}
