using CodeImp.DoomBuilder.Map;
using System.Threading;

namespace CodeImp.DoomBuilder.BuilderModes
{
    [ErrorChecker("Check off-grid vertices", true, 50)]
    public class CheckOffGridVertices : ErrorChecker
    {
        private const int PROGRESS_STEP = 1000;

        // Constructor
        public CheckOffGridVertices()
        {
            // Total progress is done when all vertices are checked
            SetTotalProgress(General.Map.Map.Vertices.Count / PROGRESS_STEP);
        }

        // This runs the check
        public override void Run()
        {
            int progress = 0;
            int stepprogress = 0;

            // Go for all vertices
            foreach (Vertex v in General.Map.Map.Vertices)
            {
                if (v.Position.x != (int)v.Position.x || v.Position.y != (int)v.Position.y)
                {
                    SubmitResult(new ResultOffGridVertex(v));
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
