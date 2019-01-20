
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
using System.Drawing;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Data;
using CodeImp.DoomBuilder.Editing;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Map;

#endregion

namespace CodeImp.DoomBuilder
{
	#region ================== ErrorItem (mxd)

	public class ErrorItem
	{
		protected ErrorType type;
		protected string description;
		protected Image icon;
		
		public ErrorType Type { get { return type; } }
		public string Description { get { return description; } }
		public Image Icon { get { return icon; } }
		internal virtual bool IsShowable { get { return false; } }

		public ErrorItem(ErrorType type, string description)
		{
			this.type = type;
			this.description = description;
			this.icon = GetIcon();
		}

		protected virtual Image GetIcon()
		{
			return (type == ErrorType.Error ? Properties.Resources.ErrorLarge : Properties.Resources.WarningLarge);
		}

		internal virtual void ShowSource() { }
	}

	#endregion

	#region ================== MapElementErrorItem (mxd)

	public class MapElementErrorItem : ErrorItem
	{
		private MapElement target;
		private RectangleF zoomarea;
		private string targetmodename;

		internal override bool IsShowable { get { return true; } }

		public MapElementErrorItem(ErrorType type, MapElement target, string description) : base(type, description)
		{
			this.target = target;

			// Calculate zoom area
			List<Vector2D> points = new List<Vector2D>();
			RectangleF area = MapSet.CreateEmptyArea();

			// Add all points to a list
			if(target is Vertex)
			{
				points.Add((target as Vertex).Position);
				targetmodename = "VerticesMode";
			}
			else if(target is Linedef)
			{
				points.Add((target as Linedef).Start.Position);
				points.Add((target as Linedef).End.Position);
				targetmodename = "LinedefsMode";
			}
			else if(target is Sidedef)
			{
				points.Add((target as Sidedef).Line.Start.Position);
				points.Add((target as Sidedef).Line.End.Position);
				targetmodename = "LinedefsMode";
			}
			else if(target is Sector)
			{
				Sector s = (target as Sector);
				foreach(Sidedef sd in s.Sidedefs)
				{
					points.Add(sd.Line.Start.Position);
					points.Add(sd.Line.End.Position);
				}
				targetmodename = "SectorsMode";
			}
			else if(target is Thing)
			{
				Thing t = (target as Thing);
				Vector2D p = t.Position;
				points.Add(p);
				points.Add(p + new Vector2D(t.Size * 2.0f, t.Size * 2.0f));
				points.Add(p + new Vector2D(t.Size * 2.0f, -t.Size * 2.0f));
				points.Add(p + new Vector2D(-t.Size * 2.0f, t.Size * 2.0f));
				points.Add(p + new Vector2D(-t.Size * 2.0f, -t.Size * 2.0f));
				targetmodename = "ThingsMode";
			}
			else
			{
				General.Fail("Unknown object given to zoom in on.");
			}

			// Make a view area from the points
			foreach(Vector2D p in points)
				area = MapSet.IncreaseArea(area, p);

			// Make the area square, using the largest side
			if(area.Width > area.Height)
			{
				float delta = area.Width - area.Height;
				area.Y -= delta * 0.5f;
				area.Height += delta;
			}
			else
			{
				float delta = area.Height - area.Width;
				area.X -= delta * 0.5f;
				area.Width += delta;
			}

			// Add padding
			area.Inflate(100f, 100f);

			// Store
			this.zoomarea = area;
		}

		internal override void ShowSource()
		{
			// Switch to appropriate mode...
			if(General.Editing.Mode.GetType().Name != targetmodename)
			{
				// Leave any volatile mode
				General.Editing.CancelVolatileMode();
				General.Editing.ChangeMode(targetmodename);
			}

			// Select map element if it still exists
			if(target != null && !target.IsDisposed)
			{
				General.Map.Map.ClearAllSelected();
				
				if(target is Vertex)
				{
					((Vertex)target).Selected = true;
				}
				else if(target is Sidedef)
				{
					((Sidedef)target).Line.Selected = true;
				}
				else if(target is Linedef)
				{
					((Linedef)target).Selected = true;
				}
				else if(target is Sector)
				{
					Sector s = (Sector)target;
					((ClassicMode)General.Editing.Mode).SelectMapElement(s);
					foreach(Sidedef sd in s.Sidedefs) sd.Line.Selected = true;
				}
				else if(target is Thing)
				{
					((Thing)target).Selected = true;
				}
				else
				{
					throw new NotImplementedException("Unknown MapElement type!");
				}
			}

			// Show area
			ClassicMode editmode = (General.Editing.Mode as ClassicMode);
			editmode.CenterOnArea(zoomarea, 0.6f);
		}

		protected override Image GetIcon()
		{
			return (type == ErrorType.Error ? Properties.Resources.ErrorLargeMapObject : Properties.Resources.WarningLargeMapObject);
		}
	}

	#endregion 

	#region ================== TextResourceErrorItem (mxd)

	public class TextResourceErrorItem : ErrorItem
	{
		private string resourcelocation;
		private ScriptType scripttype;
		private string lumpname;
		private int lumpindex;
		private int linenumber;

		public string ResourceLocation { get { return resourcelocation; } }
		public ScriptType ScriptType { get { return scripttype; } }
		public string LumpName { get { return lumpname; } }
		public int LumpIndex { get { return lumpindex; } }
		public int LineNumber { get { return linenumber; } }

		internal override bool IsShowable { get { return true; } }

		internal TextResourceErrorItem(ErrorType type, ScriptType scripttype, DataLocation resourcelocation, string lumpname, int lumpindex, int linenumber, string description)
			: base(type, description)
		{
			this.resourcelocation = resourcelocation.location;
			this.scripttype = scripttype;
			this.lumpname = lumpname;
			this.lumpindex = lumpindex;
			this.linenumber = linenumber;
		}

		internal override void ShowSource()
		{
			// Only when a map is loaded
			if(General.Map == null || General.Map.Data == null) return;

			// Show Script Editor
			General.Map.ShowScriptEditor();

			// Show in ScriptEditor
			General.Map.ScriptEditor.DisplayError(this);
		}

		protected override Image GetIcon()
		{
			return (type == ErrorType.Error ? Properties.Resources.ErrorLargeText : Properties.Resources.WarningLargeText);
		}
	}
		

	#endregion

	#region ================== TextFileErrorItem (mxd)

	/*public class TextFileErrorItem : ErrorItem
	{
		private string filepathname;
		private ScriptType scripttype;
		private int linenumber;

		public string Filename { get { return filepathname; } }
		public ScriptType ScriptType { get { return scripttype; } }
		public int LineNumber { get { return linenumber; } }

		internal override bool IsShowable { get { return true; } }

		public TextFileErrorItem(ErrorType type, ScriptType scripttype, string filepathname, int linenumber, string description)
			: base(type, description)
		{
			this.filepathname = filepathname;
			this.scripttype = scripttype;
			this.linenumber = linenumber;
		}

		internal override void ShowSource()
		{
			// Only when a map is loaded
			if(General.Map == null || General.Map.Data == null) return;

			// Show Script Editor
			General.Map.ShowScriptEditor();

			// Show in ScriptEditor
			General.Map.ScriptEditor.DisplayError(this);
		}

		protected override Image GetIcon()
		{
			return (type == ErrorType.Error ? Properties.Resources.ErrorLargeText : Properties.Resources.WarningLargeText);
		}
	}*/

	#endregion

	#region ================== ErrorType

	public enum ErrorType
	{
		/// <summary>
		/// This indicates a significant error that may cause problems for the data displayed or editing behaviour.
		/// </summary>
		Error,
		
		/// <summary>
		/// This indicates a potential problem.
		/// </summary>
		Warning
	}

	#endregion
}
