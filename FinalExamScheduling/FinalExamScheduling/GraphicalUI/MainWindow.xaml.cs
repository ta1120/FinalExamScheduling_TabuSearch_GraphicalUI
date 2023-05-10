﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FinalExamScheduling.Model;
using FinalExamScheduling.TabuSearchScheduling;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace FinalExamScheduling
{
    
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            TSParameters.Mode = "Tandem";
        }

        private TabuSearchWithVL algorithm;

        private CancellationTokenSource cancellationTokenSource;

        bool algorithmRunning = false;

        Thread algorithmThread;

        public static Application WinApp { get; private set; }
        public static Window  _MainWindow { get; private set; }

        static void InitializeWindows()
        {
            WinApp = new Application();
            WinApp.Run(_MainWindow = new MainWindow());
        }

        [STAThread]
        static void Main(string[] args)
        {
            InitializeWindows();
            System.Environment.Exit(0);
        }

        //TODO Restructure if enough time left
        public void RunTabuSearch()
        {
            //Initialization
            resultBox.Dispatcher.Invoke(new Action(() => resultBox.Items.Add("# " + (DateTime.Now.ToString()))));
            resultBox.Dispatcher.Invoke(new Action(() => resultBox.Items.Add("Mode: " + (TSParameters.Mode))));
            List<double> results = new List<double>();
            double sum = 0;
            double feasibleScheduleCount = 0;

            var watch = Stopwatch.StartNew();

            //Reading input
            FileInfo existingFile = new FileInfo("Input.xlsx");
            if (!existingFile.Exists) 
            {
                resultBox.Dispatcher.Invoke(new Action(() => resultBox.Items.Add("ERROR: Input file not found!")));
                return;
            }
            var context = ExcelHelper.Read(existingFile);
            context.Init();


            algorithm = new TabuSearchWithVL(context);

            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken currentToken = cancellationTokenSource.Token;

            List<double> iterationProgress = new List<double>();

            //Running the algorithm
            SolutionCandidate solution = algorithm.Start(iterationProgress, currentToken);

            watch.Stop();

            Schedule resultSchedule = solution.schedule;
            double penaltyScore = solution.score;

            results.Add(solution.score);
            if (!solution.vl.ContainsHardViolation()) feasibleScheduleCount++;

            //Displaying the result score
            resultBox.Dispatcher.Invoke(new Action(() => resultBox.Items.Add(penaltyScore + " points")));
            resultBox.Dispatcher.Invoke(new Action(() => resultBox.SelectedIndex = resultBox.Items.Count - 1));
            resultBox.Dispatcher.Invoke(new Action(() => resultBox.ScrollIntoView(resultBox.SelectedItem)));

            //If parameter is set, re-running the algorithm
            if (TSParameters.RestartUntilTargetReached)
            {
                while (solution.score > TSParameters.TargetScore && !currentToken.IsCancellationRequested)
                {
                    //Quick reset
                    iterationProgress.Clear();
                    watch.Restart();
                    algorithm = new TabuSearchWithVL(context);

                    solution = algorithm.Start(iterationProgress,currentToken);
                    
                    watch.Stop();

                    resultSchedule = solution.schedule;
                    penaltyScore = solution.score;

                    results.Add(solution.score);
                    if (!solution.vl.ContainsHardViolation()) feasibleScheduleCount++;

                    //Calculating statistics every 5 run
                    if (results.Count % 5 == 0) 
                    {
                        sum = 0;
                        results.ForEach(s => sum += s);
                        minLabel.Dispatcher.Invoke(new Action(() => minLabel.Content = results.Min<double>() + " points"));
                        double avg = Math.Round((sum / results.Count), 2);
                        avgLabel.Dispatcher.Invoke(new Action(() => avgLabel.Content = avg + " points"));
                        feasiblePercentageLabel.Dispatcher.Invoke(new Action(() => feasiblePercentageLabel.Content = feasibleScheduleCount + "/" + results.Count + " " + Math.Round((feasibleScheduleCount * 100 / results.Count), 1) + "%"));
                    }

                    //Displaying the result score
                    resultBox.Dispatcher.Invoke(new Action(() => resultBox.Items.Add(penaltyScore + " points")));
                    resultBox.Dispatcher.Invoke(new Action(() => resultBox.SelectedIndex = resultBox.Items.Count - 1));
                    resultBox.Dispatcher.Invoke(new Action(() => resultBox.ScrollIntoView(resultBox.SelectedItem)));

                    //Conditional file output
                    if (penaltyScore < TSParameters.WriteOutLimit || TSParameters.WriteOutLimit < 0)
                    {
                        string elapsed1 = watch.Elapsed.ToString();
                        string extraInfo1 = ("_" + TSParameters.Mode + "_" + penaltyScore);

                        ExcelHelper.WriteTS(@"..\..\Results\Done_TS_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + extraInfo1 + ".xlsx", resultSchedule, context, new CandidateCostCalculator(context).GetFinalScores(resultSchedule), iterationProgress, elapsed1);
                    }
                }
            }

            string elapsed = watch.Elapsed.ToString();

            //Calculating statistics
            sum = 0;
            results.ForEach(s => sum += s);
            minLabel.Dispatcher.Invoke(new Action(() => minLabel.Content = results.Min<double>() + " points"));
            double avgFinal = Math.Round((sum / results.Count), 2);
            avgLabel.Dispatcher.Invoke(new Action(() => avgLabel.Content = avgFinal + " points"));
            feasiblePercentageLabel.Dispatcher.Invoke(new Action(() => feasiblePercentageLabel.Content = feasibleScheduleCount + "/" + results.Count + " " + Math.Round((feasibleScheduleCount * 100 / results.Count), 1) + "%"));

            string extraInfo = ("_" + TSParameters.Mode + "_" + penaltyScore);

            //File output
            ExcelHelper.WriteTS(@"..\..\Results\Done_TS_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + extraInfo + ".xlsx", resultSchedule, context, new CandidateCostCalculator(context).GetFinalScores(resultSchedule), iterationProgress, elapsed); 
        }

        private void ResetAfterRun()
        {
            algorithmRunning = false;
            abortButton.IsEnabled = false;
            runButton.IsEnabled = true;
            parameterBox.IsEnabled = true;
            cancellationTokenSource.Dispose();
        }

        private Thread CreateRunnerThread()
        {
            Console.WriteLine("Creating algorithm thread...");
            return new Thread(t =>
            {
                try 
                {
                    RunTabuSearch();
                }
                finally
                {
                    this.Dispatcher.Invoke(new Action(() => ResetAfterRun()));
                }
            })
            { IsBackground = true };
        }

        /*
         * # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #
         * 
         * From here on, the class only contains event handlers for the GUI controls
         * 
         * # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #
         */

        private void Run_Click(object sender, EventArgs e)
        {
            avgLabel.Content = "-";
            minLabel.Content = "-";
            feasiblePercentageLabel.Content = "-";

            if (algorithmRunning)
            {
                cancellationTokenSource.Cancel();
                

                algorithmThread = CreateRunnerThread();
            }
            else
            {
                algorithmRunning = true;
                algorithmThread = CreateRunnerThread();
            }

            abortButton.IsEnabled = true;
            runButton.IsEnabled = false;
            parameterBox.IsEnabled = false;

            algorithmThread.Start();
        }

        private void Abort()
        {
            if (algorithmRunning)
            {
                cancellationTokenSource.Cancel();
                
            }
            ResetAfterRun();
            runButton.IsEnabled = true;
        }
        private void Abort_Click(object sender, EventArgs e)
        {
            Abort();
        }
        private void OnModeSelectorChanged(object sender, SelectionChangedEventArgs e)
        {
            TSParameters.Mode = ((sender as ComboBox).SelectedItem as ComboBoxItem).Content as string;
        }

        private void OnSoftConstCBUnchecked(object sender, RoutedEventArgs e)
        {
            TSParameters.OptimizeSoftConstraints = false;
        }
        private void OnSoftConstCBChecked(object sender, RoutedEventArgs e)
        {
            TSParameters.OptimizeSoftConstraints = true;
        }
        private void OnShuffleCBUnchecked(object sender, RoutedEventArgs e)
        {
            maxShufflesInput.IsEnabled = false;
            shufflePercentageInput.IsEnabled = false;
            TSParameters.AllowShuffleWhenStuck = false;
        }
        private void OnShuffleCBChecked(object sender, RoutedEventArgs e)
        {
            maxShufflesInput.IsEnabled = true;
            shufflePercentageInput.IsEnabled = true; 
            TSParameters.AllowShuffleWhenStuck = true;
            SetShufflePercentage();
        }
        private void OnRestartCBUnchecked(object sender, RoutedEventArgs e)
        {
            TSParameters.RestartUntilTargetReached = false;
        }
        private void OnRestartCBChecked(object sender, RoutedEventArgs e)
        {
            TSParameters.RestartUntilTargetReached = true;
        }
        private void OnWriteOutCBUnchecked(object sender, RoutedEventArgs e)
        {
            writeOutLimitInput.IsEnabled = true;
            SetWriteOutLimit();
        }
        private void OnWriteOutCBChecked(object sender, RoutedEventArgs e)
        {
            writeOutLimitInput.IsEnabled = false;
            TSParameters.WriteOutLimit = -1;
        }
        private void OnHardFirstCBUnchecked(object sender, RoutedEventArgs e)
        {
            TSParameters.FixAllHardFirst = false;
        }
        private void OnHardFirstCBChecked(object sender, RoutedEventArgs e)
        {
            TSParameters.FixAllHardFirst = true;
        }
        private void OnWriteoutLimitChanged(object sender, TextChangedEventArgs args) { SetWriteOutLimit(); }
        private void SetWriteOutLimit()
        {
            int limit;
            if (int.TryParse(writeOutLimitInput.Text, out limit))
            {
                if(limit > 0 && limit <= 10000) TSParameters.WriteOutLimit = limit;
                else TSParameters.WriteOutLimit = 0;
            } 
            else TSParameters.WriteOutLimit = 0;
        }
        private void OnShufflePercentageChanged(object sender, TextChangedEventArgs args) { SetShufflePercentage(); }
        private void OnMaxShufflesChanged(object sender, TextChangedEventArgs args) { SetShuffleCount(); }
        private void SetShufflePercentage()
        {
            int percentage;
            if (int.TryParse(shufflePercentageInput.Text, out percentage)) TSParameters.ShufflePercentage = percentage;
            else TSParameters.WriteOutLimit = 0;
        }
        private void SetShuffleCount()
        {
            int count;
            if (int.TryParse(maxShufflesInput.Text, out count)) TSParameters.MaxShuffles = count;
            else TSParameters.MaxShuffles = 1;
        }
        private void OnGeneratedCandidatesChanged(object sender, TextChangedEventArgs args) 
        {
            int n;
            if (int.TryParse(generatedCandidatesInput.Text, out n)) TSParameters.GeneratedCandidates = n;
            else TSParameters.GeneratedCandidates = 10;
        }
        private void OnIdleIterationsChanged(object sender, TextChangedEventArgs args)
        {
            int n;
            if (int.TryParse(allowedIdleIterationsInput.Text, out n)) TSParameters.AllowedIdleIterations = n;
            else TSParameters.AllowedIdleIterations = 10;
        }
        private void OnMaxFailedGenerationsChanged(object sender, TextChangedEventArgs args)
        {
            int n;
            if (int.TryParse(maxFailedNeighbourGenerationsInput.Text, out n)) TSParameters.MaxFailedNeighbourGenerations = n;
            else TSParameters.MaxFailedNeighbourGenerations = 5;
        }
        private void OnTandemSwitchesChanged(object sender, TextChangedEventArgs args)
        {
            int n;
            if (int.TryParse(tandemIdleSwitchesInput.Text, out n)) TSParameters.TandemIdleSwitches = n;
            else TSParameters.TandemIdleSwitches = 5;
        }
        private void OnTargetScoreChanged(object sender, TextChangedEventArgs args)
        {
            int n;
            if (int.TryParse(targetScoreInput.Text, out n)) TSParameters.TargetScore = n;
            else TSParameters.TargetScore = 0;
        }
        private void OnRandomTTLChanged(object sender, TextChangedEventArgs args)
        {
            int n;
            if (int.TryParse(randomTTLInput.Text, out n)) TSParameters.Random.TabuLifeIterations = n;
            else TSParameters.Random.TabuLifeIterations = 1;
        }
        private void OnRandomListLengthChanged(object sender, TextChangedEventArgs args)
        {
            int n;
            if (int.TryParse(randomTabuListLengthInput.Text, out n)) TSParameters.Random.TabuListLength = n;
            else TSParameters.Random.TabuListLength = 1;
        }
        private void OnHeuristicTTLChanged(object sender, TextChangedEventArgs args)
        {
            int n;
            if (int.TryParse(heuristicTTLInput.Text, out n)) TSParameters.Heuristic.TabuLifeIterations = n;
            else TSParameters.Heuristic.TabuLifeIterations = 1;
        }
        private void OnHeuristicListLengthChanged(object sender, TextChangedEventArgs args)
        {
            int n;
            if (int.TryParse(heuristicTabuListLengthInput.Text, out n)) TSParameters.Heuristic.TabuListLength = n;
            else TSParameters.Heuristic.TabuListLength = 1;
        }
        private void OnViolationFixCountChanged(object sender, TextChangedEventArgs args)
        {
            int n;
            if (int.TryParse(violationCountInput.Text, out n)) TSParameters.ViolationsToFixPerGeneration = n;
            else TSParameters.ViolationsToFixPerGeneration = 50;
        }
    }
}
