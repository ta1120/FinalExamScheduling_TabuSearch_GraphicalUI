using FinalExamScheduling.Model;
using System;
using System.Collections.Generic;

namespace FinalExamScheduling.TabuSearchScheduling
{
    class TabuSearchWithVL
    {
        public Context ctx;

        public TabuList globalTabuList;


        public TabuSearchWithVL(Context context)
        {
            ctx = context;
            globalTabuList = new TabuList();
        }

        public SolutionCandidate Start(List<double> iterationProgress)
        {
            CandidateCostCalculator costCalc = new CandidateCostCalculator(ctx);
            NeighbourGeneratorWithVL neighbourGenerator = new NeighbourGeneratorWithVL(ctx);


            int examCount = ctx.Students.Length;

            bool NextModeIsRandom = false;

            bool TandemSearch = TSParameters.Mode.Equals("Tandem");

            SolutionCandidate current;
            SolutionCandidate bestSoFar;

            //kiindulo megoldas generalasa
            SolutionCandidate initialSolution = new RandomInitialSolutionGenerator(ctx).generateInitialSolution();
            current = initialSolution;
            bestSoFar = initialSolution;

            //koltseg es VL kiszamitasa
            current.Score = costCalc.Evaluate(current);
            if (TSParameters.PrintDetails) Console.WriteLine("Initial schedule score: " + current.Score);
            current.VL = new ViolationList().Evaluate(current);
            if (TSParameters.LogIterationalProgress) iterationProgress.Add(current.Score);

            //ciklus terminalasig
            int iterCounter = 1;
            int idleIterCounter = 0;
            int tandemIdleRunCounter = 0;
            int shuffleCounter = 0;
            double prevBestScore = current.Score;

            if (TSParameters.PrintDetails) Console.WriteLine("Neighbour generation mode: " + TSParameters.Mode);
            if (TandemSearch && TSParameters.PrintDetails) Console.WriteLine("Starting from " + (NextModeIsRandom ? "Random" : "Greedy") + " mode");

            while ((idleIterCounter < TSParameters.AllowedIdleIterations && !TandemSearch) || (TandemSearch && (tandemIdleRunCounter < TSParameters.TandemIdleSwitches)) || (TSParameters.AllowShuffleWhenStuck && shuffleCounter < TSParameters.MaxShuffles))
            {
                if (TSParameters.AllowShuffleWhenStuck && idleIterCounter >= TSParameters.AllowedIdleIterations && shuffleCounter < TSParameters.MaxShuffles)
                {
                    current = ShuffleStudents(current).Clone();
                    shuffleCounter++;
                    idleIterCounter = 0;
                    if (TSParameters.PrintDetails) Console.WriteLine("Shuffled students for the #" + shuffleCounter + " time");
                }

                //szomszedok generalasa VL alapjan
                SolutionCandidate bestNeighbour = new SolutionCandidate(current.Clone().Schedule);
                SolutionCandidate aspirationCandidate = new SolutionCandidate(current.Clone().Schedule);

                int failedIterations = 0;

                //TODO: kivenni a módellenőrzést a cikluson kívülre + szépítés

                do
                {
                    SolutionCandidate[] neighbours = new SolutionCandidate[TSParameters.GeneratedCandidates];
                    switch (TSParameters.Mode)
                    {
                        case "Random":

                            if (failedIterations > TSParameters.MaxFailedNeighbourGenerations)
                            {
                                if (TSParameters.PrintDetails) Console.WriteLine("Should terminate now...");
                                if (TSParameters.PrintDetails) Console.WriteLine("If this keeps happening, maybe you should tweak the values in the TSParameters class.");
                                //globalTabuList.PrintTabuList();
                                return bestSoFar;
                            }
                            if (failedIterations > 0 && TSParameters.PrintDetails) Console.WriteLine("No feasible solutions were generated... Retrying #" + failedIterations);
                            neighbours = neighbourGenerator.GenerateNeighboursRandom(current.Clone());
                            //legjobb nem tabu szomszed kivalasztasa
                            bestNeighbour = SelectBestFeasibleCandidate(neighbours);
                            aspirationCandidate = SelectAspirationCandidate(neighbours);
                            failedIterations++;
                            break;
                        case "Greedy":
                            if (failedIterations > TSParameters.MaxFailedNeighbourGenerations)
                            {
                                if (TSParameters.PrintDetails) Console.WriteLine("Should terminate now...");
                                if (TSParameters.PrintDetails) Console.WriteLine("If this keeps happening, maybe you should tweak the values in the TSParameters class.");
                                //globalTabuList.PrintTabuList();
                                //bestSoFar.VL.printViolations(bestSoFar);
                                return bestSoFar;
                            }
                            if (failedIterations > 0 && TSParameters.PrintDetails) Console.WriteLine("No feasible solutions were generated... Retrying #" + failedIterations);
                            neighbours = neighbourGenerator.GenerateNeighboursHeuristic(current.Clone());
                            //legjobb nem tabu szomszed kivalasztasa
                            bestNeighbour = SelectBestFeasibleCandidate(neighbours);
                            aspirationCandidate = SelectAspirationCandidate(neighbours);
                            failedIterations++;
                            break;
                        case "Tandem":
                            if (idleIterCounter >= TSParameters.AllowedIdleIterations)
                            {
                                if (TSParameters.PrintDetails) Console.WriteLine("\n[ Tandem runner change ]");
                                NextModeIsRandom = !NextModeIsRandom;
                                idleIterCounter = 0;
                                tandemIdleRunCounter++;
                                
                            }
                            if (NextModeIsRandom)
                            {
                                if (failedIterations > TSParameters.MaxFailedNeighbourGenerations)
                                {
                                    NextModeIsRandom = false;
                                    failedIterations = 0;
                                    idleIterCounter = 0;
                                    tandemIdleRunCounter++;
                                    
                                    if (TSParameters.PrintDetails) Console.WriteLine("\n[ Tandem runner change ]");
                                    //globalTabuList.PrintTabuList();
                                    //return bestSoFar;
                                }
                                if (failedIterations > 0 && TSParameters.PrintDetails) Console.WriteLine("[ Random ] No feasible solutions were generated... Retrying #" + failedIterations);

                                neighbours = neighbourGenerator.GenerateNeighboursRandom(current.Clone());
                                //legjobb nem tabu szomszed kivalasztasa
                                bestNeighbour = SelectBestFeasibleCandidate(neighbours);
                                aspirationCandidate = SelectAspirationCandidate(neighbours);
                                failedIterations++;
                            }
                            else
                            {
                                if (failedIterations > TSParameters.MaxFailedNeighbourGenerations)
                                {
                                    NextModeIsRandom = true;
                                    failedIterations = 0;
                                    idleIterCounter = 0;
                                    tandemIdleRunCounter++;
                                    
                                    if (TSParameters.PrintDetails) Console.WriteLine("\n[ Tandem runner change ]");
                                    //globalTabuList.PrintTabuList();
                                    //return bestSoFar;
                                }
                                if (failedIterations > 0 && TSParameters.PrintDetails) Console.WriteLine("[ Greedy ] No feasible solutions were generated... Retrying #" + failedIterations);

                                neighbours = neighbourGenerator.GenerateNeighboursHeuristic(current.Clone());
                                //legjobb nem tabu szomszed kivalasztasa
                                bestNeighbour = SelectBestFeasibleCandidate(neighbours);
                                aspirationCandidate = SelectAspirationCandidate(neighbours);
                                failedIterations++;
                            }
                            break;
                        default:
                            Console.WriteLine("TSParameters.Mode has an invalid value. It should be either Random/Greedy/Tandem");
                            return bestSoFar;
                            break;
                    }
                    if(TSParameters.PrintDetails) Console.WriteLine(bestNeighbour == null ? "No feasible neighbour generated" : "Found at least one feasible neighbour");
                }
                while (bestNeighbour == null && (!TandemSearch || (TandemSearch && tandemIdleRunCounter < TSParameters.TandemIdleSwitches)));

                if (bestNeighbour == null) bestNeighbour = current;

                if (aspirationCandidate != null)
                {
                    aspirationCandidate.VL = new ViolationList().Evaluate(aspirationCandidate);
                }
                
                //tabulista iteraciok csokkentese
                globalTabuList.DecreaseIterationsLeft();

                //tabulista kiegeszite
                ExpandTabuList(current, bestNeighbour);

                //jelenlegi beallitasa a legjobb szomszédra
                current = bestNeighbour;

                current.VL = new ViolationList().Evaluate(current);

                if (TSParameters.PrintDetails) Console.WriteLine("#" + iterCounter++ + " iteration best neighbour score: " + current.Score);
                if (TSParameters.LogIterationalProgress) iterationProgress.Add(current.Score);

                //legjobb megoldás frissítése ha jobb az új
                if (current.Score < bestSoFar.Score) bestSoFar = current;

                //AspirationCriteria ellenőrzése
                if (aspirationCandidate != null && bestSoFar.Score > aspirationCandidate.Score)
                {
                    if (TSParameters.PrintDetails) Console.WriteLine("-> AspirationCriteria met, updating current and best schedule. Score: " + aspirationCandidate.Score);
                    current = aspirationCandidate.Clone();
                    bestSoFar = aspirationCandidate.Clone();
                    globalTabuList.tabuList.Clear();
                }

                //ellenőrizzük, hogy javult-e a legjobb megoldás
                if (bestSoFar.Score >= prevBestScore)
                {
                    idleIterCounter++;
                }
                else
                {
                    idleIterCounter = 0;
                    if (TandemSearch)
                    {
                        tandemIdleRunCounter = 0;
                        //Console.WriteLine("[ Improvement ] on this run.");
                    }
                }

                //ha üres a legjobb megoldás VL listája, akkor a jelenleg implementált követelmények maximálisan optimalizálva lettek
                if (bestSoFar.VL.Violations.Count == 0)
                {
                    Console.WriteLine("Solution is fully optimized.");
                    return bestSoFar;
                }

                prevBestScore = bestSoFar.Score;
            }


            bestSoFar.VL = new ViolationList().Evaluate(bestSoFar);
            //Console.WriteLine("\nViolations in best found schedule: \n");
            if (TSParameters.PrintDetails) bestSoFar.VL.printViolations();

            //globalTabuList.PrintTabuList();

            if (idleIterCounter >= TSParameters.AllowedIdleIterations && TSParameters.PrintDetails) Console.WriteLine("Maximum allowed idle iterations reached (" + TSParameters.AllowedIdleIterations + ")");

            return bestSoFar;
        }

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

        //Selects the best solution without tabu moves
        public SolutionCandidate SelectBestFeasibleCandidate(SolutionCandidate[] neighbours)
        {
            CandidateCostCalculator ccc = new CandidateCostCalculator(ctx);
            SolutionCandidate best = null;
            foreach (SolutionCandidate candidate in neighbours)
            {
                if (best == null && IsFeasibleSolution(candidate))
                {
                    candidate.Score = ccc.Evaluate(candidate);
                    best = candidate.Clone();
                    //Console.WriteLine("Score: " + candidate.Score);
                }
                else if (IsFeasibleSolution(candidate))
                {
                    candidate.Score = ccc.Evaluate(candidate);
                    if (candidate.Score < best.Score) best = candidate.Clone();
                    //Console.WriteLine("Score: " + candidate.Score);
                }
            }
            //Console.WriteLine("BestScore: " + best.Score);
            return best;
        }

        public SolutionCandidate SelectAspirationCandidate(SolutionCandidate[] neighbours)
        {
            CandidateCostCalculator ccc = new CandidateCostCalculator(ctx);
            SolutionCandidate best = null;
            foreach (SolutionCandidate candidate in neighbours)
            {
                if (best == null && !IsFeasibleSolution(candidate))
                {
                    candidate.Score = ccc.Evaluate(candidate);
                    best = candidate.Clone();
                    //Console.WriteLine("Score: " + candidate.Score);
                }
                else if (!IsFeasibleSolution(candidate))
                {
                    candidate.Score = ccc.Evaluate(candidate);
                    if (candidate.Score < best.Score) best = candidate.Clone();
                }
            }
            return best;
        }

        //Check whether a solution contains tabu elements/moves
        public bool IsFeasibleSolution(SolutionCandidate solution)
        {
            FinalExam[] exams = solution.Schedule.FinalExams;
            foreach (TabuListElement tabu in globalTabuList.GetTabuList())
            {
                if (tabu.TabuIterationsLeft > 0)
                {
                    switch (tabu.Attribute)
                    {
                        case "student":
                            for (int i = 0; i < exams.Length; i++)
                            {
                                if (i == tabu.ExamSlot)
                                {
                                    if (exams[i].Student.Name.Equals(tabu.Value))
                                    {
                                        return false;
                                    }
                                }
                            }
                            break;
                        case "supervisor":
                            for (int i = 0; i < exams.Length; i++)
                            {
                                if (i == tabu.ExamSlot)
                                {
                                    if (exams[i].Supervisor.Name.Equals(tabu.Value))
                                    {
                                        return false;
                                    }
                                }
                            }
                            break;
                        case "president":
                            for (int i = 0; i < exams.Length; i++)
                            {
                                if (i == tabu.ExamSlot)
                                {
                                    if (exams[i].President.Name.Equals(tabu.Value))
                                    {
                                        return false;
                                    }
                                }
                            }
                            break;
                        case "secretary":
                            for (int i = 0; i < exams.Length; i++)
                            {
                                if (i == tabu.ExamSlot)
                                {
                                    if (exams[i].Secretary.Name.Equals(tabu.Value))
                                    {
                                        return false;
                                    }
                                }
                            }
                            break;
                        case "member":
                            for (int i = 0; i < exams.Length; i++)
                            {
                                if (i == tabu.ExamSlot)
                                {
                                    if (exams[i].Member.Name.Equals(tabu.Value))
                                    {
                                        return false;
                                    }
                                }
                            }
                            break;
                        case "examiner":
                            for (int i = 0; i < exams.Length; i++)
                            {
                                if (i == tabu.ExamSlot)
                                {
                                    if (exams[i].Examiner.Name.Equals(tabu.Value))
                                    {
                                        return false;
                                    }
                                }
                            }
                            break;
                    }
                }
            }
            return true;
        }

        //Add new elements to the tabu list based on changed attributes in schedules
        public void ExpandTabuList(SolutionCandidate current, SolutionCandidate bestNeighbour)
        {
            int examCount = ctx.Students.Length;

            for (int i = 0; i < examCount; i++)
            {
                Schedule oldSchedule = current.Schedule.Clone();
                Schedule newSchedule = bestNeighbour.Schedule.Clone();

                if (!oldSchedule.FinalExams[i].Student.Name.Equals(newSchedule.FinalExams[i].Student.Name))
                {
                    globalTabuList.Add(new TabuListElement("student", oldSchedule.FinalExams[i].Student.Name, i));
                }
                if (!oldSchedule.FinalExams[i].Supervisor.Name.Equals(newSchedule.FinalExams[i].Supervisor.Name))
                {
                    globalTabuList.Add(new TabuListElement("supervisor", oldSchedule.FinalExams[i].Supervisor.Name, i));
                }
                if (!oldSchedule.FinalExams[i].President.Name.Equals(newSchedule.FinalExams[i].President.Name))
                {
                    globalTabuList.Add(new TabuListElement("president", oldSchedule.FinalExams[i].President.Name, i));
                }
                if (!oldSchedule.FinalExams[i].Secretary.Name.Equals(newSchedule.FinalExams[i].Secretary.Name))
                {
                    globalTabuList.Add(new TabuListElement("secretary", oldSchedule.FinalExams[i].Secretary.Name, i));
                }
                if (!oldSchedule.FinalExams[i].Member.Name.Equals(newSchedule.FinalExams[i].Member.Name))
                {
                    globalTabuList.Add(new TabuListElement("member", oldSchedule.FinalExams[i].Member.Name, i));
                }
                if (!oldSchedule.FinalExams[i].Examiner.Name.Equals(newSchedule.FinalExams[i].Examiner.Name))
                {
                    globalTabuList.Add(new TabuListElement("examiner", oldSchedule.FinalExams[i].Examiner.Name, i));
                }
            }
        }
    }
}
