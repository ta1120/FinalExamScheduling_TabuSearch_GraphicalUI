using FinalExamScheduling.Model;
using FinalExamScheduling.TabuSearchScheduling;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace FinalExamScheduling
{
    public class Program
    {
        static TabuSearchScheduler scheduler;

        static void Main(string[] args)
        {
            RunTabuSearch();

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
            System.Environment.Exit(0);

        }

        //Based on the RunGenetic() function
        static void RunTabuSearch()
        {
            double sum = 0;
            int ctr = 0;

            var watch = Stopwatch.StartNew();
            FileInfo existingFile = new FileInfo("Input.xlsx");

            var context = ExcelHelper.Read(existingFile);
            context.Init();
            scheduler = new TabuSearchScheduler(context);
            bool done = false;

            List<double> iterationProgress = new List<double>();

            SolutionCandidate solution = scheduler.Run(iterationProgress);
            watch.Stop();
            Schedule resultSchedule = solution.Schedule;
            double penaltyScore = solution.Score;

            sum += solution.Score;
            ctr++;

            if (TSParameters.RestartUntilTargetReached)
            {
                while (solution.Score > TSParameters.TargetScore)
                {
                    if (TSParameters.PrintDetails) Console.WriteLine("Target not reached... Restarting search");
                    if (TSParameters.PrintDetails) Console.WriteLine("#######################################");
                    iterationProgress.Clear();
                    watch.Restart();
                    solution = scheduler.Run(iterationProgress);
                    watch.Stop();
                    resultSchedule = solution.Schedule;
                    penaltyScore = solution.Score;

                    sum += solution.Score;
                    ctr++;

                    double avg = Math.Round((sum / ctr), 2);
                    /*if(!TSParameters.MuteConsoleUnlessDone)*/
                    if (ctr % 30 == 0) Console.WriteLine("@AVG " + avg + "\n");
                    if (!TSParameters.MuteConsoleUnlessDone) Console.WriteLine("Best penalty score reached: " + penaltyScore);

                    if (penaltyScore < TSParameters.WriteOutLimit || TSParameters.WriteOutLimit < 0)
                    {
                        string elapsed1 = watch.Elapsed.ToString();
                        string extraInfo1 = ("_" + TSParameters.Mode + "_" + penaltyScore);

                        ExcelHelper.WriteTS(@"..\..\Results\Done_TS_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + extraInfo1 + ".xlsx", resultSchedule, context, new CandidateCostCalculator(context).GetFinalScores(resultSchedule), iterationProgress, elapsed1);

                    }
                }
            }
            string elapsed = watch.Elapsed.ToString();

            Console.WriteLine("Best penalty score: " + penaltyScore);

            //solution.VL.printViolations();

            string extraInfo = ("_" + TSParameters.Mode + "_" + penaltyScore);

            ExcelHelper.WriteTS(@"..\..\Results\Done_TS_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + extraInfo + ".xlsx", resultSchedule, context, new CandidateCostCalculator(context).GetFinalScores(resultSchedule), iterationProgress, elapsed);
            Console.WriteLine("Done");
        }
    }
}
