using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Map;
using System.Collections.Generic;
using System.Threading;

namespace CodeImp.DoomBuilder.BuilderModes
{
    [ErrorChecker("Check missing activations", true, 50)]
    public class CheckMissingActivations : ErrorChecker
    {
        private const int PROGRESS_STEP = 1000;

        // Constructor
        public CheckMissingActivations()
        {
            // Total progress is done when all linedefs are checked
            SetTotalProgress(General.Map.Map.Linedefs.Count / PROGRESS_STEP);
        }

        public override bool SkipCheck { get { return !General.Map.UDMF; } }

        // This runs the check
        public override void Run()
        {
            int progress = 0;
            int stepprogress = 0;

            //If this map isn't a UDMF then we can't reach a situation where activations are missing
            if (!General.Map.UDMF)
            {
                return;
            }

            // Go for all vertices
            foreach (Linedef l in General.Map.Map.Linedefs)
            {
                int action = l.Action;
                Dictionary<string, bool> flags = l.GetFlags();
                bool hasActivation = false;
                if (action != 0
                    && General.Map.Config.LinedefActions.ContainsKey(action)
                    && General.Map.Config.LinedefActions[action].RequiresActivation)
                {
                    foreach (LinedefActivateInfo ai in General.Map.Config.LinedefActivates)
                    {
                        if (flags.ContainsKey(ai.Key) && flags[ai.Key] == true)
                        {
                            hasActivation = true;
                            break;
                        }
                    }

                    if (!hasActivation)
                    {
                        SubmitResult(new ResultMissingActivation(l));
                    }
                }

                // Handle thread interruption
                try { Thread.Sleep(0); } catch (ThreadInterruptedException) { return; }

                // We are making progress!
                if ((++progress / PROGRESS_STEP) > stepprogress)
                {
                    stepprogress = (progress / PROGRESS_STEP);
                    AddProgress(1);
                }
            }
        }
    }
}
