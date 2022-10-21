using FinalExamScheduling.Model;
using System;

namespace FinalExamScheduling.TabuSearchScheduling
{
    class RandomInitialSolutionGenerator
    {
        public Context ctx;

        public RandomInitialSolutionGenerator(Context context)
        {
            ctx = context;
        }

        public SolutionCandidate generateInitialSolution()
        {
            Random rnd = new Random();

            int examCount = ctx.Students.Length;

            Schedule randomSchedule = new Schedule(examCount);
            bool[] studentUsed = new bool[examCount];

            if (TSParameters.PrintDetails) Console.WriteLine("Generating Random Initial Solution\n");

            for (int i = 0; i < examCount; i++)
            {
                randomSchedule.FinalExams[i] = new FinalExam();

                int x = rnd.Next(0, ctx.Students.Length);
                while (studentUsed[x] /*|| !ctx.Students[x].Supervisor.Availability[i]*/) x = rnd.Next(0, ctx.Students.Length);

                studentUsed[x] = true;

                randomSchedule.FinalExams[i].setStudent(ctx.Students[x]);

                randomSchedule.FinalExams[i].Supervisor = randomSchedule.FinalExams[i].Student.Supervisor;

                randomSchedule.FinalExams[i].President = ctx.Presidents[rnd.Next(0, ctx.Presidents.Length)];

                randomSchedule.FinalExams[i].Secretary = ctx.Secretaries[rnd.Next(0, ctx.Secretaries.Length)];

                randomSchedule.FinalExams[i].Member = ctx.Members[rnd.Next(0, ctx.Members.Length)];

                randomSchedule.FinalExams[i].Examiner = ctx.Instructors[rnd.Next(0, ctx.Instructors.Length)];
            }

            return new SolutionCandidate(randomSchedule);
            //throw new NotImplementedException();
        }
    }
}
