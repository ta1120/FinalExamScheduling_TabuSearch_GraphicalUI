using System;
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
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System.Runtime.Remoting.Contexts;
using Context = FinalExamScheduling.Model.Context;

namespace FinalExamScheduling
{
    
    //This class is realizing a window, that provides the GUI for the program. Contains code for conrolling the GUI, and creating and running an algorithm thread
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            TSParameters.Mode = "Tandem"; //This is the default mode
        }

        //Cancellation tokens are used for manually terminating a run at a safe point
        private CancellationTokenSource cancellationTokenSource;

        //Used for storing the current state
        bool algorithmRunning = false;

        //Points to the current algorithm thread
        Thread algorithmThread;

        public static Application WinApp { get; private set; }
        public static Window  _MainWindow { get; private set; }

        //Instantiating and running the MainWindow
        static void InitializeWindows()
        {
            WinApp = new Application();
            WinApp.Run(_MainWindow = new MainWindow());
        }

        //The main 
        [STAThread]
        static void Main(string[] args)
        {
            InitializeWindows();
        }

        //Running the algorithm until any termination criteria is met
        public void RunTabuSearch()
        {
            //Initialization
            this.Dispatcher.Invoke(new Action(() => this.DisplayNewRun()));

            //Variables used for calculating stats
            List<double> results = new List<double>();
            int feasibleScheduleCount = 0;

            //Trying to read the input, when unsuccessfull, the program will notify the user, and the method will quit
            FileInfo existingFile = new FileInfo("Input.xlsx");
            if (!existingFile.Exists) 
            {
                resultBox.Dispatcher.Invoke(new Action(() => resultBox.Items.Add("ERROR: Input file not found!")));
                return;
            }
            var context = ExcelHelper.Read(existingFile);
            context.Init();

            //Instantiation of the algorithm runner class
            TabuSearchWithVL algorithm = new TabuSearchWithVL(context);

            //Instantiating a cancellation token for every new run (disposed of at the end of run)
            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken currentToken = cancellationTokenSource.Token;

            //Used for storing the iterational progress for the file output
            List<double> iterationProgress = new List<double>();

            //The time of each run is measured using a stopwatch
            var watch = Stopwatch.StartNew();

            //Running the algorithm
            SolutionCandidate solution = algorithm.Start(iterationProgress, currentToken);

            //Administrating run results when it is finished
            watch.Stop();
            string elapsed = watch.Elapsed.ToString();
            Schedule resultSchedule = solution.schedule;
            double penaltyScore = solution.score;
            results.Add(penaltyScore);
            if (!solution.vl.ContainsHardViolation()) feasibleScheduleCount++;

            //Displaying the result score
            this.Dispatcher.Invoke(new Action(() => this.DisplayResult(penaltyScore)));
            //Displaying the stats
            this.Dispatcher.Invoke(new Action(() => this.DisplayStats(results, feasibleScheduleCount)));

            //Writing the schedule to Excel
            WriteToExcel(penaltyScore, resultSchedule, context, iterationProgress, elapsed);

            //If restart parameter is set, re-running the algorithm until target points are reached
            if (TSParameters.RestartUntilTargetReached)
            {
                while (solution.score > TSParameters.TargetScore && !currentToken.IsCancellationRequested)
                {
                    //Reset for new run
                    iterationProgress.Clear();

                    //Resetting control variables for a new run
                    algorithm.InitializeAlgorithm();

                    //Restarting stopwatch
                    watch.Restart();

                    //Running the algorithm again
                    solution = algorithm.Start(iterationProgress,currentToken);
                    
                    //Administration of result
                    watch.Stop();
                    elapsed = watch.Elapsed.ToString();
                    resultSchedule = solution.schedule;
                    penaltyScore = solution.score;
                    results.Add(solution.score);
                    if (!solution.vl.ContainsHardViolation()) feasibleScheduleCount++;

                    //Calculating statistics every time
                    this.Dispatcher.Invoke(new Action(() => this.DisplayStats(results,feasibleScheduleCount)));

                    //Displaying the result score
                    this.Dispatcher.Invoke(new Action(() => this.DisplayResult(penaltyScore)));

                    //Writing the new results to Excel
                    WriteToExcel(penaltyScore, resultSchedule, context, iterationProgress, elapsed);
                }
            }
        }

        //Resetting the GUI controls and control variables when the thread is finished
        private void ResetAfterRun()
        {
            algorithmRunning = false;
            abortButton.IsEnabled = false;
            runButton.IsEnabled = true;
            parameterBox.IsEnabled = true;
            cancellationTokenSource.Dispose();
            resultBox.Items.Add("@ " + (DateTime.Now.ToString()));
            resultBox.Items.Add("--Run finished");
        }

        //Creating the thread to run the algorithm
        private Thread CreateRunnerThread()
        {
            return new Thread(t =>
            {
                try { RunTabuSearch(); }

                finally { this.Dispatcher.Invoke(new Action(() => ResetAfterRun())); }
            })
            { IsBackground = true };
        }

        //When the global parameters allow it, writing the result schedule to Excel
        private void WriteToExcel(double penaltyScore, Schedule result, Context ctx, List<double> iterationProgress, string time)
        {
            if (!(penaltyScore < TSParameters.WriteOutLimit || TSParameters.WriteOutLimit < 0)) return;

            string path = @"Results\";
            string fileNamePrefix = "Done_TS";
            string dateTime = DateTime.Now.ToString("yyyyMMdd_HHmm");
            string runInfo = (TSParameters.Mode + "_" + penaltyScore);
            string format = ".xlsx";
            string fullFilePath = path + fileNamePrefix + "_" + dateTime + "_" + runInfo + format;
            double[] finalScoresPerConstraint = new CandidateCostCalculator(ctx).GetFinalScores(result);

            ExcelHelper.WriteTS(fullFilePath, result, ctx, finalScoresPerConstraint, iterationProgress, time);
        }

        //This will display the date, time and mode in the result box at the start of a run
        private void DisplayNewRun()
        {
            resultBox.Items.Add("# " + (DateTime.Now.ToString()));
            resultBox.Items.Add("Scheduling started...");
            resultBox.Items.Add("Mode: " + (TSParameters.Mode));
            resultBox.Items.Add("Please wait...");
        }

        //This will add the passed result to the result box, and make sure, it is scrolled into view
        private void DisplayResult(double result)
        {
            resultBox.Items.Add(result + " points");
            resultBox.SelectedIndex = resultBox.Items.Count - 1;
            resultBox.ScrollIntoView(resultBox.SelectedItem);
        }

        //This will update the statistics section on the page
        private void DisplayStats(List<double> results, int feasibleScheduleCount)
        {
            double sum = 0;
            results.ForEach(s => sum += s);
            minLabel.Content = results.Min<double>() + " points";
            double avgFinal = Math.Round((sum / results.Count), 2);
            avgLabel.Content = avgFinal + " points";
            feasiblePercentageLabel.Content = feasibleScheduleCount + "/" + results.Count + " " + Math.Round(((double)feasibleScheduleCount * 100 / results.Count), 1) + "%";
        }

        //# # # # # # # # # # # # # # # # # # # #
        //# Event handlers for the GUI controls #
        //# # # # # # # # # # # # # # # # # # # #         

        //Event handler for the Run button - Creating and running the algorithm thread, handling other GUI controls
        private void Run_Click(object sender, EventArgs e)
        {
            avgLabel.Content = "-";
            minLabel.Content = "-";
            feasiblePercentageLabel.Content = "-";

            //This should not actually happen during a run because ResetAfterRun is called in the "finally" block of the thread, safeguard
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

            //Handling GUI controls
            abortButton.IsEnabled = true;
            runButton.IsEnabled = false;
            parameterBox.IsEnabled = false;

            //Starting the thread
            algorithmThread.Start();
        }
        
        //Event handler for the Abort button - Requesting cancellation through the token. The disposal of the token is automatically done when the thread is finished
        private void Abort_Click(object sender, EventArgs e)
        {
            if (algorithmRunning)
            {
                cancellationTokenSource.Cancel();
            }
        }

        //Parameter control handlers...

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
