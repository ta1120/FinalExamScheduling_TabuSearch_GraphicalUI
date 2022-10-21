using FinalExamScheduling.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinalExamScheduling.TabuSearchScheduling
{
    class CandidateCostCalculator
    {
        /*
         * 
         * This class is the slightly altered version of the GeneticScheduling/SchedulingFitness class.
         * 
         */

        public Context ctx;

        public readonly List<Func<Schedule, double>> CostFunctions;


        public CandidateCostCalculator(Context context)
        {
            ctx = context;
            CostFunctions = new List<Func<Schedule, double>>()
            {
                GetWrongExaminerScore,
                GetStudentDuplicatedScore,

                GetPresidentNotAvailableScore,
                GetSecretaryNotAvailableScore,
                GetExaminerNotAvailableScore,
                GetMemberNotAvailableScore,
                GetSupervisorNotAvailableScore,
                
                GetPresidentChangeScore,
                GetSecretaryChangeScore,
                GetPresidentChangeLongScore,
                GetSecretaryChangeLongScore,
                
                GetPresidentWorkloadWorstScore,
                GetPresidentWorkloadWorseScore,
                GetPresidentWorkloadBadScore,
                GetSecretaryWorkloadWorstScore,
                GetSecretaryWorkloadWorseScore,
                GetSecretaryWorkloadBadScore,
                GetMemberWorkloadWorstScore,
                GetMemberWorkloadWorseScore,
                GetMemberWorkloadBadScore,
                
                GetSupervisorNotPresidentScore,
                GetSupervisorNotSecretaryScore,
                GetExaminerNotPresidentScore,

                GetPresidentIsSecretaryScore,
                GetPresidentIsMemberScore,
                GetSecretaryIsMemberScore,

                GetSecretaryNotExaminerScore,
                GetMemberNotExaminerScore,
                GetSupervisorNotMemberScore,
                GetSupervisorNotExaminerScore,

                GetWrongSupervisorScore

                

           };
        }

        public double[] GetFinalScores(Schedule sch)
        {
            return CostFunctions.Select(cf => cf(sch)).ToList().ToArray();
        }

        public double Evaluate(SolutionCandidate cand)
        {
            Schedule sch = cand.Schedule;

            int score = 0;

            sch.Details = new FinalExamDetail[100];

            var tasks = CostFunctions.Select(cf => Task.Run(() => cf(sch))).ToArray();
            Task.WaitAll(tasks);
            foreach (var task in tasks)
            {
                score += (int)task.Result;
            }

            cand.Score = score;

            return score;
        }

        public double GetWrongExaminerScore(Schedule sch)
        {
            double score = 0;

            foreach (var fe in sch.FinalExams)
            {
                if (!fe.Student.ExamCourse.Instructors.ToArray().Contains(fe.Examiner)) score += TS_Scores.WrongExaminer;
            }

            return score;
        }

        public double GetStudentDuplicatedScore(Schedule sch)
        {
            double score = 0;
            List<Student> studentBefore = new List<Student>();
            int[] count = new int[100];
            foreach (var fe in sch.FinalExams)
            {
                count[fe.Student.Id]++;
            }
            for (int i = 0; i < 100; i++)
            {
                if (count[i] > 1)
                {
                    score += (count[i] - 1) * TS_Scores.StudentDuplicated;

                }
            }
            return score;
        }

        public double GetPresidentNotAvailableScore(Schedule sch)
        {
            double score = 0;
            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                if (sch.FinalExams[i].President.Availability[i] == false)
                {
                    score += TS_Scores.PresidentNotAvailable;
                    if (ctx.FillDetails)
                    {
                        sch.Details[i].PresidentComment += $"President not available: {TS_Scores.PresidentNotAvailable}\n";
                        sch.Details[i].PresidentScore += TS_Scores.PresidentNotAvailable;
                    }
                }
            }
            return score;
        }

        public double GetSecretaryNotAvailableScore(Schedule sch)
        {
            double score = 0;
            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                if (sch.FinalExams[i].Secretary.Availability[i] == false)
                {
                    score += TS_Scores.SecretaryNotAvailable;
                    if (ctx.FillDetails)
                    {
                        sch.Details[i].SecretaryComment += $"Secretary not available: {TS_Scores.SecretaryNotAvailable}\n";
                        sch.Details[i].SecretaryScore += TS_Scores.SecretaryNotAvailable;
                    }
                }
            }
            return score;
        }

        public double GetExaminerNotAvailableScore(Schedule sch)
        {
            double score = 0;
            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                if (sch.FinalExams[i].Examiner.Availability[i] == false)
                {
                    score += TS_Scores.ExaminerNotAvailable;
                    if (ctx.FillDetails)
                    {
                        sch.Details[i].ExaminerComment += $"Examiner not available: {TS_Scores.ExaminerNotAvailable}\n";
                        sch.Details[i].ExaminerScore += TS_Scores.ExaminerNotAvailable;
                    }
                }


            }
            return score;
        }

        public double GetMemberNotAvailableScore(Schedule sch)
        {
            double score = 0;
            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                if (sch.FinalExams[i].Member.Availability[i] == false)
                {
                    score += TS_Scores.MemberNotAvailable;
                    if (ctx.FillDetails)
                    {
                        sch.Details[i].MemberComment += $"Member not available: {TS_Scores.MemberNotAvailable}\n";
                        sch.Details[i].MemberScore += TS_Scores.MemberNotAvailable;
                    }
                }



            }
            return score;
        }

        public double GetSupervisorNotAvailableScore(Schedule sch)
        {
            double score = 0;
            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                if (sch.FinalExams[i].Supervisor.Availability[i] == false)
                {
                    score += TS_Scores.SupervisorNotAvailable;
                    if (ctx.FillDetails)
                    {
                        sch.Details[i].SupervisorComment += $"Supervisor not available: {TS_Scores.SupervisorNotAvailable}\n";
                        sch.Details[i].SupervisorScore += TS_Scores.SupervisorNotAvailable;
                    }
                }


            }
            return score;
        }

        public double GetPresidentChangeScore(Schedule sch)
        {
            double score = 0;

            for (int i = 0; i < sch.FinalExams.Length; i += 5)
            {
                if (sch.FinalExams[i].President != sch.FinalExams[i + 1].President)
                {
                    score += TS_Scores.PresidentChange;
                    if (ctx.FillDetails)
                    {
                        sch.Details[i + 1].PresidentComment += $"President changed: {TS_Scores.PresidentChange}\n";
                        sch.Details[i + 1].PresidentScore += TS_Scores.PresidentChange;
                    }
                }
                if (sch.FinalExams[i + 1].President != sch.FinalExams[i + 2].President)
                {
                    score += TS_Scores.PresidentChange;
                    if (ctx.FillDetails)
                    {
                        sch.Details[i + 2].PresidentComment += $"President changed: {TS_Scores.PresidentChange}\n";
                        sch.Details[i + 2].PresidentScore += TS_Scores.PresidentChange;
                    }
                }
                if (sch.FinalExams[i + 2].President != sch.FinalExams[i + 3].President)
                {
                    score += TS_Scores.PresidentChange;
                    if (ctx.FillDetails)
                    {
                        sch.Details[i + 3].PresidentComment += $"President changed: {TS_Scores.PresidentChange}\n";
                        sch.Details[i + 3].PresidentScore += TS_Scores.PresidentChange;
                    }
                }
                if (sch.FinalExams[i + 3].President != sch.FinalExams[i + 4].President)
                {
                    score += TS_Scores.PresidentChange;
                    if (ctx.FillDetails)
                    {
                        sch.Details[i + 4].PresidentComment += $"President changed: {TS_Scores.PresidentChange}\n";
                        sch.Details[i + 4].PresidentScore += TS_Scores.PresidentChange;
                    }
                }
            }
            return score;
        }

        public double GetSecretaryChangeScore(Schedule sch)
        {
            double score = 0;

            for (int i = 0; i < sch.FinalExams.Length; i += 5)
            {
                if (sch.FinalExams[i].Secretary != sch.FinalExams[i + 1].Secretary)
                {
                    score += TS_Scores.SecretaryChange;
                    if (ctx.FillDetails)
                    {
                        sch.Details[i + 1].SecretaryComment += $"Secretary changed: {TS_Scores.SecretaryChange}\n";
                        sch.Details[i + 1].SecretaryScore += TS_Scores.SecretaryChange;
                    }
                }
                if (sch.FinalExams[i + 1].Secretary != sch.FinalExams[i + 2].Secretary)
                {
                    score += TS_Scores.SecretaryChange;
                    if (ctx.FillDetails)
                    {
                        sch.Details[i + 2].SecretaryComment += $"Secretary changed: {TS_Scores.SecretaryChange}\n";
                        sch.Details[i + 2].SecretaryScore += TS_Scores.SecretaryChange;
                    }
                }
                if (sch.FinalExams[i + 2].Secretary != sch.FinalExams[i + 3].Secretary)
                {
                    score += TS_Scores.SecretaryChange;
                    if (ctx.FillDetails)
                    {
                        sch.Details[i + 3].SecretaryComment += $"Secretary changed: {TS_Scores.SecretaryChange}\n";
                        sch.Details[i + 3].SecretaryScore += TS_Scores.SecretaryChange;
                    }
                }
                if (sch.FinalExams[i + 3].Secretary != sch.FinalExams[i + 4].Secretary)
                {
                    score += TS_Scores.SecretaryChange;
                    if (ctx.FillDetails)
                    {
                        sch.Details[i + 4].SecretaryComment += $"Secretary changed: {TS_Scores.SecretaryChange}\n";
                        sch.Details[i + 4].SecretaryScore += TS_Scores.SecretaryChange;
                    }
                }

            }

            return score;
        }

        public double GetPresidentWorkloadWorstScore(Schedule schedule)
        {
            double score = 0;
            int[] presidentWorkloads = new int[ctx.Presidents.Length];

            foreach (FinalExam fi in schedule.FinalExams)
            {
                presidentWorkloads[Array.IndexOf(ctx.Presidents, fi.President)]++;
            }

            double optimalWorkload = 100 / ctx.Presidents.Length;

            foreach (Instructor pres in ctx.Presidents)
            {
                if (presidentWorkloads[Array.IndexOf(ctx.Presidents, pres)] < optimalWorkload * 0.5)
                {
                    score += TS_Scores.PresidentWorkloadWorst;
                }

                if (presidentWorkloads[Array.IndexOf(ctx.Presidents, pres)] > optimalWorkload * 1.5)
                {
                    score += TS_Scores.PresidentWorkloadWorst;
                }

            }

            return score;
        }

        public double GetPresidentWorkloadWorseScore(Schedule schedule)
        {
            double score = 0;
            int[] presidentWorkloads = new int[ctx.Presidents.Length];

            foreach (FinalExam fi in schedule.FinalExams)
            {
                presidentWorkloads[Array.IndexOf(ctx.Presidents, fi.President)]++;
            }


            double optimalWorkload = 100 / ctx.Presidents.Length;

            foreach (Instructor pres in ctx.Presidents)
            {

                if (presidentWorkloads[Array.IndexOf(ctx.Presidents, pres)] < optimalWorkload * 0.7 && presidentWorkloads[Array.IndexOf(ctx.Presidents, pres)] >= optimalWorkload * 0.5)
                {
                    score += TS_Scores.PresidentWorkloadWorse;
                }

                if (presidentWorkloads[Array.IndexOf(ctx.Presidents, pres)] > optimalWorkload * 1.3 && presidentWorkloads[Array.IndexOf(ctx.Presidents, pres)] <= optimalWorkload * 1.5)
                {
                    score += TS_Scores.PresidentWorkloadWorse;
                }

            }

            return score;
        }

        public double GetPresidentWorkloadBadScore(Schedule schedule)
        {
            double score = 0;
            int[] presidentWorkloads = new int[ctx.Presidents.Length];

            foreach (FinalExam fi in schedule.FinalExams)
            {
                presidentWorkloads[Array.IndexOf(ctx.Presidents, fi.President)]++;
            }


            double optimalWorkload = 100 / ctx.Presidents.Length;

            foreach (Instructor pres in ctx.Presidents)
            {

                if (presidentWorkloads[Array.IndexOf(ctx.Presidents, pres)] < optimalWorkload * 0.9 && presidentWorkloads[Array.IndexOf(ctx.Presidents, pres)] >= optimalWorkload * 0.7)
                {
                    score += TS_Scores.PresidentWorkloadBad;
                }

                if (presidentWorkloads[Array.IndexOf(ctx.Presidents, pres)] > optimalWorkload * 1.1 && presidentWorkloads[Array.IndexOf(ctx.Presidents, pres)] <= optimalWorkload * 1.3)
                {
                    score += TS_Scores.PresidentWorkloadBad;
                }
            }

            return score;
        }

        public double GetSecretaryWorkloadWorstScore(Schedule schedule)
        {
            double score = 0;
            int[] secretaryWorkloads = new int[ctx.Secretaries.Length];

            foreach (FinalExam fi in schedule.FinalExams)
            {
                secretaryWorkloads[Array.IndexOf(ctx.Secretaries, fi.Secretary)]++;
            }

            double optimalWorkload = 100 / ctx.Secretaries.Length;

            foreach (Instructor secr in ctx.Secretaries)
            {
                if (secretaryWorkloads[Array.IndexOf(ctx.Secretaries, secr)] < optimalWorkload * 0.5)
                {
                    score += TS_Scores.SecretaryWorkloadWorst;
                }

                if (secretaryWorkloads[Array.IndexOf(ctx.Secretaries, secr)] > optimalWorkload * 1.5)
                {
                    score += TS_Scores.SecretaryWorkloadWorst;
                }

            }

            return score;
        }

        public double GetSecretaryWorkloadWorseScore(Schedule schedule)
        {
            double score = 0;
            int[] secretaryWorkloads = new int[ctx.Secretaries.Length];

            foreach (FinalExam fi in schedule.FinalExams)
            {
                secretaryWorkloads[Array.IndexOf(ctx.Secretaries, fi.Secretary)]++;
            }


            double optimalWorkload = 100 / ctx.Secretaries.Length;

            foreach (Instructor secr in ctx.Secretaries)
            {

                if (secretaryWorkloads[Array.IndexOf(ctx.Secretaries, secr)] < optimalWorkload * 0.7 && secretaryWorkloads[Array.IndexOf(ctx.Secretaries, secr)] >= optimalWorkload * 0.5)
                {
                    score += TS_Scores.SecretaryWorkloadWorse;
                }

                if (secretaryWorkloads[Array.IndexOf(ctx.Secretaries, secr)] > optimalWorkload * 1.3 && secretaryWorkloads[Array.IndexOf(ctx.Secretaries, secr)] <= optimalWorkload * 1.5)
                {
                    score += TS_Scores.SecretaryWorkloadWorse;
                }

            }

            return score;
        }

        public double GetSecretaryWorkloadBadScore(Schedule schedule)
        {
            double score = 0;
            int[] secretaryWorkloads = new int[ctx.Secretaries.Length];

            foreach (FinalExam fi in schedule.FinalExams)
            {
                secretaryWorkloads[Array.IndexOf(ctx.Secretaries, fi.Secretary)]++;
            }


            double optimalWorkload = 100 / ctx.Secretaries.Length;

            foreach (Instructor secr in ctx.Secretaries)
            {

                if (secretaryWorkloads[Array.IndexOf(ctx.Secretaries, secr)] < optimalWorkload * 0.9 && secretaryWorkloads[Array.IndexOf(ctx.Secretaries, secr)] >= optimalWorkload * 0.7)
                {
                    score += TS_Scores.SecretaryWorkloadBad;
                }

                if (secretaryWorkloads[Array.IndexOf(ctx.Secretaries, secr)] > optimalWorkload * 1.1 && secretaryWorkloads[Array.IndexOf(ctx.Secretaries, secr)] <= optimalWorkload * 1.3)
                {
                    score += TS_Scores.SecretaryWorkloadBad;
                }
            }

            return score;
        }

        public double GetMemberWorkloadWorstScore(Schedule schedule)
        {
            double score = 0;
            int[] memberWorkloads = new int[ctx.Members.Length];

            foreach (FinalExam fi in schedule.FinalExams)
            {
                memberWorkloads[Array.IndexOf(ctx.Members, fi.Member)]++;
            }

            double optimalWorkload = 100 / ctx.Members.Length;

            foreach (Instructor memb in ctx.Members)
            {
                if (memberWorkloads[Array.IndexOf(ctx.Members, memb)] < optimalWorkload * 0.5)
                {
                    score += TS_Scores.MemberWorkloadWorst;
                }

                if (memberWorkloads[Array.IndexOf(ctx.Members, memb)] > optimalWorkload * 1.5)
                {
                    score += TS_Scores.MemberWorkloadWorst;
                }

            }

            return score;
        }

        public double GetMemberWorkloadWorseScore(Schedule schedule)
        {
            double score = 0;
            int[] memberWorkloads = new int[ctx.Members.Length];

            foreach (FinalExam fi in schedule.FinalExams)
            {
                memberWorkloads[Array.IndexOf(ctx.Members, fi.Member)]++;
            }


            double optimalWorkload = 100 / ctx.Members.Length;

            foreach (Instructor memb in ctx.Members)
            {

                if (memberWorkloads[Array.IndexOf(ctx.Members, memb)] < optimalWorkload * 0.7 && memberWorkloads[Array.IndexOf(ctx.Members, memb)] >= optimalWorkload * 0.5)
                {
                    score += TS_Scores.MemberWorkloadWorse;
                }

                if (memberWorkloads[Array.IndexOf(ctx.Members, memb)] > optimalWorkload * 1.3 && memberWorkloads[Array.IndexOf(ctx.Members, memb)] <= optimalWorkload * 1.5)
                {
                    score += TS_Scores.MemberWorkloadWorse;
                }

            }

            return score;
        }

        public double GetMemberWorkloadBadScore(Schedule schedule)
        {
            double score = 0;
            int[] memberWorkloads = new int[ctx.Members.Length];

            foreach (FinalExam fi in schedule.FinalExams)
            {
                memberWorkloads[Array.IndexOf(ctx.Members, fi.Member)]++;
            }

            double optimalWorkload = 100 / ctx.Members.Length;

            foreach (Instructor memb in ctx.Members)
            {

                if (memberWorkloads[Array.IndexOf(ctx.Members, memb)] < optimalWorkload * 0.9 && memberWorkloads[Array.IndexOf(ctx.Members, memb)] >= optimalWorkload * 0.7)
                {
                    score += TS_Scores.MemberWorkloadBad;
                }

                if (memberWorkloads[Array.IndexOf(ctx.Members, memb)] > optimalWorkload * 1.1 && memberWorkloads[Array.IndexOf(ctx.Members, memb)] <= optimalWorkload * 1.3)
                {
                    score += TS_Scores.MemberWorkloadBad;
                }
            }
            return score;
        }

        public double GetSupervisorNotPresidentScore(Schedule sch)
        {
            double score = 0;
            foreach (var fi in sch.FinalExams)
            {
                if ((fi.Supervisor.Roles & Roles.President) == Roles.President && fi.Supervisor != fi.President)
                {
                    score += TS_Scores.SupervisorNotPresident;
                    if (ctx.FillDetails)
                    {
                        sch.Details[Array.IndexOf(sch.FinalExams, fi)].SupervisorComment += $"Not President: {TS_Scores.SupervisorNotPresident}\n";
                        sch.Details[Array.IndexOf(sch.FinalExams, fi)].SupervisorScore += TS_Scores.SupervisorNotPresident;
                    }
                }
            }
            return score;
        }

        public double GetSupervisorNotSecretaryScore(Schedule sch)
        {
            double score = 0;
            foreach (var fi in sch.FinalExams)
            {
                if ((fi.Supervisor.Roles & Roles.Secretary) == Roles.Secretary && fi.Supervisor != fi.Secretary)
                {
                    score += TS_Scores.SupervisorNotSecretary;
                    if (ctx.FillDetails)
                    {
                        sch.Details[Array.IndexOf(sch.FinalExams, fi)].SupervisorComment += $"Not Secretary: {TS_Scores.SupervisorNotSecretary}\n";
                        sch.Details[Array.IndexOf(sch.FinalExams, fi)].SupervisorScore += TS_Scores.SupervisorNotSecretary;
                    }
                }
            }
            return score;
        }

        public double GetExaminerNotPresidentScore(Schedule sch)
        {
            double score = 0;
            foreach (var fi in sch.FinalExams)
            {
                if ((fi.Examiner.Roles & Roles.President) == Roles.President && fi.Examiner != fi.President)
                {
                    score += TS_Scores.ExaminerNotPresident;
                    if (ctx.FillDetails)
                    {
                        sch.Details[Array.IndexOf(sch.FinalExams, fi)].ExaminerComment += $"Not President: {TS_Scores.ExaminerNotPresident}\n";
                        sch.Details[Array.IndexOf(sch.FinalExams, fi)].ExaminerScore += TS_Scores.ExaminerNotPresident;
                    }
                }
            }
            return score;
        }
        
        public double GetPresidentIsSecretaryScore(Schedule schedule)
        {
            double score = 0;

            foreach (FinalExam fi in schedule.FinalExams)
            {
                if (fi.President.Name.Equals(fi.Secretary.Name)) score += TS_Scores.PresidentIsSecretary;
            }

            return score;
        }

        public double GetPresidentIsMemberScore(Schedule schedule)
        {
            double score = 0;

            foreach (FinalExam fi in schedule.FinalExams)
            {
                if (fi.President.Name.Equals(fi.Member.Name)) score += TS_Scores.PresidentIsMember;
            }

            return score;
        }

        public double GetSecretaryIsMemberScore(Schedule schedule)
        {
            double score = 0;

            foreach (FinalExam fi in schedule.FinalExams)
            {
                if (fi.Secretary.Name.Equals(fi.Member.Name)) score += TS_Scores.SecretaryIsMember;
            }

            return score;
        }

        public double GetSecretaryNotExaminerScore(Schedule schedule)
        {
            double score = 0;

            foreach (FinalExam fi in schedule.FinalExams)
            {
                if (!fi.Secretary.Name.Equals(fi.Examiner.Name) && fi.Student.ExamCourse.Instructors.Contains(fi.Secretary)) score += TS_Scores.SecretaryNotExaminer;
            }

            return score;
        }

        public double GetMemberNotExaminerScore(Schedule schedule)
        {
            double score = 0;

            foreach (FinalExam fi in schedule.FinalExams)
            {
                if (!fi.Member.Name.Equals(fi.Examiner.Name) && fi.Student.ExamCourse.Instructors.Contains(fi.Member)) score += TS_Scores.MemberNotExaminer;
            }

            return score;
        }

        public double GetSupervisorNotExaminerScore(Schedule schedule)
        {
            double score = 0;

            foreach (FinalExam fi in schedule.FinalExams)
            {
                if (!fi.Supervisor.Name.Equals(fi.Examiner.Name) && fi.Student.ExamCourse.Instructors.Contains(fi.Supervisor)) score += TS_Scores.SupervisorNotExaminer;
            }

            return score;
        }

        public double GetSupervisorNotMemberScore(Schedule schedule)
        {
            double score = 0;

            foreach (FinalExam fi in schedule.FinalExams)
            {
                if (!fi.Supervisor.Name.Equals(fi.Member.Name) && ctx.Members.Contains(fi.Supervisor)) score += TS_Scores.SupervisorNotMember;
            }

            return score;
        }
        public double GetWrongSupervisorScore(Schedule schedule)
        {
            double score = 0;

            foreach (FinalExam fi in schedule.FinalExams)
            {
                if (!fi.Supervisor.Name.Equals(fi.Student.Supervisor.Name)) score += TS_Scores.WrongSupervisor;
            }

            return score;
        }
        public double GetPresidentChangeLongScore(Schedule schedule)
        {
            double score = 0;
            int ctr = 1;
            Instructor currentPresident = schedule.FinalExams[0].President;

            for(int i = 0; i < schedule.FinalExams.Length; i++)
            {
                if(ctr > 10)
                {
                    ctr = 1;
                    currentPresident = schedule.FinalExams[i].President;
                }
                else
                {
                    ctr++;
                    if (schedule.FinalExams[i].President.Name.Equals(currentPresident.Name)) score += TS_Scores.PresidentChangeLong;
                }

            }

            return score;
        }
        public double GetSecretaryChangeLongScore(Schedule schedule)
        {
            double score = 0;
            int ctr = 1;
            Instructor currentSecretary = schedule.FinalExams[0].Secretary;

            for (int i = 0; i < schedule.FinalExams.Length; i++)
            {
                if (ctr > 10)
                {
                    ctr = 1;
                    currentSecretary = schedule.FinalExams[i].Secretary;
                }
                else
                {
                    ctr++;
                    if (schedule.FinalExams[i].Secretary.Name.Equals(currentSecretary.Name)) score += TS_Scores.SecretaryChangeLong;
                }

            }

            return score;
        }



    }
}