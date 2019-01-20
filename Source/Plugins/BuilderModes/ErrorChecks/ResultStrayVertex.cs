#region ================== Namespaces

using System;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;
using System.Drawing;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	public class ResultStrayVertex : ErrorResult
	{
		#region ================== Variables

		private readonly Vertex vertex;

		#endregion

		#region ================== Properties

		public override int Buttons { get { return 1; } }
		public override string Button1Text { get { return "Delete Vertex"; } }

		#endregion

		#region ================== Constructor / Destructor

		public ResultStrayVertex(Vertex v) 
		{
			// Initialize
			vertex = v;
			viewobjects.Add(v);
			hidden = v.IgnoredErrorChecks.Contains(this.GetType()); //mxd
			description = "This vertex is not connected to any linedef.";
		}

		#endregion

		#region ================== Methods

		// This sets if this result is displayed in ErrorCheckForm (mxd)
		internal override void Hide(bool hide) 
		{
			hidden = hide;
			Type t = this.GetType();
			if(hide) vertex.IgnoredErrorChecks.Add(t);
			else if(vertex.IgnoredErrorChecks.Contains(t)) vertex.IgnoredErrorChecks.Remove(t);
		}

		// This must return the string that is displayed in the listbox
		public override string ToString()
		{
			return "Vertex " + vertex.Index + " at " + vertex.Position.x + ", " + vertex.Position.y + " is not connected to any linedef.";
		}

		// Rendering
		public override void RenderOverlaySelection(IRenderer2D renderer)
		{
			renderer.RenderRectangleFilled(new RectangleF(vertex.Position.x - 3, vertex.Position.y - 3, 6f, 6f), General.Colors.Selection, true);
		}

		// This removes the vertex
		public override bool Button1Click(bool batchMode)
		{
			if(!batchMode) General.Map.UndoRedo.CreateUndo("Delete vertex");
			vertex.Dispose();
			General.Map.IsChanged = true;
			General.Map.ThingsFilter.Update();
			return true;
		}

		#endregion
	}
}
