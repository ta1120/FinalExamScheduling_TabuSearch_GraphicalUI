using FinalExamScheduling.Model;
using System;
using System.Collections.Generic;

namespace FinalExamScheduling.TabuSearchScheduling
{
    class TabuSearchScheduler
    {
        public Context ctx;


        public TabuSearchScheduler(Context context)
        {
            ctx = context;
        }

        public SolutionCandidate Run(List<double> iterationProgress)
        {
            //Created for the ability of choosing which version of the algorithm to run, but only one version was implemented so far
            TabuSearchWithVL tabuSearchAlgorithm = new TabuSearchWithVL(ctx);

            SolutionCandidate best = tabuSearchAlgorithm.Start(iterationProgress);

            return best;
        }
    }
}
