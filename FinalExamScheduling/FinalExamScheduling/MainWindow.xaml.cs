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
                    if (ctr % 10 == 0) avgLabel.Dispatcher.Invoke(new Action(() => avgLabel.Content = avg + " points")) ;
                    if (!TSParameters.MuteConsoleUnlessDone) Console.WriteLine("Best penalty score reached: " + penaltyScore);
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

            Console.WriteLine("Best penalty score: " + penaltyScore);

            //solution.VL.printViolations();

            string extraInfo = ("_" + TSParameters.Mode + "_" + penaltyScore);

            ExcelHelper.WriteTS(@"..\..\Results\Done_TS_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + extraInfo + ".xlsx", resultSchedule, context, new CandidateCostCalculator(context).GetFinalScores(resultSchedule), iterationProgress, elapsed);
            Console.WriteLine("Done");
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
            algorithmThread.Start();
        }

        private void Abort_Click(object sender, EventArgs e)
        {
            if (algorithmRunning)
            {
                algorithmThread.Abort();
                algorithmRunning = false;
                abortButton.IsEnabled = false;
            }
        }
    }
}
