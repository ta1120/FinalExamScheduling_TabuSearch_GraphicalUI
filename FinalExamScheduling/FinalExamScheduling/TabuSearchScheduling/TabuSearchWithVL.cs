using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FinalExamScheduling.Model;

namespace FinalExamScheduling.TabuSearchScheduling
{
    class TabuSearchWithVL
    {
        private Context ctx;

        private TabuList tabuList;

        private SolutionCandidate currentSolution;

        private SolutionCandidate bestSolution;

        private int examCount;
        private bool isNextModeRandom = false;
        private bool isTandemSearch;
        private int generationCycleCounter;
        private int idleIterCounter;
        private int tandemIdleRunCounter;
        private int shuffleCounter;
        private double prevBestScore;
        private bool shuffleRequested;
        private List<SolutionCandidate> neighbouringSolutions;
        private CancellationToken currentCancallationToken;

        public TabuSearchWithVL(Context _ctx)
        {
            ctx = _ctx;
            examCount = ctx.Students.Length;
            isNextModeRandom = false;
            isTandemSearch = TSParameters.Mode.Equals("Tandem");
            generationCycleCounter = 0;
            idleIterCounter = 0;
            tandemIdleRunCounter = 0;
            shuffleCounter = 0;
            neighbouringSolutions = new List<SolutionCandidate>();
            shuffleRequested = false;   
        }

        public SolutionCandidate Start(List<double> iterationalProgress, CancellationToken cancellationToken)
        {
            currentCancallationToken = cancellationToken;

            CandidateCostCalculator costCalculator = new CandidateCostCalculator(ctx);

            SolutionCandidate initialSolution = EvaluateSolution(RandomInitialSolutionGenerator.GenerateInitialSolution(ctx));

            //Console.WriteLine("##### Initial solution generated\n");

            currentSolution = initialSolution.Clone();
            bestSolution = initialSolution.Clone();
            tabuList = new TabuList();
            prevBestScore = currentSolution.score;

            SolutionCandidate selectedNeighbour = null;

            while (GetGlobalTerminationCriteriaState())
            {
                currentSolution.vl = new ViolationList().Evaluate(currentSolution);
                //Console.WriteLine("Current solution violations: " + currentSolution.VL.Violations.Count +  "\n");

                do
                {
                    //This is a kind of relaxing added to the existing algorithm
                    if(generationCycleCounter > TSParameters.MaxFailedNeighbourGenerations)
                    {
                        tabuList.DecreaseIterationsLeft();
                        generationCycleCounter = 0;
                    }

                    if (neighbouringSolutions.Count < 2)
                    {
                        //Console.WriteLine("### Neighbour generation started " + generationCycleCounter);
                        neighbouringSolutions = GenerateNeighbours(currentSolution);
                        generationCycleCounter++;
                    }

                    else neighbouringSolutions.Remove(selectedNeighbour);


                    selectedNeighbour = EvaluateSolution(SelectBestCandidate());

                    if (bestSolution.score > selectedNeighbour.score)
                    {
                        bestSolution = EvaluateSolution(selectedNeighbour.Clone());
                        //Console.WriteLine(IsFeasibleSolution(selectedNeighbour) ? "Acceptable (BS)" : "Aspiration criteria met");
                        break;
                    }
                    //Console.WriteLine(IsFeasibleSolution(selectedNeighbour) ? "Acceptable" : "Tabu " + neighbouringSolutions.Count);
                }
                while (!IsFeasibleSolution(selectedNeighbour) && GetGlobalTerminationCriteriaState());

                //Console.WriteLine("Score: " + selectedNeighbour.score);

                SolutionCandidate prevSolution = currentSolution;
                currentSolution = selectedNeighbour;

                IterationCountHandler(prevBestScore > bestSolution.score);
                prevBestScore = bestSolution.score;

                neighbouringSolutions.Clear();

                tabuList.DecreaseIterationsLeft();

                ExpandTabuList(prevSolution, currentSolution);

                if (TSParameters.LogIterationalProgress) iterationalProgress.Add(currentSolution.score);

                //ha üres a legjobb megoldás VL listája, akkor a jelenleg implementált követelmények maximálisan optimalizálva lettek
                if (bestSolution.vl.violations.Count == 0)
                {
                    return bestSolution;
                }

                Console.WriteLine("Current best score: " + bestSolution.score);

                //When stuck and shuffling is activated, shuffle the students in the current solution
                if(shuffleRequested && TSParameters.AllowShuffleWhenStuck && TSParameters.MaxShuffles > shuffleCounter)
                {
                    Console.WriteLine("Shuffling... " + shuffleCounter);
                    currentSolution =  ShuffleStudents(currentSolution);
                }

            }
            return EvaluateSolution(bestSolution);
        }

        public void ManageTandemSwitches()
        {
            if(idleIterCounter >= TSParameters.AllowedIdleIterations)
            {
                isNextModeRandom = !isNextModeRandom;
                idleIterCounter = 0;

                tandemIdleRunCounter++;

                if (tandemIdleRunCounter >= TSParameters.TandemIdleSwitches) shuffleRequested = true;            }
        }

        public SolutionCandidate EvaluateSolution(SolutionCandidate solution)
        {
            CandidateCostCalculator calc = new CandidateCostCalculator(ctx);
            solution.score = calc.Evaluate(solution);
            solution.vl = new ViolationList().Evaluate(solution);
            return solution;
        }

        public void IterationCountHandler(bool improvement)
        {
            if (improvement)
            {
                Console.WriteLine("#IMPROVEMENT");
                idleIterCounter = 0;
                
                if(isTandemSearch) tandemIdleRunCounter = 0;
            }
            else
            {
                Console.WriteLine("#NOBETTER " + idleIterCounter + ", " + tandemIdleRunCounter + ", " + generationCycleCounter);
                idleIterCounter++;
                if (isTandemSearch) ManageTandemSwitches();
                else
                {
                    if(idleIterCounter >= TSParameters.AllowedIdleIterations) shuffleRequested = true;
                }
            }
            generationCycleCounter = 0;
        }

        //REVIEW
        public bool GetGlobalTerminationCriteriaState() //Termination will be initiated when this returns false
        {
            return
            (isTandemSearch || idleIterCounter < TSParameters.AllowedIdleIterations)
            &&
            (!isTandemSearch || tandemIdleRunCounter < TSParameters.TandemIdleSwitches)
            &&
            (!TSParameters.AllowShuffleWhenStuck || shuffleCounter < TSParameters.MaxShuffles)
            &&
            (bestSolution.score > TSParameters.TargetScore)
            &&
            (!currentCancallationToken.IsCancellationRequested) //Checking whether cancellation is requested from the GUI
            ;
        }

        public List<SolutionCandidate> GenerateNeighbours(SolutionCandidate current)
        {
            if (isTandemSearch)
            {
                if(isNextModeRandom)
                {
                    isNextModeRandom = false;
                    return GetNeighbours(current, "Random");
                }
                else
                {
                    isNextModeRandom= true;
                    return GetNeighbours(current, "Heuristic");
                }
            }
            else return GetNeighbours(current, TSParameters.Mode);
        }

        public List<SolutionCandidate> GetNeighbours(SolutionCandidate current, string mode)
        {
            NeighbourGeneratorWithVL neighbourGenerator = new NeighbourGeneratorWithVL(ctx);
            SolutionCandidate currentSolution = current.Clone();
            SolutionCandidate[] neighbours = new SolutionCandidate[TSParameters.GeneratedCandidates];

            neighbours = neighbourGenerator.GenerateNeighbours(currentSolution, mode);

            return neighbours.ToList<SolutionCandidate>();
        }

        /*################################################################################*/
        /*Rewrite from here*/

        //Exchange a given percentage of students with other ones, to possibly unstuck
        public SolutionCandidate ShuffleStudents(SolutionCandidate current)
        {
            //Resetting variables first
            shuffleRequested = false;
            idleIterCounter = 0;
            if (isTandemSearch) tandemIdleRunCounter = 0;
            shuffleCounter++;

            SolutionCandidate shuffled = current.Clone();
            int shuffleCount = current.schedule.FinalExams.Length * (TSParameters.ShufflePercentage / 100);
            Random rand = new Random();
            for (int i = 0; i < shuffleCount; i++)
            {
                int x = rand.Next(0, ctx.Students.Length);
                int y = rand.Next(0, ctx.Students.Length);
                while (y == x) x = rand.Next(0, ctx.Students.Length);

                Student temp = shuffled.schedule.FinalExams[y].Student;

                shuffled.schedule.FinalExams[y].Student = shuffled.schedule.FinalExams[x].Student;
                shuffled.schedule.FinalExams[x].Student = temp;
                shuffled.schedule.FinalExams[y].Supervisor = shuffled.schedule.FinalExams[y].Student.Supervisor;
                shuffled.schedule.FinalExams[x].Supervisor = shuffled.schedule.FinalExams[x].Student.Supervisor;
            }
            return shuffled;
        }

        //Selects the best solution from neighbours
        public SolutionCandidate SelectBestCandidate()
        {
            CandidateCostCalculator calc = new CandidateCostCalculator(ctx);
            SolutionCandidate best = null;

            //Console.WriteLine(neighbouringSolutions.Count == 0 ? "Elfogytak az s":"Van meg s " + neighbouringSolutions.Count );

            foreach (SolutionCandidate candidate in neighbouringSolutions)
            {
                candidate.score = calc.Evaluate(candidate);

                if(best == null) best = candidate;
                else
                {
                    if(best.score > candidate.score) best = candidate;
                }
            }
            
            return best;
        }

        //Check whether a solution contains tabu elements/moves
        public bool IsFeasibleSolution(SolutionCandidate solution)
        {
            FinalExam[] exams = solution.schedule.FinalExams;
            
            foreach (TabuListElement tabu in tabuList.GetTabuList())
            {
                if (tabu.TabuIterationsLeft > 0)
                {
                    if (tabu.Exam.IsEqualExam(exams[tabu.ExamSlot])) return false;
                }
            }
            return true;
        }

        //Add new elements to the tabu list based on changed attributes in schedules
        public void ExpandTabuList(SolutionCandidate current, SolutionCandidate bestNeighbour)
        {
            int examCount = ctx.Students.Length;
            FinalExam[] oldExams = current.schedule.FinalExams;
            FinalExam[] newExams = bestNeighbour.schedule.FinalExams;

            for (int i = 0; i < examCount; i++)
            {
                if (!oldExams[i].IsEqualExam(newExams[i]))
                {
                    tabuList.Add(new TabuListElement(oldExams[i].Clone(),i));
                }
            }
        }
    }
}
