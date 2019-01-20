
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
using System.IO;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Windows;
using CodeImp.DoomBuilder.IO;
using CodeImp.DoomBuilder.Actions;
using CodeImp.DoomBuilder.Data;
using CodeImp.DoomBuilder.Geometry;

#endregion

namespace CodeImp.DoomBuilder.Editing
{
	public class GridSetup : IDisposable
	{
		#region ================== Constants

		private const float DEFAULT_GRID_SIZE = 32f;
		internal const float MINIMUM_GRID_SIZE_UDMF = 0.125f; //mxd
		internal const float MINIMUM_GRID_SIZE = 1.0f; //mxd

		public const int SOURCE_TEXTURES = 0;
		public const int SOURCE_FLATS = 1;
		public const int SOURCE_FILE = 2;

		#endregion

		#region ================== Variables

		// Grid
		private int gridsize;
		private float gridsizef;
		private float gridsizefinv;
		private float gridrotate;
		private float gridoriginx, gridoriginy;

		// Background
		private string background = "";
		private int backsource;
		private ImageData backimage = new UnknownImage(null);
		private int backoffsetx, backoffsety;
		private float backscalex, backscaley;

		// Disposing
		private bool isdisposed;
		
		#endregion

		#region ================== Properties

		public int GridSize { get { return gridsize; } } //mxd
		public float GridSizeF { get { return gridsizef; } }
		public float GridRotate { get { return gridrotate; }}
		public float GridOriginX { get { return gridoriginx; }}
		public float GridOriginY { get { return gridoriginy; }}
		internal string BackgroundName { get { return background; } }
		internal int BackgroundSource { get { return backsource; } }
		internal ImageData Background { get { return backimage; } }
		internal int BackgroundX { get { return backoffsetx; } }
		internal int BackgroundY { get { return backoffsety; } }
		internal float BackgroundScaleX { get { return backscalex; } }
		internal float BackgroundScaleY { get { return backscaley; } }
		internal bool Disposed { get { return isdisposed; } }
		
		#endregion

		#region ================== Constructor / Disposer

		// Constructor
		internal GridSetup()
		{
			// Initialize
			SetGridSize(DEFAULT_GRID_SIZE);
			backscalex = 1.0f;
			backscaley = 1.0f;
			gridrotate = 0.0f;
			gridoriginx = 0;
			gridoriginy = 0;
			
			// Register actions
			General.Actions.BindMethods(this);
			
			// We have no destructor
			GC.SuppressFinalize(this);
		}

		// Disposer
		public void Dispose()
		{
			if(!isdisposed)
			{
				// Dispose image if needed
				if(backimage is FileImage) backimage.Dispose();
				
				// Clean up
				backimage = null;

				// Unregister actions
				General.Actions.UnbindMethods(this);

				// Done
				isdisposed = true;
			}
		}

		#endregion

		#region ================== Methods

		// Write settings to configuration
		internal void WriteToConfig(Configuration cfg, string path)
		{
			// Write settings
			cfg.WriteSetting(path + ".background", background);
			cfg.WriteSetting(path + ".backsource", backsource);
			cfg.WriteSetting(path + ".backoffsetx", backoffsetx);
			cfg.WriteSetting(path + ".backoffsety", backoffsety);
			cfg.WriteSetting(path + ".backscalex", (int)(backscalex * 100.0f));
			cfg.WriteSetting(path + ".backscaley", (int)(backscaley * 100.0f));
			cfg.WriteSetting(path + ".gridsize", gridsizef);
			cfg.WriteSetting(path + ".gridrotate", gridrotate);
			cfg.WriteSetting(path + ".gridoriginx", gridoriginx);
			cfg.WriteSetting(path + ".gridoriginy", gridoriginy);
		}

		// Read settings from configuration
		internal void ReadFromConfig(Configuration cfg, string path)
		{
			// Read settings
			background = cfg.ReadSetting(path + ".background", "");
			backsource = cfg.ReadSetting(path + ".backsource", 0);
			backoffsetx = cfg.ReadSetting(path + ".backoffsetx", 0);
			backoffsety = cfg.ReadSetting(path + ".backoffsety", 0);
			backscalex = cfg.ReadSetting(path + ".backscalex", 100) / 100.0f;
			backscaley = cfg.ReadSetting(path + ".backscaley", 100) / 100.0f;
			gridsizef = cfg.ReadSetting(path + ".gridsize", DEFAULT_GRID_SIZE);
			gridoriginx = cfg.ReadSetting(path + ".gridoriginx", 0);
			gridoriginy = cfg.ReadSetting(path + ".gridoriginy", 0);
			gridrotate = cfg.ReadSetting(path + ".gridrotate", 0.0f);

			// Setup
			SetGridSize(gridsizef);
			LinkBackground();
		}

		// This sets the grid size
		internal void SetGridSize(float size)
		{
			//mxd. Bad things happen when size <= 0
			size = Math.Max(size, ((General.Map != null && General.Map.UDMF) ? MINIMUM_GRID_SIZE_UDMF : MINIMUM_GRID_SIZE));
			
			// Change grid
			gridsizef = size;
			gridsize = (int)Math.Max(1, Math.Round(gridsizef)); //mxd
			gridsizefinv = 1f / gridsizef;

			// Update in main window
			General.MainWindow.UpdateGrid(gridsizef);
		}

		// Set the rotation angle of the grid
		internal void SetGridRotation(float angle)
		{
			gridrotate = angle;
		}

		// Set the origin of the grid
		internal void SetGridOrigin(float x, float y)
		{
			gridoriginx = x;
			gridoriginy = y;
		}

		// This sets the background
		internal void SetBackground(string name, int source)
		{
			// Set background
			if(name == null) name = "";
			this.backsource = source;
			this.background = name;

			// Find this image
			LinkBackground();
		}

		// This sets the background view
		internal void SetBackgroundView(int offsetx, int offsety, float scalex, float scaley)
		{
			// Set background offset
			this.backoffsetx = offsetx;
			this.backoffsety = offsety;
			this.backscalex = scalex;
			this.backscaley = scaley;
		}
		
		// This finds and links the background image
		internal void LinkBackground()
		{
			// Dispose image if needed
			if(backimage is FileImage) backimage.Dispose();
			
			// Where to load background from?
			switch(backsource)
			{
				case SOURCE_TEXTURES:
					backimage = General.Map.Data.GetTextureImage(background);
					break;

				case SOURCE_FLATS:
					backimage = General.Map.Data.GetFlatImage(background);
					break;

				case SOURCE_FILE:
					backimage = new FileImage(Path.GetFileNameWithoutExtension(background), background, false, 1.0f, 1.0f);
					break;
			}

			// Make sure it is loaded
			backimage.LoadImage();
			backimage.CreateTexture();
		}
		
		// This returns the next higher coordinate
		public float GetHigher(float offset)
		{
			return (float)Math.Round((offset + (gridsizef * 0.5f)) * gridsizefinv) * gridsizef;
		}

		// This returns the next lower coordinate
		public float GetLower(float offset)
		{
			return (float)Math.Round((offset - (gridsizef * 0.5f)) * gridsizefinv) * gridsizef;
		}
		
		// This snaps to the nearest grid coordinate
		public Vector2D SnappedToGrid(Vector2D v)
		{
			return SnappedToGrid(v, gridsizef, gridsizefinv, gridrotate, gridoriginx, gridoriginy);
		}

		// This snaps to the nearest grid coordinate
		public static Vector2D SnappedToGrid(Vector2D v, float gridsize, float gridsizeinv, float gridrotate = 0.0f, float gridoriginx = 0, float gridoriginy = 0)
		{
			Vector2D origin = new Vector2D(gridoriginx, gridoriginy);
			bool transformed = Math.Abs(gridrotate) > 1e-4 || gridoriginx != 0 || gridoriginx != 0;
			if (transformed)
			{
				// Grid is transformed, so reverse the transformation first
				v = ((v - origin).GetRotated(-gridrotate));
			}
		
			Vector2D sv = new Vector2D((float)Math.Round(v.x * gridsizeinv) * gridsize,
								(float)Math.Round(v.y * gridsizeinv) * gridsize);

			if (transformed)
			{
				// Put back into original frame
				sv = sv.GetRotated(gridrotate) + origin;
			}

			if(sv.x < General.Map.Config.LeftBoundary) sv.x = General.Map.Config.LeftBoundary;
			else if(sv.x > General.Map.Config.RightBoundary) sv.x = General.Map.Config.RightBoundary;

			if(sv.y > General.Map.Config.TopBoundary) sv.y = General.Map.Config.TopBoundary;
			else if(sv.y < General.Map.Config.BottomBoundary) sv.y = General.Map.Config.BottomBoundary;

			return sv;
		}

		#endregion

		#region ================== Actions

		// This shows the grid setup dialog
		internal static void ShowGridSetup()
		{
			// Show preferences dialog
			GridSetupForm gridform = new GridSetupForm();
			if(gridform.ShowDialog(General.MainWindow) == DialogResult.OK)
			{
				// Redraw display
				General.MainWindow.RedrawDisplay();
			}

			// Done
			gridform.Dispose();
		}
		
		// This changes grid size
		// Note: these were incorrectly swapped before, hence the wrong action name
		[BeginAction("gridinc")]
		internal void DecreaseGrid()
		{
			//mxd. Not lower than 0.125 in UDMF or 1 otherwise
			float preminsize = (General.Map.UDMF ? MINIMUM_GRID_SIZE_UDMF * 2 : MINIMUM_GRID_SIZE * 2);
			if(gridsizef >= preminsize)
			{
				//mxd. Disable automatic grid resizing
				General.MainWindow.DisableDynamicGridResize();
				
				// Change grid
				SetGridSize(gridsizef / 2);
				
				// Redraw display
				General.MainWindow.RedrawDisplay();
			}
		}

		// This changes grid size
		// Note: these were incorrectly swapped before, hence the wrong action name
		[BeginAction("griddec")]
		internal void IncreaseGrid()
		{
			// Not higher than 1024
			if(gridsizef <= 512)
			{
				//mxd. Disable automatic grid resizing
				General.MainWindow.DisableDynamicGridResize();
				
				// Change grid
				SetGridSize(gridsizef * 2);

				// Redraw display
				General.MainWindow.RedrawDisplay();
			}
		}

		#endregion
	}
}
