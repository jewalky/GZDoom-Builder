
#region ================== Copyright (c) 2012 Pascal vd Heiden

/*
 * Copyright (c) 2012 Pascal vd Heiden, www.codeimp.com
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
using System.Globalization;
using System.IO;
using CodeImp.DoomBuilder.Plugins.VisplaneExplorer.Properties;
using CodeImp.DoomBuilder.Windows;

#endregion

namespace CodeImp.DoomBuilder.Plugins.VisplaneExplorer
{
	public class BuilderPlug : Plug
	{
		#region ================== Variables

		// Objects
		private static BuilderPlug me;
		private VPOManager vpo;
		private InterfaceForm interfaceform;

		// Palettes
		private Palette[] palettes;
		
		#endregion

		#region ================== Properties
		
		// Properties
		public override string Name { get { return "VisplaneExplorer"; } }
		internal static VPOManager VPO { get { return me.vpo; } }
		internal static InterfaceForm InterfaceForm { get { return me.interfaceform; } }
		internal static Palette[] Palettes { get { return me.GetPalettes(); } }
		public override int MinimumRevision { get { return 2411; } }
		
		#endregion

		#region ================== Initialize / Dispose

		public override void OnInitialize()
		{
			base.OnInitialize();

			// Keep a static reference
			me = this;
		}

		//mxd
		public static void InitVPO()
		{
			// Load interface controls
			if(me.interfaceform == null) me.interfaceform = new InterfaceForm();

			// Load VPO manager (manages multithreading and communication with vpo.dll)
			if(me.vpo == null) me.vpo = new VPOManager();
		}

		// Preferences changed
		public override void OnClosePreferences(PreferencesController controller)
		{
			base.OnClosePreferences(controller);
			ApplyUserColors();
		}

		// This is called when the plugin is terminated
		public override void Dispose()
		{
			//mxd. Active and not already disposed?
			if(!IsDisposed)
			{
				if(interfaceform != null)
				{
					interfaceform.Dispose();
					interfaceform = null;
				}

				if(vpo != null)
				{
					vpo.Dispose();
					vpo = null;
				}

				// Done
				me = null;

				base.Dispose();
			}
		}

		#endregion

		#region ================== Methods

		//mxd
		private Palette[] GetPalettes()
		{
			if(palettes == null)
			{
				// Load palettes
				palettes = new Palette[(int)ViewStats.NumStats];
				palettes[(int)ViewStats.Visplanes] = new Palette(Resources.Visplanes_pal);
				palettes[(int)ViewStats.Drawsegs] = new Palette(Resources.Drawsegs_pal);
				palettes[(int)ViewStats.Solidsegs] = new Palette(Resources.Solidsegs_pal);
				palettes[(int)ViewStats.Openings] = new Palette(Resources.Openings_pal);
				palettes[(int)ViewStats.Heatmap] = new Palette(Resources.Heatmap_pal); //mxd
				ApplyUserColors();
			}
			return palettes;
		}

		// This applies user-defined appearance colors to the palettes
		private void ApplyUserColors()
		{
			//mxd
			if(palettes == null)
			{
				GetPalettes();
				return;
			}
			
			// Override special palette indices with user-defined colors
			foreach(Palette p in palettes) p.SetColor(Tile.POINT_VOID_B, General.Colors.Background.WithAlpha(0).ToInt());
		}

		// This returns a unique temp filename
		public static string MakeTempFilename(string extension)
		{
			string filename;

			do
			{
				//mxd. Generate a filename
				filename = Path.Combine(General.TempPath, MurmurHash2.Hash(DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture)) + extension);
			}
			// Continue while file is not unique
			while(File.Exists(filename) || Directory.Exists(filename));

			// Return the filename
			return filename;
		}

		#endregion
	}
}
