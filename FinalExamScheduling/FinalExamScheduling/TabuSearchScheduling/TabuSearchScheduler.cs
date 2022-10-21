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
            //Algorithm choice: later to be outsourced to class TSParameters
            TabuSearchWithVL tabuSearchAlgorithm = new TabuSearchWithVL(ctx);

            if (TSParameters.PrintDetails) Console.WriteLine("TabuSearch running...");

            SolutionCandidate best = tabuSearchAlgorithm.Start(iterationProgress);

            return best;
        }
    }
}
