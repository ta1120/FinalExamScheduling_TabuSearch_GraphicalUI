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

namespace FinalExamScheduling
{
    
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            TSParameters.Mode = "Tandem";
        }

        static TabuSearchScheduler scheduler;

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

        //Based on the RunGenetic() function
        public void RunTabuSearch()
        {
            resultBox.Dispatcher.Invoke(new Action(() => resultBox.Items.Add("# " + (DateTime.Now.ToString()))));
            resultBox.Dispatcher.Invoke(new Action(() => resultBox.Items.Add("Mode: " + (TSParameters.Mode))));
            List<double> results = new List<double>();
            double sum = 0;
            double feasibleScheduleCount = 0;

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

            results.Add(solution.Score);
            if (!solution.VL.ContainsHardViolation()) feasibleScheduleCount++;

            if (TSParameters.RestartUntilTargetReached)
            {
                while (solution.Score > TSParameters.TargetScore)
                {
                    iterationProgress.Clear();
                    watch.Restart();
                    solution = scheduler.Run(iterationProgress);
                    watch.Stop();
                    resultSchedule = solution.Schedule;
                    penaltyScore = solution.Score;

                    results.Add(solution.Score);
                    
                    if (results.Count % 10 == 0) 
                    {
                        sum = 0;
                        results.ForEach(s => sum += s);
                        minLabel.Dispatcher.Invoke(new Action(() => minLabel.Content = results.Min<double>() + " points"));
                        double avg = Math.Round((sum / results.Count), 2);
                        avgLabel.Dispatcher.Invoke(new Action(() => avgLabel.Content = avg + " points"));
                        feasiblePercentageLabel.Dispatcher.Invoke(new Action(() => feasiblePercentageLabel.Content = feasibleScheduleCount + "/" + results.Count + " " + Math.Round((feasibleScheduleCount / results.Count), 1) + "%"));
                    } 
                    resultBox.Dispatcher.Invoke(new Action(() => resultBox.Items.Add(penaltyScore + " points")));
                    resultBox.Dispatcher.Invoke(new Action(() => resultBox.SelectedIndex = resultBox.Items.Count - 1));
                    resultBox.Dispatcher.Invoke(new Action(() => resultBox.ScrollIntoView(resultBox.SelectedItem)));
                    if (penaltyScore < TSParameters.WriteOutLimit || TSParameters.WriteOutLimit < 0)
                    {
                        string elapsed1 = watch.Elapsed.ToString();
                        string extraInfo1 = ("_" + TSParameters.Mode + "_" + penaltyScore);

                        ExcelHelper.WriteTS(@"..\..\Results\Done_TS_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + extraInfo1 + ".xlsx", resultSchedule, context, new CandidateCostCalculator(context).GetFinalScores(resultSchedule), iterationProgress, elapsed1);

                    }
                }
            }
            string elapsed = watch.Elapsed.ToString();

            sum = 0;
            results.ForEach(s => sum += s);
            minLabel.Dispatcher.Invoke(new Action(() => minLabel.Content = results.Min<double>() + " points"));
            double avgFinal = Math.Round((sum / results.Count), 2);
            avgLabel.Dispatcher.Invoke(new Action(() => avgLabel.Content = avgFinal + " points"));
            resultBox.Dispatcher.Invoke(new Action(() => resultBox.Items.Add(penaltyScore + " points")));
            resultBox.Dispatcher.Invoke(new Action(() => resultBox.SelectedIndex = resultBox.Items.Count - 1));
            resultBox.Dispatcher.Invoke(new Action(() => resultBox.ScrollIntoView(resultBox.SelectedItem)));

            string extraInfo = ("_" + TSParameters.Mode + "_" + penaltyScore);

            ExcelHelper.WriteTS(@"..\..\Results\Done_TS_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + extraInfo + ".xlsx", resultSchedule, context, new CandidateCostCalculator(context).GetFinalScores(resultSchedule), iterationProgress, elapsed); 
        }

        private void Run_Click(object sender, EventArgs e)
        {
            if (algorithmRunning)
            {
                algorithmThread.Abort();

                algorithmThread = new Thread(t =>
                {
                    RunTabuSearch();
                })
                { IsBackground = true };
            }
            else
            {
                algorithmRunning = true;
                algorithmThread = new Thread(t =>
                {
                    RunTabuSearch();
                })
                { IsBackground = true };
            }
            abortButton.IsEnabled = true;
            parameterBox.IsEnabled = false;
            algorithmThread.Start();
        }

        private void Abort()
        {
            if (algorithmRunning)
            {
                algorithmThread.Abort();
            }
            algorithmRunning = false;
            abortButton.IsEnabled = false;
            parameterBox.IsEnabled = true;
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
            TSParameters.AllowShuffleWhenStuck = false;
        }
        private void OnShuffleCBChecked(object sender, RoutedEventArgs e)
        {
            TSParameters.AllowShuffleWhenStuck = true;
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
            if (int.TryParse(writeOutLimitInput.Text, out limit)) TSParameters.WriteOutLimit = limit;
            else TSParameters.WriteOutLimit = 0;
        }

    }
}
