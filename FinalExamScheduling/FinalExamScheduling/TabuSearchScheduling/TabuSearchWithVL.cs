using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinalExamScheduling.Model;

namespace FinalExamScheduling.TabuSearchScheduling
{
    class TabuSearchWithVL
    {
        public Context ctx;

        public TabuList tabuList;

        SolutionCandidate currentSolution;

        SolutionCandidate bestSolution;

        int examCount;
        bool NextModeIsRandom = false;
        bool TandemSearch;
        int generationCycleCounter;
        int idleIterCounter;
        int tandemIdleRunCounter;
        int shuffleCounter;
        double prevBestScore;
        List<SolutionCandidate> neighbouringSolutions; 

        public TabuSearchWithVL(Context _ctx)
        {
            ctx = _ctx;
            examCount = ctx.Students.Length;
            NextModeIsRandom = false;
            TandemSearch = TSParameters.Mode.Equals("Tandem");
            generationCycleCounter = 0;
            idleIterCounter = 0;
            tandemIdleRunCounter = 0;
            shuffleCounter = 0;
            neighbouringSolutions = new List<SolutionCandidate>();
        }

        public SolutionCandidate Start(List<double> iterationalProgress)
        {
            CandidateCostCalculator costCalculator = new CandidateCostCalculator(ctx);

            SolutionCandidate initialSolution = returnEvaluatedSolution(new RandomInitialSolutionGenerator(ctx).generateInitialSolution());
            Console.WriteLine("##### Initial solution generated\n");

            currentSolution = initialSolution.Clone();
            bestSolution = initialSolution.Clone();
            tabuList = new TabuList();
            prevBestScore = currentSolution.Score;

            SolutionCandidate selectedNeighbour = null;

            Console.WriteLine(GetGlobalTerminationCriteriaState() ? "Temination criteria not met\n" : "Termination criteria met\n");

            while (GetGlobalTerminationCriteriaState())
            {
                currentSolution.VL = new ViolationList().Evaluate(currentSolution);
                //Console.WriteLine("Current solution violations: " + currentSolution.VL.Violations.Count +  "\n");
                
                do
                {
                    if (neighbouringSolutions.Count < 2)
                    {
                        Console.WriteLine("### Neighbour generation started " + generationCycleCounter);
                        neighbouringSolutions = GenerateNeighbours(currentSolution);
                        generationCycleCounter++;
                    }

                    else neighbouringSolutions.Remove(selectedNeighbour);


                    selectedNeighbour = returnEvaluatedSolution(SelectBestCandidate());

                    if(bestSolution.Score > selectedNeighbour.Score)
                    {
                        bestSolution = returnEvaluatedSolution(selectedNeighbour.Clone());
                        Console.WriteLine(IsFeasibleSolution(selectedNeighbour) ? "Acceptable (BS)" : "Aspiration criteria met");
                        break;
                    }
                    Console.WriteLine(IsFeasibleSolution(selectedNeighbour) ? "Acceptable" : "Tabu " + neighbouringSolutions.Count);
                }
                while(!IsFeasibleSolution(selectedNeighbour) && GetGlobalTerminationCriteriaState());

                Console.WriteLine("Score: " + selectedNeighbour.Score) ;

                SolutionCandidate prevSolution = currentSolution;
                currentSolution = selectedNeighbour;

                IterationCountHandler(prevBestScore > bestSolution.Score);
                prevBestScore = bestSolution.Score;

                neighbouringSolutions.Clear();

                tabuList.DecreaseIterationsLeft();

                ExpandTabuList(prevSolution, currentSolution);

                if (TSParameters.LogIterationalProgress) iterationalProgress.Add(currentSolution.Score);

                //ha üres a legjobb megoldás VL listája, akkor a jelenleg implementált követelmények maximálisan optimalizálva lettek
                if (bestSolution.VL.Violations.Count == 0)
                {
                    return bestSolution;
                }

                //Console.WriteLine(bestSolution.Score);
                
            }
            return bestSolution;
        }

        public SolutionCandidate returnEvaluatedSolution(SolutionCandidate solution)
        {
            CandidateCostCalculator calc = new CandidateCostCalculator(ctx);
            solution.Score = calc.Evaluate(solution);
            solution.VL = new ViolationList().Evaluate(solution);
            return solution;
        }

        public void IterationCountHandler(bool improvement)
        {
            generationCycleCounter = 0;

            if (improvement)
            {
                Console.WriteLine("#IMPROVEMENT");
                idleIterCounter = 0;
                if(TandemSearch) tandemIdleRunCounter = 0;
            }
            else
            {
                Console.WriteLine("#NOBETTER " + idleIterCounter + ", " + tandemIdleRunCounter);
                idleIterCounter++;
                if (TandemSearch) tandemIdleRunCounter++;
            }
        }

        //REVIEW
        public bool GetGlobalTerminationCriteriaState() //Termination will be initiated when this returns false
        {
            return
            (TandemSearch || idleIterCounter < TSParameters.AllowedIdleIterations)
            &&
            (!TandemSearch || tandemIdleRunCounter < TSParameters.TandemIdleSwitches)
            &&
            (!TSParameters.AllowShuffleWhenStuck || shuffleCounter < TSParameters.MaxShuffles)
            &&
            (generationCycleCounter < TSParameters.MaxFailedNeighbourGenerations)
            &&
            (bestSolution.Score > TSParameters.TargetScore)
            ;
        }

        public List<SolutionCandidate> GenerateNeighbours(SolutionCandidate current)
        {
            if (TandemSearch)
            {
                if(NextModeIsRandom)
                {
                    NextModeIsRandom = false;
                    return GetNeighbours(current, "Random");
                }
                else
                {
                    NextModeIsRandom= true;
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
            SolutionCandidate shuffled = current.Clone();
            int shuffleCount = current.Schedule.FinalExams.Length * (TSParameters.ShufflePercentage / 100);
            Random rand = new Random();
            for (int i = 0; i < shuffleCount; i++)
            {
                int x = rand.Next(0, ctx.Students.Length);
                int y = rand.Next(0, ctx.Students.Length);
                while (y == x) x = rand.Next(0, ctx.Students.Length);

                Student temp = shuffled.Schedule.FinalExams[y].Student;

                shuffled.Schedule.FinalExams[y].Student = shuffled.Schedule.FinalExams[x].Student;
                shuffled.Schedule.FinalExams[x].Student = temp;
                shuffled.Schedule.FinalExams[y].Supervisor = shuffled.Schedule.FinalExams[y].Student.Supervisor;
                shuffled.Schedule.FinalExams[x].Supervisor = shuffled.Schedule.FinalExams[x].Student.Supervisor;
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
                candidate.Score = calc.Evaluate(candidate);

                if(best == null) best = candidate;
                else
                {
                    if(best.Score > candidate.Score) best = candidate;
                }
            }
            
            return best;
        }

        //Check whether a solution contains tabu elements/moves
        public bool IsFeasibleSolution(SolutionCandidate solution)
        {
            FinalExam[] exams = solution.Schedule.FinalExams;
            
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
            FinalExam[] oldExams = current.Schedule.FinalExams;
            FinalExam[] newExams = bestNeighbour.Schedule.FinalExams;

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
