#region ================== Namespaces

using System;
using System.Collections.Generic;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Map;
using System.Threading;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	[ErrorChecker("Check polyobjects", true, 100)]
	public class CheckPolyobjects : ErrorChecker
	{
		#region ================== Constants

		private const int PROGRESS_STEP = 1000;

		#endregion

		#region ================== Properties

		// Only possible in Hexen/UDMF map formats
		public override bool SkipCheck { get { return (!General.Map.UDMF && !General.Map.HEXEN); } }

		#endregion

		#region ================== Constructor / Destructor

		public CheckPolyobjects() 
		{
			// Total progress is somewhat done when all linedefs and things are checked
			SetTotalProgress((General.Map.Map.Linedefs.Count + General.Map.Map.Things.Count) / PROGRESS_STEP);
		}

		#endregion

		#region ================== Methods

		// This runs the check
		public override void Run() 
		{
			int progress = 0;
			int stepprogress = 0;
			const string Polyobj_StartLine = "Polyobj_StartLine";
			const string Polyobj_ExplicitLine = "Polyobj_ExplicitLine";

			// <Polyobj_Action, <Polyobj_number, Lines using this number>>
			Dictionary<string, Dictionary<int, List<Linedef>>> polyobjlines = new Dictionary<string, Dictionary<int, List<Linedef>>>();

			// All polyobject-related actions
			HashSet<string> allactions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				Polyobj_StartLine, Polyobj_ExplicitLine, 
				"Polyobj_RotateLeft", "Polyobj_RotateRight", 
				"Polyobj_Move", "Polyobj_MoveTimes8", 
				"Polyobj_DoorSwing", "Polyobj_DoorSlide", 
				"Polyobj_OR_MoveToSpot", "Polyobj_MoveToSpot", 
				"Polyobj_Stop", "Polyobj_MoveTo", 
				"Polyobj_OR_MoveTo", "Polyobj_OR_RotateLeft", 
				"Polyobj_OR_RotateRight", "Polyobj_OR_Move", 
				"Polyobj_OR_MoveTimes8",
			};

			Dictionary<int, List<Thing>> anchors = new Dictionary<int, List<Thing>>();
			Dictionary<int, List<Thing>> startspots = new Dictionary<int, List<Thing>>();

			// Collect Linedefs...
			foreach(Linedef l in General.Map.Map.Linedefs)
			{
				if(l.Action > 0 && General.Map.Config.LinedefActions.ContainsKey(l.Action) && allactions.Contains(General.Map.Config.LinedefActions[l.Action].Id))
				{
					string id = General.Map.Config.LinedefActions[l.Action].Id;
					
					if(!polyobjlines.ContainsKey(id))
						polyobjlines.Add(id, new Dictionary<int, List<Linedef>>());

					// Polyobj number is always the first arg
					if(!polyobjlines[id].ContainsKey(l.Args[0]))
						polyobjlines[id].Add(l.Args[0], new List<Linedef>());

					polyobjlines[id][l.Args[0]].Add(l);
				}

				UpdateProgress(ref progress, ref stepprogress);
			}

			// Collect Things...
			foreach(Thing t in General.Map.Map.Things)
			{
				ThingTypeInfo info = General.Map.Data.GetThingInfoEx(t.Type);
				if(info == null) continue;
				switch(info.ClassName.ToLowerInvariant())
				{
					case "$polyanchor":
						if(!anchors.ContainsKey(t.AngleDoom)) anchors.Add(t.AngleDoom, new List<Thing>());
						anchors[t.AngleDoom].Add(t);
						break;

					case "$polyspawn":
					case "$polyspawncrush":
					case "$polyspawnhurt":
						if(!startspots.ContainsKey(t.AngleDoom)) startspots.Add(t.AngleDoom, new List<Thing>());
						startspots[t.AngleDoom].Add(t);
						break;
				}

				UpdateProgress(ref progress, ref stepprogress);
			}

			// Check Linedefs. These can connect 1 - multiple (except Polyobj_StartLine)
			// Polyobject number is always arg0.
			foreach(KeyValuePair<string, Dictionary<int, List<Linedef>>> group in polyobjlines)
			{
				foreach(KeyValuePair<int, List<Linedef>> linesbytype in group.Value)
				{
					if(!startspots.ContainsKey(linesbytype.Key))
						SubmitResult(new ResultInvalidPolyobjectLines(linesbytype.Value, "\"" + group.Key + "\" action targets non-existing Polyobject Start Spot (" + linesbytype.Key + ")"));
				}
			}

			// Check Linedefs with Polyobj_StartLine action. These must connect 1 - 1.
			// Polyobject number is arg0, Mirror polyobject number is arg1
			if(polyobjlines.ContainsKey(Polyobj_StartLine))
			{
				foreach(KeyValuePair<int, List<Linedef>> linesbytype in polyobjlines[Polyobj_StartLine])
				{
					// Should be only one Polyobj_StartLine per Polyobject number
					if(linesbytype.Value.Count > 1)
						SubmitResult(new ResultInvalidPolyobjectLines(linesbytype.Value, "Several \"" + Polyobj_StartLine + "\" actions have the same Polyobject Number assigned (" + linesbytype.Key + "). They won't function correctly ingame."));

					// Check if Mirror Polyobject Number exists
					foreach(Linedef linedef in linesbytype.Value)
					{
						// The value of 0 can mean either "No mirror polyobj" or "Polyobj 0" here...
						if(linedef.Args[1] > 0)
						{
							if(!startspots.ContainsKey(linedef.Args[1]))
								SubmitResult(new ResultInvalidPolyobjectLines(new List<Linedef> { linedef }, "\"" + Polyobj_StartLine + "\" action have non-existing Mirror Polyobject Number assigned (" + linedef.Args[1] + "). It won't function correctly ingame."));
							if(linedef.Args[1] == linedef.Args[0])
								SubmitResult(new ResultInvalidPolyobjectLines(new List<Linedef> { linedef }, "\"" + Polyobj_StartLine + "\" action have the same Polyobject and Mirror Polyobject numbers assigned (" + linedef.Args[1] + "). It won't function correctly ingame."));
						}
					}
				}
			}

			// Check Linedefs with Polyobj_ExplicitLine action. These can connect 1 - multiple.
			// Polyobject number is arg0, Mirror polyobject number is arg2
			if(polyobjlines.ContainsKey(Polyobj_ExplicitLine))
			{
				foreach(KeyValuePair<int, List<Linedef>> linesbytype in polyobjlines[Polyobj_ExplicitLine])
				{
					// Check if Mirror Polyobject Number exists
					foreach(Linedef linedef in linesbytype.Value)
					{
						// The value of 0 can mean either "No mirror polyobj" or "Polyobj 0" here...
						if(linedef.Args[2] > 0)
						{
							if(!startspots.ContainsKey(linedef.Args[2]))
								SubmitResult(new ResultInvalidPolyobjectLines(new List<Linedef> { linedef }, "\"" + Polyobj_StartLine + "\" action have non-existing Mirror Polyobject Number assigned (" + linedef.Args[2] + "). It won't function correctly ingame."));
							if(linedef.Args[2] == linedef.Args[0])
								SubmitResult(new ResultInvalidPolyobjectLines(new List<Linedef> { linedef }, "\"" + Polyobj_StartLine + "\" action have the same Polyobject and Mirror Polyobject numbers assigned (" + linedef.Args[2] + "). It won't function correctly ingame."));
						}
					}
				}
			}

			// Check Polyobject Anchors. These must connect 1 - 1.
			foreach(KeyValuePair<int, List<Thing>> group in anchors)
			{
				if(!startspots.ContainsKey(group.Key))
					SubmitResult(new ResultInvalidPolyobjectThings(group.Value, "Polyobject " + (group.Value.Count > 1 ? "Anchors target" : "Anchor targets") + " non-existing Polyobject Start Spot (" + group.Key + ")"));

				if(group.Value.Count > 1)
					SubmitResult(new ResultInvalidPolyobjectThings(group.Value, "Several Polyobject Anchors target the same Polyobject Start Spot (" + group.Key + "). They won't function correctly ingame."));
			}

			// Check Polyobject Start Spots. These must connect 1 - 1.
			foreach(KeyValuePair<int, List<Thing>> group in startspots)
			{
				if(!anchors.ContainsKey(group.Key))
					SubmitResult(new ResultInvalidPolyobjectThings(group.Value, "Polyobject Start " + (group.Value.Count > 1 ? "Spots are not targeted" : "Spot " + group.Key + " is not targeted") + " by any Polyobject Anchor"));

				if(group.Value.Count > 1)
					SubmitResult(new ResultInvalidPolyobjectThings(group.Value, "Several Polyobject Start Spots have the same Polyobject number (" + group.Key + "). They won't function correctly ingame."));
			}
		}

		private void UpdateProgress(ref int progress, ref int stepprogress)
		{
			// Handle thread interruption
			try { Thread.Sleep(0); }
			catch(ThreadInterruptedException) { return; }

			// We are making progress!
			if((++progress / PROGRESS_STEP) > stepprogress)
			{
				stepprogress = (progress / PROGRESS_STEP);
				AddProgress(1);
			}
		}

		#endregion
	}
}
