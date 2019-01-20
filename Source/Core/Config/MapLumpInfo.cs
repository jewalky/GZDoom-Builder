
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

using CodeImp.DoomBuilder.IO;

#endregion

namespace CodeImp.DoomBuilder.Config
{
	public struct MapLumpInfo
	{
		// Members
		public readonly string Name;
		public readonly bool Required;
		public readonly bool BlindCopy;
		public readonly bool NodeBuild;
		public readonly bool AllowEmpty;
		public readonly bool ScriptBuild; //mxd
        public readonly bool Forbidden; // [ZZ]
		internal readonly ScriptConfiguration Script;
		
		// Construct from IDictionary
		internal MapLumpInfo(string name, Configuration cfg)
		{
			// Apply settings
			this.Name = name;
			this.Script = null;
			this.Required = cfg.ReadSetting("maplumpnames." + name + ".required", false);
			this.BlindCopy = cfg.ReadSetting("maplumpnames." + name + ".blindcopy", false);
			this.NodeBuild = cfg.ReadSetting("maplumpnames." + name + ".nodebuild", false);
			this.AllowEmpty = cfg.ReadSetting("maplumpnames." + name + ".allowempty", false);
			this.ScriptBuild = cfg.ReadSetting("maplumpnames." + name + ".scriptbuild", false); //mxd
            this.Forbidden = cfg.ReadSetting("maplumpnames." + name + ".forbidden", false); //mxd
            string scriptconfig = (this.ScriptBuild ? string.Empty : cfg.ReadSetting("maplumpnames." + name + ".script", "")); //mxd. Setting Script when "scriptbuild" is true can result in unexpected behaviour...
			
			// Find script configuration
			if(scriptconfig.Length > 0)
			{
				if(General.ScriptConfigs.ContainsKey(scriptconfig.ToLowerInvariant()))
				{
					this.Script = General.ScriptConfigs[scriptconfig.ToLowerInvariant()];
				}
				else
				{
					General.ErrorLogger.Add(ErrorType.Warning, "Map lump \"" + name + "\" in the current game configuration specifies an unknown script configuration \"" + scriptconfig + "\". Using plain text instead.");
					this.Script = new ScriptConfiguration();
				}
			}
		}
    }
}

