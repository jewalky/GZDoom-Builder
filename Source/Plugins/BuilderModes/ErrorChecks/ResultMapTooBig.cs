#region ================== Namespaces

using System.Drawing;
using CodeImp.DoomBuilder.Geometry;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes.ErrorChecks
{
	public class ResultMapTooBig : ErrorResult
	{
		#region ================== Variables

		private readonly bool toowide;
		private readonly bool toohigh;
		private readonly Vector2D min;
		private readonly Vector2D max;

		#endregion
		
		#region ================== Properties

		public override int Buttons { get { return 0; } }

		#endregion

		#region ================== Constructor / Destructor

		public ResultMapTooBig(Vector2D min, Vector2D max) 
		{
			// Initialize
			this.min = min;
			this.max = max;
			this.toowide = max.x - min.x > General.Map.Config.SafeBoundary;
			this.toohigh = max.y - min.y > General.Map.Config.SafeBoundary;
			description = "Map is too big.";
		}

		#endregion

		#region ================== Methods

		public override RectangleF GetZoomArea()
		{
			const float scaler = 0.5f;
			return new RectangleF(min.x * scaler, min.y * scaler, (max.x - min.x) * scaler, (max.y - min.y) * scaler);
		}

		// This sets if this result is displayed in ErrorCheckForm (mxd)
		internal override void Hide(bool hide)
		{
			hidden = hide;
		}

		// This must return the string that is displayed in the listbox
		public override string ToString()
		{
			if(toowide && toohigh) return "Map's width and height is bigger than " + General.Map.Config.SafeBoundary + " m.u. This can cause rendering and physics issues.";
			if(toowide) return "Map is wider than " + General.Map.Config.SafeBoundary + " m.u. This can cause rendering and physics issues.";
			return "Map is taller than " + General.Map.Config.SafeBoundary + " m.u. This can cause rendering and physics issues.";
		}

		#endregion
	}
}
