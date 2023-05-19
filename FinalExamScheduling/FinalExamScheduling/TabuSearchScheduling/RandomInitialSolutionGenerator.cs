using FinalExamScheduling.Model;
using System;

namespace FinalExamScheduling.TabuSearchScheduling
{
    //Static class, used for generating a random initial SolutionCandidate, based on the Context (input data)
    static class RandomInitialSolutionGenerator
    {
        public static SolutionCandidate GenerateInitialSolution(Context ctx)
        {
            Random rnd = new Random();

            //The number of exams is given by the number of students in the input
            int examCount = ctx.Students.Length;

            //First, we create a schedule to store the exams
            Schedule randomSchedule = new Schedule(examCount);

            //This is used to make sure every student is only scheduled for one exam
            bool[] studentUsed = new bool[examCount];

            for (int i = 0; i < examCount; i++)
            {
                //Creating a FinalExam object for each exam in the schedule
                randomSchedule.FinalExams[i] = new FinalExam();

                //Randomly picking students until one is found who is not yet scheduled for an exam
                int x = rnd.Next(0, ctx.Students.Length);
                while (studentUsed[x]) x = rnd.Next(0, ctx.Students.Length);

                studentUsed[x] = true;

                //The student and the supervisor are scheduled together automatically
                randomSchedule.FinalExams[i].Student = ctx.Students[x];
                randomSchedule.FinalExams[i].Supervisor = randomSchedule.FinalExams[i].Student.Supervisor;

                //The other instructors are picked randomly from the Context
                randomSchedule.FinalExams[i].President = ctx.Presidents[rnd.Next(0, ctx.Presidents.Length)];
                randomSchedule.FinalExams[i].Secretary = ctx.Secretaries[rnd.Next(0, ctx.Secretaries.Length)];
                randomSchedule.FinalExams[i].Member = ctx.Members[rnd.Next(0, ctx.Members.Length)];
                randomSchedule.FinalExams[i].Examiner = ctx.Instructors[rnd.Next(0, ctx.Instructors.Length)];
            }

            //The returned value is a SolutionCandidate (Schedule + Cost + ViolationList), created from the random schedule
            return new SolutionCandidate(randomSchedule);
        }
    }
}