using FinalExamScheduling.Model;
using FinalExamScheduling.TabuSearchScheduling;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Threading;

namespace FinalExamScheduling
{
    public partial class MainForm : Form
    {
        private Button RunButton;
        static TabuSearchScheduler scheduler;
        private Button AbortButton;

        bool algorithmRunning = false;
        private ListBox resultList;
        Thread algorithmThread;

        public MainForm() { InitializeComponent(); }

        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Console.WriteLine("Algoritm ready to launch, use the graphical interface to control the program...");
            Application.Run(new MainForm());

            System.Environment.Exit(0);

        }

        //Based on the RunGenetic() function
        public void RunTabuSearch()
        {
            resultList.Invoke(new Action(() => resultList.Items.Add("# "+(DateTime.Now.ToString()))));
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
                    resultList.Invoke(new Action(() => resultList.Items.Add(penaltyScore)));
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

        private void InitializeComponent()
        {
            this.RunButton = new System.Windows.Forms.Button();
            this.AbortButton = new System.Windows.Forms.Button();
            this.resultList = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // RunButton
            // 
            this.RunButton.Location = new System.Drawing.Point(12, 12);
            this.RunButton.Name = "RunButton";
            this.RunButton.Size = new System.Drawing.Size(75, 23);
            this.RunButton.TabIndex = 0;
            this.RunButton.Text = "Run";
            this.RunButton.UseVisualStyleBackColor = true;
            this.RunButton.Click += new System.EventHandler(this.Run_Click);
            // 
            // AbortButton
            // 
            this.AbortButton.Location = new System.Drawing.Point(93, 12);
            this.AbortButton.Name = "AbortButton";
            this.AbortButton.Size = new System.Drawing.Size(75, 23);
            this.AbortButton.TabIndex = 1;
            this.AbortButton.Text = "Abort";
            this.AbortButton.UseVisualStyleBackColor = true;
            this.AbortButton.Click += new System.EventHandler(this.AbortButton_Click);
            // 
            // resultList
            // 
            this.resultList.FormattingEnabled = true;
            this.resultList.Location = new System.Drawing.Point(12, 41);
            this.resultList.Name = "resultList";
            this.resultList.Size = new System.Drawing.Size(156, 264);
            this.resultList.TabIndex = 3;
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(537, 316);
            this.Controls.Add(this.resultList);
            this.Controls.Add(this.AbortButton);
            this.Controls.Add(this.RunButton);
            this.Name = "MainForm";
            this.Text = "FinalExamScheduling - TabuSearch";
            this.ResumeLayout(false);

        }

        private void Run_Click(object sender, EventArgs e)
        {
            if(algorithmRunning)
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
            AbortButton.Enabled = true;
            algorithmThread.Start();
        }

        private void AbortButton_Click(object sender, EventArgs e)
        {
            if (algorithmRunning)
            {
                algorithmThread.Abort();
                algorithmRunning = false;
                AbortButton.Enabled = false;
            }
           
        }
    }
}
