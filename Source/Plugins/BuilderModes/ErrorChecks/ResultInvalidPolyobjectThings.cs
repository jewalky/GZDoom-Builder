#region ================== Namespaces

using System;
using System.Collections.Generic;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	public class ResultInvalidPolyobjectThings : ErrorResult
	{
		#region ================== Variables

		private readonly List<Thing> things;
		private readonly string thingsinfo;

		#endregion

		#region ================== Properties

		public override int Buttons { get { return 0; } }

		#endregion

		#region ================== Constructor / Destructor

		public ResultInvalidPolyobjectThings(List<Thing> things, string details) 
		{
			// Initialize
			this.things = things;
			this.hidden = true;
			foreach(Thing t in things)
			{
				this.viewobjects.Add(t);
				this.hidden &= t.IgnoredErrorChecks.Contains(this.GetType());
			}

			if(things.Count == 1)
			{
				thingsinfo = "Incorrect Polyobject setup for thing " + things[0].Index;
			}
			else
			{
				thingsinfo = "Incorrect Polyobject setup for things " + things[0].Index;
				for(int i = 1; i < things.Count - 1; i++) thingsinfo += ", " + things[i].Index;
				thingsinfo += " and " + things[things.Count - 1].Index;
			}

			this.description = thingsinfo + ": " + details;
		}

		#endregion

		#region ================== Methods

		// This sets if this result is displayed in ErrorCheckForm (mxd)
		internal override void Hide(bool hide) 
		{
			hidden = hide;
			Type t = this.GetType();
			if(hide)
			{
				foreach(Thing thing in things) thing.IgnoredErrorChecks.Add(t);
			}
			else
			{
				foreach(Thing thing in things)
					if(thing.IgnoredErrorChecks.Contains(t)) thing.IgnoredErrorChecks.Remove(t);
			}
		}

		// This must return the string that is displayed in the listbox
		public override string ToString()
		{
			return thingsinfo;
		}

		// Rendering
		public override void RenderOverlaySelection(IRenderer2D renderer) 
		{
			foreach(Thing thing in things)
				renderer.RenderThing(thing, General.Colors.Selection, General.Settings.ActiveThingsAlpha);
		}

		#endregion
	}
}
