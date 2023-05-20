using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FinalExamScheduling.Model;

namespace FinalExamScheduling.TabuSearchScheduling
{
    //This class contains the implementation of the core of the algorithm
    class TabuSearchWithVL
    {
        //Storing data from the input
        private Context ctx;

        //Storing a tabu list
        private TabuList tabuList;

        //Storing the current solution (s)
        private SolutionCandidate currentSolution;

        //Storing the best solution so far (bs)
        private SolutionCandidate bestSolution;

        //The algorithm has many control variables, these are explained in the documentation
        private bool isNextModeRandom = false;
        private bool isTandemSearch;
        private int generationCycleCounter;
        private int idleIterCounter;
        private int tandemIdleRunCounter;
        private int shuffleCounter;
        private double prevBestScore;
        private bool shuffleRequested;
        private List<SolutionCandidate> neighbouringSolutions;

        //The algorithm receives a cancellation token when started, the state of this is checked with the other termination criteria
        private CancellationToken currentCancallationToken;

        //Initialization
        public void InitializeAlgorithm()
        {
            isNextModeRandom = false;
            isTandemSearch = TSParameters.Mode.Equals("Tandem");
            generationCycleCounter = 0;
            idleIterCounter = 0;
            tandemIdleRunCounter = 0;
            shuffleCounter = 0;
            prevBestScore = 0;
            neighbouringSolutions = new List<SolutionCandidate>();
            shuffleRequested = false;
        }

        public TabuSearchWithVL(Context _ctx)
        {
            ctx = _ctx;
            InitializeAlgorithm();
        }

        //This method is implementing the algorithm itself. It follows the flowchart and the pseudocode in the documentation.
        public SolutionCandidate Start(List<double> iterationalProgress, CancellationToken cancellationToken)
        {
            currentCancallationToken = cancellationToken;

            CandidateCostCalculator costCalculator = new CandidateCostCalculator(ctx);

            //Generating initial solution based on the context
            SolutionCandidate initialSolution = EvaluateSolution(RandomInitialSolutionGenerator.GenerateInitialSolution(ctx));

            //Setup
            currentSolution = initialSolution.Clone();
            bestSolution = initialSolution.Clone();
            tabuList = new TabuList();
            prevBestScore = currentSolution.score;

            SolutionCandidate selectedNeighbour = null;

            //Running iterations until termination criteria is met
            while (GetGlobalTerminationCriteriaState())
            {
                //Evaluating the current solution
                currentSolution.vl = new ViolationList().Evaluate(currentSolution);

                //Generating neighbours, until a feasible candidate is found or termination is requested
                do
                {
                    //This is a kind of relaxing added to the existing algorithm. 
                    //When a certain number of iterations is reached without successfull generation, the current tabu elements lifecycle will be shortened (after some time they will be disposed of)
                    if(generationCycleCounter > TSParameters.MaxFailedNeighbourGenerations)
                    {
                        tabuList.DecreaseIterationsLeft();
                        generationCycleCounter = 0;
                    }

                    //Checking whether we have run out of possible candidates, if so, regenerating
                    if (neighbouringSolutions.Count < 2)
                    {
                        neighbouringSolutions = GenerateNeighbours(currentSolution);
                        generationCycleCounter++;
                    }

                    //Removing the previous neighbour (tabu neighbour) from the list
                    else neighbouringSolutions.Remove(selectedNeighbour);

                    //Selecting and evaluating the next best candidate
                    selectedNeighbour = EvaluateSolution(SelectBestCandidate());

                    //Checking the aspiration criteria: if the selected neighbour is better than the current best solution, it will be the new current and best solution
                    if (bestSolution.score > selectedNeighbour.score)
                    {
                        bestSolution = EvaluateSolution(selectedNeighbour.Clone());
                        break;
                    }
                }
                while (!IsFeasibleSolution(selectedNeighbour) && GetGlobalTerminationCriteriaState());

                //Storing the previous solution to expand the tabu list later
                SolutionCandidate prevSolution = currentSolution;

                //Updating the current solution with the best feasible neighbour (s = s')
                currentSolution = selectedNeighbour;

                //Handling the itaration counters
                IterationCountHandler(prevBestScore > bestSolution.score);
                prevBestScore = bestSolution.score;

                //Clearing the neighbours for the next iteration
                neighbouringSolutions.Clear();

                //Handling lifecycle for tabu elements 
                tabuList.DecreaseIterationsLeft();

                //Adding the new tabu steps to the list based on the diifferences of the 2 solutions
                ExpandTabuList(prevSolution, currentSolution);

                //Adding the current iterations score to the log
                if (TSParameters.LogIterationalProgress) iterationalProgress.Add(currentSolution.score);

                //Checking whether there are any violations left (otherwise the solution would be fully optimized, and the algorithm should quit)
                if (bestSolution.vl.violations.Count == 0)
                {
                    return bestSolution;
                }

                //When stuck and shuffling is activated, shuffle the students in the current solution
                if(shuffleRequested && TSParameters.AllowShuffleWhenStuck && TSParameters.MaxShuffles > shuffleCounter)
                {
                    currentSolution =  ShuffleStudents(currentSolution);
                }

            }

            //Returning the evaluated version of the best found solution
            return EvaluateSolution(bestSolution);
        }

        //This method is responsible for managing the switches in Tandem mode
        public void ManageTandemSwitches()
        {
            //Checking whether idle iteration limit is reached
            if(idleIterCounter >= TSParameters.AllowedIdleIterations)
            {
                //Changing mode
                isNextModeRandom = !isNextModeRandom;
                tabuList.ChangeListParameter_Mode(isNextModeRandom ? "Random" : "Heuristic");
                idleIterCounter = 0;

                tandemIdleRunCounter++;

                if (tandemIdleRunCounter >= TSParameters.TandemIdleSwitches) shuffleRequested = true;            }
        }

        //This will fully evaluate and return the passed solution (cost + violations)
        public SolutionCandidate EvaluateSolution(SolutionCandidate solution)
        {
            CandidateCostCalculator calc = new CandidateCostCalculator(ctx);
            solution.score = calc.Evaluate(solution);
            solution.vl = new ViolationList().Evaluate(solution);
            return solution;
        }

        //Handling the iteration counters based on progress in the last iteration. Improvement will reset the counters.
        public void IterationCountHandler(bool improvement)
        {
            if (improvement)
            {
                idleIterCounter = 0;
                
                if(isTandemSearch) tandemIdleRunCounter = 0;
            }
            else
            {
                idleIterCounter++;
                if (isTandemSearch) ManageTandemSwitches();
                else
                {
                    if(idleIterCounter >= TSParameters.AllowedIdleIterations) shuffleRequested = true;
                }
            }
            generationCycleCounter = 0;
        }

        //Checking the global termination criteria
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

        //Used for initiating the neighbour generation process. Selecting the current mode for neighbour generation. 
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

        //Calling the neighbour generator with the determined parameters
        public List<SolutionCandidate> GetNeighbours(SolutionCandidate current, string mode)
        {
            NeighbourGeneratorWithVL neighbourGenerator = new NeighbourGeneratorWithVL(ctx);
            SolutionCandidate currentSolution = current.Clone();
            SolutionCandidate[] neighbours = new SolutionCandidate[TSParameters.GeneratedCandidates];

            neighbours = neighbourGenerator.GenerateNeighbours(currentSolution, mode);

            return neighbours.ToList<SolutionCandidate>();
        }

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
