
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
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	public class ResultUnknownFlat : ErrorResult
	{
		#region ================== Variables
		
		private readonly Sector sector;
		private readonly bool ceiling;
		private static string imagename = "-"; //mxd
		
		#endregion
		
		#region ================== Properties

		public override int Buttons { get { return 2; } }
		public override string Button1Text { get { return "Add Default Flat"; } }
		public override string Button2Text { get { return "Browse Flat..."; } } //mxd
		
		#endregion
		
		#region ================== Constructor / Destructor
		
		// Constructor
		public ResultUnknownFlat(Sector s, bool ceiling)
		{
			// Initialize
			this.sector = s;
			this.ceiling = ceiling;
			this.viewobjects.Add(s);
			this.hidden = s.IgnoredErrorChecks.Contains(this.GetType()); //mxd
			imagename = "-"; //mxd

			string objname = ceiling ? "ceiling" : "floor";
			this.description = "This sector's " + objname + " uses an unknown flat. This could be the result of missing resources, or a mistyped flat name.";
		}
		
		#endregion
		
		#region ================== Methods

		// This sets if this result is displayed in ErrorCheckForm (mxd)
		internal override void Hide(bool hide) 
		{
			hidden = hide;
			Type t = this.GetType();
			if(hide) sector.IgnoredErrorChecks.Add(t);
			else if(sector.IgnoredErrorChecks.Contains(t)) sector.IgnoredErrorChecks.Remove(t);
		}
		
		// This must return the string that is displayed in the listbox
		public override string ToString()
		{
			if(ceiling)
				return "Sector " + sector.Index + " has unknown ceiling flat \"" + sector.CeilTexture + "\"";
			else
				return "Sector " + sector.Index + " has unknown floor flat \"" + sector.FloorTexture + "\"";
		}
		
		// Rendering
		public override void PlotSelection(IRenderer2D renderer)
		{
			renderer.PlotSector(sector, General.Colors.Selection);
		}

		//mxd. More rendering
		public override void RenderOverlaySelection(IRenderer2D renderer) 
		{
			if(!General.Settings.UseHighlight) return;
			renderer.RenderHighlight(sector.FlatVertices, General.Colors.Selection.WithAlpha(64).ToInt());
		}
		
		// Fix by setting default flat
		public override bool Button1Click(bool batchMode)
		{
			if(!batchMode) General.Map.UndoRedo.CreateUndo("Unknown flat correction");
			General.Settings.FindDefaultDrawSettings();
			
			if(ceiling)
				sector.SetCeilTexture(General.Map.Options.DefaultCeilingTexture);
			else
				sector.SetFloorTexture(General.Map.Options.DefaultFloorTexture);
			
			General.Map.Map.Update();
			General.Map.Data.UpdateUsedTextures();
			return true;
		}

		//mxd. Fix by picking a flat
		public override bool Button2Click(bool batchMode) 
		{
			if(!batchMode) General.Map.UndoRedo.CreateUndo("Unknown flat correction");
			if(imagename == "-") imagename = General.Interface.BrowseFlat(General.Interface, imagename);
			if(imagename == "-") return false;

			if(ceiling) sector.SetCeilTexture(imagename);
			else sector.SetFloorTexture(imagename);

			General.Map.Map.Update();
			General.Map.Data.UpdateUsedTextures();
			return true;
		}
		
		#endregion
	}
}
