using FinalExamScheduling.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FinalExamScheduling.TabuSearchScheduling
{
    class NeighbourGeneratorWithVL
    {
        public Context ctx;

        public NeighbourGeneratorWithVL(Context context)
        {
            ctx = context;
        }

        public SolutionCandidate[] GenerateNeighboursHeuristic(SolutionCandidate current)
        {
            int candidateCount = TSParameters.GeneratedCandidates;

            SolutionCandidate[] neighbours = new SolutionCandidate[candidateCount];

            ViolationList VL = current.VL;

            Random rand = new Random();

            for (int i = 0; i < candidateCount; i++)
            {
                neighbours[i] = new SolutionCandidate(current.Clone().Schedule);

                foreach (KeyValuePair<string, string> v in VL.Violations)
                {
                    if (v.Key.Equals(v.Key.Equals("supervisorAvailability")))
                    {
                        string[] data = v.Value.Split(';');
                        int index = int.Parse(data[0]);
                        string name = data[1];
                        if (neighbours[i].Schedule.FinalExams[index].Supervisor.Name.Equals(name))
                        {
                            int x = rand.Next(0, ctx.Students.Length);
                            while (index == x && neighbours[i].Schedule.FinalExams[index].Student.Supervisor.Availability[x] && neighbours[i].Schedule.FinalExams[x].Student.Supervisor.Availability[index]) x = rand.Next(0, ctx.Students.Length);

                            Student temp = neighbours[i].Schedule.FinalExams[index].Student;

                            neighbours[i].Schedule.FinalExams[index].Student = neighbours[i].Schedule.FinalExams[x].Student;
                            neighbours[i].Schedule.FinalExams[x].Student = temp;
                            neighbours[i].Schedule.FinalExams[index].Supervisor = neighbours[i].Schedule.FinalExams[index].Student.Supervisor;
                            neighbours[i].Schedule.FinalExams[x].Supervisor = neighbours[i].Schedule.FinalExams[x].Student.Supervisor;
                        }
                    }
                    else if(v.Key.Equals("studentDuplicated"))
                    {
                        string[] data = v.Value.Split(';');
                        int missingID = -1;
                        int duplicatedID = -1;
                        for(int index = 0;i<ctx.Students.Length;index++)
                        {
                            int currentIDCount = int.Parse(data[i]);
                            if (currentIDCount == 0) missingID = currentIDCount;
                            else if (currentIDCount > 1) duplicatedID = currentIDCount;
                        }
                        if(!(duplicatedID == -1 || missingID == -1))
                        {
                            foreach(FinalExam fe in neighbours[i].Schedule.FinalExams)
                            {
                                if(fe.Student.Id == duplicatedID)
                                {
                                    fe.Student = ctx.GetStudentById(missingID);
                                    fe.Supervisor = fe.Student.Supervisor;
                                }
                            }
                        }
                    }
                    else if (v.Key.Equals("presidentAvailability"))
                    {
                        string[] data = v.Value.Split(';');
                        int index = int.Parse(data[0]);
                        string name = data[1];
                        if (neighbours[i].Schedule.FinalExams[index].President.Name.Equals(name))
                        {
                            int x = 0;
                            while (!ctx.Presidents[x].Availability[index])
                            {
                                if (x < (ctx.Presidents.Length - 1)) x++; 
                            }
                            neighbours[i].Schedule.FinalExams[index].President = ctx.Presidents[x];
                        }

                    }
                    else if (v.Key.Equals("secretaryAvailability"))
                    {
                        string[] data = v.Value.Split(';');
                        int index = int.Parse(data[0]);
                        string name = data[1];
                        if (neighbours[i].Schedule.FinalExams[index].Secretary.Name.Equals(name))
                        {
                            int x = 0;
                            while (!ctx.Secretaries[x].Availability[index])
                            {
                                if (x < (ctx.Secretaries.Length - 1)) x++;
                            }
                            neighbours[i].Schedule.FinalExams[index].Secretary = ctx.Secretaries[x];
                        }
                    }
                    else if(v.Key.Equals("wrongExaminer"))
                    {
                        string[] data = v.Value.Split(';');
                        int index = int.Parse(data[0]);
                        string name = data[1];
                        if (neighbours[i].Schedule.FinalExams[index].Examiner.Name.Equals(name))
                        {
                            int instIndex = 1;
                            Instructor newExaminer = ctx.Instructors[0];
                            while(instIndex < ctx.Instructors.Length && !neighbours[i].Schedule.FinalExams[index].Student.ExamCourse.Instructors.Contains(newExaminer))
                            {
                                newExaminer = ctx.Instructors[instIndex];
                                instIndex++;
                            }
                            if (neighbours[i].Schedule.FinalExams[index].Student.ExamCourse.Instructors.Contains(newExaminer)) neighbours[i].Schedule.FinalExams[index].Examiner = newExaminer;
                        }
                    }
                    else if (v.Key.Equals("examinerAvailability"))
                    {
                        string[] data = v.Value.Split(';');
                        int index = int.Parse(data[0]);
                        string name = data[1];
                        if (neighbours[i].Schedule.FinalExams[index].Examiner.Name.Equals(name))
                        {
                            int x = rand.Next(0, ctx.Instructors.Length);
                            int ctr = 0;
                            int max = 10;
                            while (ctr < max && (!ctx.Instructors[x].Availability[index] || !neighbours[i].Schedule.FinalExams[index].Student.ExamCourse.Instructors.ToArray().Contains(ctx.Instructors[x])))
                            {
                                x = rand.Next(0, ctx.Instructors.Length);
                                ctr++;
                            }
                            // If no eligible examiner is available at the time, switch 2 random students and go on
                            if (ctr >= max)
                            {
                                int y = rand.Next(0, ctx.Students.Length);
                                while (index == y) y = rand.Next(0, ctx.Students.Length);

                                Student temp = neighbours[i].Schedule.FinalExams[index].Student;

                                neighbours[i].Schedule.FinalExams[index].Student = neighbours[i].Schedule.FinalExams[y].Student;
                                neighbours[i].Schedule.FinalExams[y].Student = temp;
                                neighbours[i].Schedule.FinalExams[index].Supervisor = neighbours[i].Schedule.FinalExams[index].Student.Supervisor;
                                neighbours[i].Schedule.FinalExams[y].Supervisor = neighbours[i].Schedule.FinalExams[y].Student.Supervisor;
                            }
                            else neighbours[i].Schedule.FinalExams[index].Examiner = ctx.Instructors[x];
                        }
                    }
                    else if (v.Key.Equals("memberAvailability"))
                    {
                        string[] data = v.Value.Split(';');
                        int index = int.Parse(data[0]);
                        string name = data[1];
                        if (neighbours[i].Schedule.FinalExams[index].Member.Name.Equals(name))
                        {
                            int x = rand.Next(0, ctx.Members.Length);
                            while (!ctx.Members[x].Availability[index])
                            {
                                x = rand.Next(0, ctx.Members.Length);
                            }
                            neighbours[i].Schedule.FinalExams[index].Member = ctx.Members[x];
                        }
                    }

                    else if (v.Key.Equals("presidentChange"))
                    {
                        string[] data = v.Value.Split(';');
                        int index = int.Parse(data[0]);
                        string presidentName = data[1];
                        int offset = int.Parse(data[2]);

                        if (neighbours[i].Schedule.FinalExams[index-offset].President.Name.Equals(presidentName)) neighbours[i].Schedule.FinalExams[index].President = ctx.GetInstructorByName(presidentName);
                    }
                    else if (v.Key.Equals("secretaryChange"))
                    {
                        string[] data = v.Value.Split(';');
                        int index = int.Parse(data[0]);
                        string secretaryName = data[1];
                        int offset = int.Parse(data[2]);

                        if (neighbours[i].Schedule.FinalExams[index-offset].Secretary.Name.Equals(secretaryName)) neighbours[i].Schedule.FinalExams[index].Secretary = ctx.GetInstructorByName(secretaryName);
                    }

                    else if (v.Key.Equals("presidentIsSecretary"))
                    {
                        string[] data = v.Value.Split(';');
                        int index = int.Parse(data[0]);
                        string name = data[1];
                        if (neighbours[i].Schedule.FinalExams[index].President.Name.Equals(name))
                        {
                            int x = rand.Next(0, ctx.Secretaries.Length);
                            while (!ctx.Secretaries[x].Name.Equals(neighbours[i].Schedule.FinalExams[index].Secretary.Name))
                            {
                                x = rand.Next(0, ctx.Secretaries.Length);
                            }

                            neighbours[i].Schedule.FinalExams[index].Secretary = ctx.Secretaries[x];
                        }
                    }
                    else if (v.Key.Equals("presidentIsMember") || v.Key.Equals("secretaryIsMember"))
                    {
                        string[] data = v.Value.Split(';');
                        int index = int.Parse(data[0]);
                        string name = data[1];
                        if (neighbours[i].Schedule.FinalExams[index].President.Name.Equals(name))
                        {
                            int x = rand.Next(0, ctx.Members.Length);
                            while (!ctx.Members[x].Name.Equals(neighbours[i].Schedule.FinalExams[index].Member.Name))
                            {
                                x = rand.Next(0, ctx.Members.Length);
                            }

                            neighbours[i].Schedule.FinalExams[index].Member = ctx.Members[x];
                        }
                    }

                }

                //Trying to fix soft violations only after no hard ones left
                if (TSParameters.OptimizeSoftConstraints && !VL.ContainsHardViolation())
                {
                    foreach (KeyValuePair<string, string> v in VL.Violations)
                    {
                        if (v.Key.Equals("supervisorNotPresident"))
                        {
                            string[] val = v.Value.Split(';');
                            int index = int.Parse(val[0]);
                            string name = val[1];
                            if (neighbours[i].Schedule.FinalExams[index].Supervisor.Name.Equals(name)) neighbours[i].Schedule.FinalExams[index].President = neighbours[i].Schedule.FinalExams[index].Supervisor;
                        }
                        else if (v.Key.Equals("supervisorNotSecretary"))
                        {
                            string[] val = v.Value.Split(';');
                            int index = int.Parse(val[0]);
                            string name = val[1];
                            if (neighbours[i].Schedule.FinalExams[index].Supervisor.Name.Equals(name)) neighbours[i].Schedule.FinalExams[index].Secretary = neighbours[i].Schedule.FinalExams[index].Supervisor;
                        }
                        else if (v.Key.Equals("examinerNotPresident"))
                        {
                            string[] val = v.Value.Split(';');
                            int index = int.Parse(val[0]);
                            string name = val[1];
                            if (neighbours[i].Schedule.FinalExams[index].Examiner.Name.Equals(name)) neighbours[i].Schedule.FinalExams[index].President = neighbours[i].Schedule.FinalExams[index].Examiner;
                        }
                        else if(v.Key.Equals("secretaryNotExaminer"))
                        {
                            string[] val = v.Value.Split(';');
                            int index = int.Parse(val[0]);
                            string name = val[1];
                            if (neighbours[i].Schedule.FinalExams[index].Secretary.Name.Equals(name)) neighbours[i].Schedule.FinalExams[index].Examiner = neighbours[i].Schedule.FinalExams[index].Secretary;
                        }
                        else if (v.Key.Equals("memberNotExaminer"))
                        {
                            string[] val = v.Value.Split(';');
                            int index = int.Parse(val[0]);
                            string name = val[1];
                            if (neighbours[i].Schedule.FinalExams[index].Member.Name.Equals(name)) neighbours[i].Schedule.FinalExams[index].Examiner = neighbours[i].Schedule.FinalExams[index].Member;
                        }
                        else if (v.Key.Equals("supervisorNotExaminer"))
                        {
                            string[] val = v.Value.Split(';');
                            int index = int.Parse(val[0]);
                            string name = val[1];
                            if (neighbours[i].Schedule.FinalExams[index].Supervisor.Name.Equals(name)) neighbours[i].Schedule.FinalExams[index].Examiner = neighbours[i].Schedule.FinalExams[index].Supervisor;
                        }
                        
                        else if (v.Key.Equals("supervisorNotMember"))
                        {
                            string[] val = v.Value.Split(';');
                            int index = int.Parse(val[0]);
                            string name = val[1];
                            if (neighbours[i].Schedule.FinalExams[index].Supervisor.Name.Equals(name)) neighbours[i].Schedule.FinalExams[index].Member = neighbours[i].Schedule.FinalExams[index].Supervisor;
                        }
                        
                        else if (v.Key.Equals("presidentChangeLong"))
                        {
                            string[] data = v.Value.Split(';');
                            int index = int.Parse(data[0]);
                            string name = data[1];
                            if (neighbours[i].Schedule.FinalExams[index-1].President.Name.Equals(name))
                            {
                                neighbours[i].Schedule.FinalExams[index].President = neighbours[i].Schedule.FinalExams[index - 1].President;
                            }
                        }
                        else if (v.Key.Equals("secretaryChangeLong"))
                        {
                            string[] data = v.Value.Split(';');
                            int index = int.Parse(data[0]);
                            string name = data[1];
                            if (neighbours[i].Schedule.FinalExams[index-1].Secretary.Name.Equals(name))
                            {
                                neighbours[i].Schedule.FinalExams[index].Secretary = neighbours[i].Schedule.FinalExams[index - 1].Secretary;
                            }
                        }
                    }
                }

            }
            return neighbours;
        }

        public SolutionCandidate[] GenerateNeighboursRandom(SolutionCandidate current)
        {
            int candidateCount = TSParameters.GeneratedCandidates;

            SolutionCandidate[] neighbours = new SolutionCandidate[candidateCount];

            ViolationList VL = current.VL;

            Random rand = new Random();

            for (int i = 0; i < candidateCount; i++)
            {
                neighbours[i] = new SolutionCandidate(current.Clone().Schedule);

                //Trying to fix soft violations only when there are no hard ones left
                if (TSParameters.OptimizeSoftConstraints && !VL.ContainsHardViolation())
                {
                    foreach (KeyValuePair<string, string> v in VL.Violations)
                    {
                        if (v.Key.Equals("presidentWorkload"))
                        {
                            string[] data = v.Value.Split(';');
                            KeyValuePair<string, int> max = new KeyValuePair<string, int>(data[0], int.Parse(data[1]));
                            KeyValuePair<string, int> min = new KeyValuePair<string, int>(data[2], int.Parse(data[3]));
                            int optimalWorkload = ctx.Students.Length / ctx.Presidents.Length;
                            int maxToOptimal = max.Value - optimalWorkload;
                            int minToOptimal = optimalWorkload - min.Value;
                            int ctr = 0;
                            int target = 0;
                            if (maxToOptimal > 0 && maxToOptimal < minToOptimal)
                            {
                                target = maxToOptimal;
                            }
                            else if (minToOptimal > 0)
                            {
                                target = minToOptimal;
                            }
                            foreach (FinalExam fe in neighbours[i].Schedule.FinalExams)
                            {
                                if (ctr >= target) break;
                                if (fe.President.Name.Equals(max.Key))
                                {
                                    fe.President = ctx.GetInstructorByName(min.Key);
                                    ctr++;
                                }
                            }
                        }
                        else if (v.Key.Equals("secretaryWorkload"))
                        {
                            string[] data = v.Value.Split(';');
                            KeyValuePair<string, int> max = new KeyValuePair<string, int>(data[0], int.Parse(data[1]));
                            KeyValuePair<string, int> min = new KeyValuePair<string, int>(data[2], int.Parse(data[3]));
                            int optimalWorkload = ctx.Students.Length / ctx.Secretaries.Length;
                            int maxToOptimal = max.Value - optimalWorkload;
                            int minToOptimal = optimalWorkload - min.Value;
                            int ctr = 0;
                            int target = 0;
                            if (maxToOptimal > 0 && maxToOptimal < minToOptimal)
                            {
                                target = maxToOptimal;
                            }
                            else if (minToOptimal > 0)
                            {
                                target = minToOptimal;
                            }
                            foreach (FinalExam fe in neighbours[i].Schedule.FinalExams)
                            {
                                if (ctr >= target) break;
                                if (fe.Secretary.Name.Equals(max.Key))
                                {
                                    fe.Secretary = ctx.GetInstructorByName(min.Key);
                                    ctr++;
                                }
                            }
                        }
                        else if (v.Key.Equals("memberWorkload"))
                        {
                            string[] data = v.Value.Split(';');
                            KeyValuePair<string, int> max = new KeyValuePair<string, int>(data[0], int.Parse(data[1]));
                            KeyValuePair<string, int> min = new KeyValuePair<string, int>(data[2], int.Parse(data[3]));
                            int optimalWorkload = ctx.Students.Length / ctx.Members.Length;
                            int maxToOptimal = max.Value - optimalWorkload;
                            int minToOptimal = optimalWorkload - min.Value;
                            int ctr = 0;
                            int target = 0;
                            if (maxToOptimal > 0 && maxToOptimal < minToOptimal)
                            {
                                target = maxToOptimal;
                            }
                            else if (minToOptimal > 0)
                            {
                                target = minToOptimal;
                            }
                            foreach (FinalExam fe in neighbours[i].Schedule.FinalExams)
                            {
                                if (ctr >= target) break;
                                if (fe.Member.Name.Equals(max.Key))
                                {
                                    fe.Member = ctx.GetInstructorByName(min.Key);
                                    ctr++;
                                }
                            }
                        }
                    }
                }

                //Hard violations
                foreach (KeyValuePair<string, string> v in VL.Violations)
                {
                    if (v.Key.Equals(v.Key.Equals("supervisorAvailability")))
                    {
                        string[] data = v.Value.Split(';');
                        int index = int.Parse(data[0]);
                        string name = data[1];
                        if (neighbours[i].Schedule.FinalExams[index].Supervisor.Name.Equals(name))
                        {
                            int x = rand.Next(0, ctx.Students.Length);
                            while (index == x) x = rand.Next(0, ctx.Students.Length);

                            Student temp = neighbours[i].Schedule.FinalExams[index].Student;

                            neighbours[i].Schedule.FinalExams[index].Student = neighbours[i].Schedule.FinalExams[x].Student;
                            neighbours[i].Schedule.FinalExams[x].Student = temp;
                            neighbours[i].Schedule.FinalExams[index].Supervisor = neighbours[i].Schedule.FinalExams[index].Student.Supervisor;
                            neighbours[i].Schedule.FinalExams[x].Supervisor = neighbours[i].Schedule.FinalExams[x].Student.Supervisor;
                            //Console.WriteLine("Swapped: " + neighbours[i].Schedule.FinalExams[index].Student.Name + " - " + neighbours[i].Schedule.FinalExams[x].Student.Name);
                        }
                    }
                    else if (v.Key.Equals("studentDuplicated"))
                    {
                        string[] data = v.Value.Split(';');
                        int missingID = -1;
                        int duplicatedID = -1;
                        for (int index = 0; i < ctx.Students.Length; index++)
                        {
                            int currentIDCount = int.Parse(data[i]);
                            if (currentIDCount == 0) missingID = currentIDCount;
                            else if (currentIDCount > 1) duplicatedID = currentIDCount;
                        }
                        if (!(duplicatedID == -1 || missingID == -1))
                        {
                            foreach (FinalExam fe in neighbours[i].Schedule.FinalExams)
                            {
                                if (fe.Student.Id == duplicatedID)
                                {
                                    fe.Student = ctx.GetStudentById(missingID);
                                    fe.Supervisor = fe.Student.Supervisor;
                                }
                            }
                        }
                    }
                    else if (v.Key.Equals("presidentAvailability"))
                    {
                        string[] data = v.Value.Split(';');
                        int index = int.Parse(data[0]);
                        string name = data[1];
                        if (neighbours[i].Schedule.FinalExams[index].President.Name.Equals(name))
                        {
                            int x = rand.Next(0, ctx.Presidents.Length);
                            while (!ctx.Presidents[x].Availability[index])
                            {
                                x = rand.Next(0, ctx.Presidents.Length);
                            }
                            neighbours[i].Schedule.FinalExams[index].President = ctx.Presidents[x];
                        }

                    }
                    else if (v.Key.Equals("secretaryAvailability"))
                    {
                        string[] data = v.Value.Split(';');
                        int index = int.Parse(data[0]);
                        string name = data[1];
                        if (neighbours[i].Schedule.FinalExams[index].Secretary.Name.Equals(name))
                        {
                            int x = rand.Next(0, ctx.Secretaries.Length);
                            while (!ctx.Secretaries[x].Availability[index])
                            {
                                x = rand.Next(0, ctx.Secretaries.Length);
                            }
                            neighbours[i].Schedule.FinalExams[index].Secretary = ctx.Secretaries[x];
                        }
                    }
                    else if (v.Key.Equals("wrongExaminer"))
                    {
                        string[] data = v.Value.Split(';');
                        int index = int.Parse(data[0]);
                        string name = data[1];
                        if (neighbours[i].Schedule.FinalExams[index].Examiner.Name.Equals(name))
                        {
                            int instIndex = rand.Next(0, ctx.Instructors.Length);
                            Instructor newExaminer = ctx.Instructors[instIndex];
                            while (instIndex < ctx.Instructors.Length && !neighbours[i].Schedule.FinalExams[index].Student.ExamCourse.Instructors.Contains(newExaminer))
                            {
                                newExaminer = ctx.Instructors[instIndex];
                                instIndex = rand.Next(0, ctx.Instructors.Length);
                            }
                            if (neighbours[i].Schedule.FinalExams[index].Student.ExamCourse.Instructors.Contains(newExaminer)) neighbours[i].Schedule.FinalExams[index].Examiner = newExaminer;
                        }
                    }
                    else if (v.Key.Equals("examinerAvailability"))
                    {
                        string[] data = v.Value.Split(';');
                        int index = int.Parse(data[0]);
                        string name = data[1];
                        if (neighbours[i].Schedule.FinalExams[index].Examiner.Name.Equals(name))
                        {
                            int x = rand.Next(0, ctx.Instructors.Length);

                            neighbours[i].Schedule.FinalExams[index].Examiner = ctx.Instructors[x];
                        }
                    }
                    else if (v.Key.Equals("memberAvailability"))
                    {
                        string[] data = v.Value.Split(';');
                        int index = int.Parse(data[0]);
                        string name = data[1];
                        if (neighbours[i].Schedule.FinalExams[index].Member.Name.Equals(name))
                        {
                            int x = rand.Next(0, ctx.Members.Length);

                            neighbours[i].Schedule.FinalExams[index].Member = ctx.Members[x];
                        }
                    }
                    else if (v.Key.Equals("presidentChange"))
                    {
                        string[] data = v.Value.Split(';');
                        int index = int.Parse(data[0]);
                        string presidentName = data[1];
                        int offset = int.Parse(data[2]);

                        if (neighbours[i].Schedule.FinalExams[index - offset].President.Name.Equals(presidentName)) neighbours[i].Schedule.FinalExams[index].President = ctx.GetInstructorByName(presidentName);
                    }
                    else if (v.Key.Equals("secretaryChange"))
                    {
                        string[] data = v.Value.Split(';');
                        int index = int.Parse(data[0]);
                        string secretaryName = data[1];
                        int offset = int.Parse(data[2]);

                        if (neighbours[i].Schedule.FinalExams[index - offset].Secretary.Name.Equals(secretaryName)) neighbours[i].Schedule.FinalExams[index].Secretary = ctx.GetInstructorByName(secretaryName);
                    }
                    else if (v.Key.Equals("presidentIsSecretary"))
                    {
                        string[] data = v.Value.Split(';');
                        int index = int.Parse(data[0]);
                        string name = data[1];
                        if (neighbours[i].Schedule.FinalExams[index].President.Name.Equals(name))
                        {
                            int x = rand.Next(0, ctx.Secretaries.Length);
                            while (!ctx.Secretaries[x].Name.Equals(neighbours[i].Schedule.FinalExams[index].Secretary.Name))
                            {
                                x = rand.Next(0, ctx.Secretaries.Length);
                            }

                            neighbours[i].Schedule.FinalExams[index].Secretary = ctx.Secretaries[x];
                        }
                    }
                    else if (v.Key.Equals("presidentIsMember"))
                    {
                        string[] data = v.Value.Split(';');
                        int index = int.Parse(data[0]);
                        string name = data[1];
                        if (neighbours[i].Schedule.FinalExams[index].President.Name.Equals(name))
                        {
                            int x = rand.Next(0, ctx.Members.Length);
                            while (!ctx.Members[x].Name.Equals(neighbours[i].Schedule.FinalExams[index].Member.Name))
                            {
                                x = rand.Next(0, ctx.Members.Length);
                            }

                            neighbours[i].Schedule.FinalExams[index].Member = ctx.Members[x];
                        }
                    }
                    else if (v.Key.Equals("secretaryIsMember"))
                    {
                        string[] data = v.Value.Split(';');
                        int index = int.Parse(data[0]);
                        string name = data[1];
                        if (neighbours[i].Schedule.FinalExams[index].Secretary.Name.Equals(name))
                        {
                            int x = rand.Next(0, ctx.Members.Length);
                            while (!ctx.Members[x].Name.Equals(neighbours[i].Schedule.FinalExams[index].Member.Name))
                            {
                                x = rand.Next(0, ctx.Members.Length);
                            }

                            neighbours[i].Schedule.FinalExams[index].Member = ctx.Members[x];
                        }
                    }
                }
            }
            return neighbours;
        }
    }
}
