#region ================== Namespaces

using System;
using System.Threading;
using CodeImp.DoomBuilder.GZBuilder;
using CodeImp.DoomBuilder.Map;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	[ErrorChecker("Check unknown ACS scripts", true, 50)]
	public class CheckUnknownScripts : ErrorChecker
	{
		#region ================== Constants

		private const int PROGRESS_STEP = 1000;

		#endregion

		#region ================== Properties

		// Only possible in Hexen/UDMF map formats
		public override bool SkipCheck { get { return (!General.Map.UDMF && !General.Map.HEXEN); } }

		#endregion

		#region ================== Constructor / Destructor

		public CheckUnknownScripts()	
		{
			// Total progress is done when all things are checked
			SetTotalProgress((General.Map.Map.Things.Count + General.Map.Map.Linedefs.Count) / PROGRESS_STEP);
		}

		#endregion

		#region ================== Methods

		// This runs the check
		public override void Run() 
		{
			int progress = 0;
			int stepprogress = 0;

			// Go for all linedefs
			foreach(Linedef l in General.Map.Map.Linedefs)
			{
				bool isacsscript = (Array.IndexOf(GZGeneral.ACS_SPECIALS, l.Action) != -1);
				bool isnamedacsscript = (isacsscript && General.Map.UDMF && l.Fields.ContainsKey("arg0str"));

				if(isnamedacsscript)
				{
					string scriptname = l.Fields.GetValue("arg0str", string.Empty);
					if(!General.Map.ScriptNameExists(scriptname))
						SubmitResult(new ResultUnknownLinedefScript(l, true));
				}
				else if(isacsscript && !General.Map.ScriptNumberExists(l.Args[0]))
				{
					SubmitResult(new ResultUnknownLinedefScript(l, false));
				}

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

			// Go for all things
			foreach(Thing t in General.Map.Map.Things) 
			{
				bool isacsscript = (Array.IndexOf(GZGeneral.ACS_SPECIALS, t.Action) != -1);
				bool isnamedacsscript = (isacsscript && General.Map.UDMF && t.Fields.ContainsKey("arg0str"));

				if(isnamedacsscript)
				{
					string scriptname = t.Fields.GetValue("arg0str", string.Empty);
					if(!General.Map.ScriptNameExists(scriptname))
						SubmitResult(new ResultUnknownThingScript(t, true));
				}
				else if(isacsscript && !General.Map.ScriptNumberExists(t.Args[0]))
				{
					SubmitResult(new ResultUnknownThingScript(t, false));
				}

				// Handle thread interruption
				try { Thread.Sleep(0); } catch(ThreadInterruptedException) { return; }

				// We are making progress!
				if((++progress / PROGRESS_STEP) > stepprogress) 
				{
					stepprogress = (progress / PROGRESS_STEP);
					AddProgress(1);
				}
			}
		}

		#endregion
	}
}
