using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using FinalExamScheduling.Model;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;

namespace FinalExamScheduling.TabuSearchScheduling
{
    class NeighbourGeneratorWithVL
    {
        private Context ctx;

        public NeighbourGeneratorWithVL(Context context)
        {
            ctx = context;
        }

        public SolutionCandidate[] GenerateNeighbours(SolutionCandidate candidate, string mode)
        {
            int candidateCount = TSParameters.GeneratedCandidates;
            SolutionCandidate[] neighbours = new SolutionCandidate[candidateCount];

            for (int i = 0; candidateCount > i; i++)
            {
                ViolationList partialViolations = GetRandomViolationSubset(candidate.vl);

                if(mode.Equals("Random"))
                {
                    neighbours[i] = FixViolations_Random(candidate.Clone(),partialViolations);
                }
                else if(mode.Equals("Heuristic"))
                {
                    neighbours[i] = FixViolations_Heuristic(candidate.Clone(),partialViolations);
                }
            }

            return neighbours;
        }

        public ViolationList GetRandomViolationSubset(ViolationList violations)
        {
            int modCount = TSParameters.ViolationsToFixPerGeneration;

            int violationCount = violations.Violations.Count;

            if (modCount >= violationCount) return violations;

            ViolationList partialViolations  = new ViolationList();

            foreach (KeyValuePair<string, string> v in violations.Violations)
            {
                if (1 > modCount) break;

                bool decision = RollForSelectionSampling(modCount, violationCount);

                if(decision)
                {
                    partialViolations.AddViolation(v.Key, v.Value);
                    modCount--;
                }

                violationCount--;
            }

            return partialViolations;
        }

        //Selection sampling method
        public bool RollForSelectionSampling(int itemsNeeded, int itemsLeft)
        {
            Random rand = new Random();
            int num = rand.Next(itemsLeft);

            return itemsNeeded >= num;
        }

        //TODO Review / Restructure
        public SolutionCandidate FixViolations_Heuristic(SolutionCandidate candidate, ViolationList partialViolations) 
        {
            Random rand = new Random();
            foreach (KeyValuePair<string, string> v in partialViolations.Violations)
            {
                if (v.Key.Equals("supervisorAvailability"))
                {
                    string[] data = v.Value.Split(';');
                    int index = int.Parse(data[0]);
                    string name = data[1];
                    if (candidate.schedule.FinalExams[index].Supervisor.Name.Equals(name) || !TSParameters.CheckViolationPersistance)
                    {
                        int x = rand.Next(0, ctx.Students.Length);
                        while (index == x || !candidate.schedule.FinalExams[index].Student.Supervisor.Availability[x] || !candidate.schedule.FinalExams[x].Student.Supervisor.Availability[index]) x = rand.Next(0, ctx.Students.Length);

                        Student temp = candidate.schedule.FinalExams[index].Student;

                        candidate.schedule.FinalExams[index].Student = candidate.schedule.FinalExams[x].Student;
                        candidate.schedule.FinalExams[x].Student = temp;
                        candidate.schedule.FinalExams[index].Supervisor = candidate.schedule.FinalExams[index].Student.Supervisor;
                        candidate.schedule.FinalExams[x].Supervisor = candidate.schedule.FinalExams[x].Student.Supervisor;
                    }
                }
                else if (v.Key.Equals("studentDuplicated"))
                {
                    string[] data = v.Value.Split(';');
                    int missingID = -1;
                    int duplicatedID = -1;
                    for (int index = 0; index < ctx.Students.Length; index++)
                    {
                        int currentIDCount = int.Parse(data[index]);
                        if (currentIDCount == 0) missingID = currentIDCount;
                        else if (currentIDCount > 1) duplicatedID = currentIDCount;
                    }
                    if (!(duplicatedID == -1 || missingID == -1))
                    {
                        foreach (FinalExam fe in candidate.schedule.FinalExams)
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
                    if (candidate.schedule.FinalExams[index].President.Name.Equals(name) || !TSParameters.CheckViolationPersistance)
                    {
                        int x = 0;
                        while (!ctx.Presidents[x].Availability[index])
                        {
                            if (x < (ctx.Presidents.Length - 1)) x++;
                        }
                        candidate.schedule.FinalExams[index].President = ctx.Presidents[x];
                    }

                }
                else if (v.Key.Equals("secretaryAvailability"))
                {
                    string[] data = v.Value.Split(';');
                    int index = int.Parse(data[0]);
                    string name = data[1];
                    if (candidate.schedule.FinalExams[index].Secretary.Name.Equals(name) || !TSParameters.CheckViolationPersistance)
                    {
                        int x = 0;
                        while (!ctx.Secretaries[x].Availability[index])
                        {
                            if (x < (ctx.Secretaries.Length - 1)) x++;
                        }
                        candidate.schedule.FinalExams[index].Secretary = ctx.Secretaries[x];
                    }
                }
                else if (v.Key.Equals("wrongExaminer"))
                {
                    string[] data = v.Value.Split(';');
                    int index = int.Parse(data[0]);
                    string name = data[1];
                    if (candidate.schedule.FinalExams[index].Examiner.Name.Equals(name) || !TSParameters.CheckViolationPersistance)
                    {
                        int instIndex = 1;
                        Instructor newExaminer = ctx.Instructors[0];
                        while (instIndex < ctx.Instructors.Length && !candidate.schedule.FinalExams[index].Student.ExamCourse.Instructors.Contains(newExaminer))
                        {
                            newExaminer = ctx.Instructors[instIndex];
                            instIndex++;
                        }
                        if (candidate.schedule.FinalExams[index].Student.ExamCourse.Instructors.Contains(newExaminer)) candidate.schedule.FinalExams[index].Examiner = newExaminer;
                    }
                }
                else if (v.Key.Equals("examinerAvailability"))
                {
                    string[] data = v.Value.Split(';');
                    int index = int.Parse(data[0]);
                    string name = data[1];
                    if (candidate.schedule.FinalExams[index].Examiner.Name.Equals(name) || !TSParameters.CheckViolationPersistance)
                    {
                        int x = rand.Next(0, ctx.Instructors.Length);
                        int ctr = 0;
                        int max = 10;
                        while (ctr < max && (!ctx.Instructors[x].Availability[index] || !candidate.schedule.FinalExams[index].Student.ExamCourse.Instructors.ToArray().Contains(ctx.Instructors[x])))
                        {
                            x = rand.Next(0, ctx.Instructors.Length);
                            ctr++;
                        }
                        // If no eligible examiner is available at the time, switch 2 random students and go on
                        if (ctr >= max)
                        {
                            int y = rand.Next(0, ctx.Students.Length);
                            while (index == y) y = rand.Next(0, ctx.Students.Length);

                            Student temp = candidate.schedule.FinalExams[index].Student;

                            candidate.schedule.FinalExams[index].Student = candidate.schedule.FinalExams[y].Student;
                            candidate.schedule.FinalExams[y].Student = temp;
                            candidate.schedule.FinalExams[index].Supervisor = candidate.schedule.FinalExams[index].Student.Supervisor;
                            candidate.schedule.FinalExams[y].Supervisor = candidate.schedule.FinalExams[y].Student.Supervisor;
                        }
                        else candidate.schedule.FinalExams[index].Examiner = ctx.Instructors[x];
                    }
                }
                else if (v.Key.Equals("memberAvailability"))
                {
                    string[] data = v.Value.Split(';');
                    int index = int.Parse(data[0]);
                    string name = data[1];
                    if (candidate.schedule.FinalExams[index].Member.Name.Equals(name) || !TSParameters.CheckViolationPersistance)
                    {
                        int x = rand.Next(0, ctx.Members.Length);
                        while (!ctx.Members[x].Availability[index])
                        {
                            x = rand.Next(0, ctx.Members.Length);
                        }
                        candidate.schedule.FinalExams[index].Member = ctx.Members[x];
                    }
                }

                else if (v.Key.Equals("presidentChange"))
                {
                    string[] data = v.Value.Split(';');
                    int index = int.Parse(data[0]);
                    string presidentName = data[1];
                    int offset = int.Parse(data[2]);

                    if (candidate.schedule.FinalExams[index - offset].President.Name.Equals(presidentName) || !TSParameters.CheckViolationPersistance) candidate.schedule.FinalExams[index].President = ctx.GetInstructorByName(presidentName);
                }
                else if (v.Key.Equals("secretaryChange"))
                {
                    string[] data = v.Value.Split(';');
                    int index = int.Parse(data[0]);
                    string secretaryName = data[1];
                    int offset = int.Parse(data[2]);

                    if (candidate.schedule.FinalExams[index - offset].Secretary.Name.Equals(secretaryName) || !TSParameters.CheckViolationPersistance) candidate.schedule.FinalExams[index].Secretary = ctx.GetInstructorByName(secretaryName);
                }

                else if (v.Key.Equals("presidentIsSecretary"))
                {
                    string[] data = v.Value.Split(';');
                    int index = int.Parse(data[0]);
                    string name = data[1];
                    if (candidate.schedule.FinalExams[index].President.Name.Equals(name) || !TSParameters.CheckViolationPersistance)
                    {
                        int x = rand.Next(0, ctx.Secretaries.Length);
                        while (!ctx.Secretaries[x].Name.Equals(candidate.schedule.FinalExams[index].Secretary.Name))
                        {
                            x = rand.Next(0, ctx.Secretaries.Length);
                        }

                        candidate.schedule.FinalExams[index].Secretary = ctx.Secretaries[x];
                    }
                }
                else if (v.Key.Equals("presidentIsMember") || v.Key.Equals("secretaryIsMember"))
                {
                    string[] data = v.Value.Split(';');
                    int index = int.Parse(data[0]);
                    string name = data[1];
                    if (candidate.schedule.FinalExams[index].President.Name.Equals(name) || !TSParameters.CheckViolationPersistance)
                    {
                        int x = rand.Next(0, ctx.Members.Length);
                        while (!ctx.Members[x].Name.Equals(candidate.schedule.FinalExams[index].Member.Name))
                        {
                            x = rand.Next(0, ctx.Members.Length);
                        }

                        candidate.schedule.FinalExams[index].Member = ctx.Members[x];
                    }
                }
            }

            //Trying to fix soft violations only after no hard ones left
            if (TSParameters.OptimizeSoftConstraints && (!partialViolations.ContainsHardViolation() || !TSParameters.FixAllHardFirst))
            {
                foreach (KeyValuePair<string, string> v in partialViolations.Violations)
                {
                    if (v.Key.Equals("supervisorNotPresident"))
                    {
                        string[] val = v.Value.Split(';');
                        int index = int.Parse(val[0]);
                        string name = val[1];
                        if (candidate.schedule.FinalExams[index].Supervisor.Name.Equals(name) || !TSParameters.CheckViolationPersistance) candidate.schedule.FinalExams[index].President = candidate.schedule.FinalExams[index].Supervisor;
                    }
                    else if (v.Key.Equals("supervisorNotSecretary"))
                    {
                        string[] val = v.Value.Split(';');
                        int index = int.Parse(val[0]);
                        string name = val[1];
                        if (candidate.schedule.FinalExams[index].Supervisor.Name.Equals(name) || !TSParameters.CheckViolationPersistance) candidate.schedule.FinalExams[index].Secretary = candidate.schedule.FinalExams[index].Supervisor;
                    }
                    else if (v.Key.Equals("examinerNotPresident"))
                    {
                        string[] val = v.Value.Split(';');
                        int index = int.Parse(val[0]);
                        string name = val[1];
                        if (candidate.schedule.FinalExams[index].Examiner.Name.Equals(name) || !TSParameters.CheckViolationPersistance) candidate.schedule.FinalExams[index].President = candidate.schedule.FinalExams[index].Examiner;
                    }
                    else if (v.Key.Equals("secretaryNotExaminer"))
                    {
                        string[] val = v.Value.Split(';');
                        int index = int.Parse(val[0]);
                        string name = val[1];
                        if (candidate.schedule.FinalExams[index].Secretary.Name.Equals(name) || !TSParameters.CheckViolationPersistance) candidate.schedule.FinalExams[index].Examiner = candidate.schedule.FinalExams[index].Secretary;
                    }
                    else if (v.Key.Equals("memberNotExaminer"))
                    {
                        string[] val = v.Value.Split(';');
                        int index = int.Parse(val[0]);
                        string name = val[1];
                        if (candidate.schedule.FinalExams[index].Member.Name.Equals(name) || !TSParameters.CheckViolationPersistance) candidate.schedule.FinalExams[index].Examiner = candidate.schedule.FinalExams[index].Member;
                    }
                    else if (v.Key.Equals("supervisorNotExaminer"))
                    {
                        string[] val = v.Value.Split(';');
                        int index = int.Parse(val[0]);
                        string name = val[1];
                        if (candidate.schedule.FinalExams[index].Supervisor.Name.Equals(name) || !TSParameters.CheckViolationPersistance) candidate.schedule.FinalExams[index].Examiner = candidate.schedule.FinalExams[index].Supervisor;
                    }

                    else if (v.Key.Equals("supervisorNotMember"))
                    {
                        string[] val = v.Value.Split(';');
                        int index = int.Parse(val[0]);
                        string name = val[1];
                        if (candidate.schedule.FinalExams[index].Supervisor.Name.Equals(name) || !TSParameters.CheckViolationPersistance) candidate.schedule.FinalExams[index].Member = candidate.schedule.FinalExams[index].Supervisor;
                    }

                    else if (v.Key.Equals("presidentChangeLong"))
                    {
                        string[] data = v.Value.Split(';');
                        int index = int.Parse(data[0]);
                        string name = data[1];
                        if (candidate.schedule.FinalExams[index - 1].President.Name.Equals(name) || !TSParameters.CheckViolationPersistance)
                        {
                            candidate.schedule.FinalExams[index].President = candidate.schedule.FinalExams[index - 1].President;
                        }
                    }
                    else if (v.Key.Equals("secretaryChangeLong"))
                    {
                        string[] data = v.Value.Split(';');
                        int index = int.Parse(data[0]);
                        string name = data[1];
                        if (candidate.schedule.FinalExams[index - 1].Secretary.Name.Equals(name) || !TSParameters.CheckViolationPersistance)
                        {
                            candidate.schedule.FinalExams[index].Secretary = candidate.schedule.FinalExams[index - 1].Secretary;
                        }
                    }
                }
            }

            return candidate;
        }

        //TODO Review / Restructure
        public SolutionCandidate FixViolations_Random(SolutionCandidate candidate, ViolationList partialViolations)
        {
            Random rand = new Random();

            //Trying to fix soft violations only when there are no hard ones left
            if (TSParameters.OptimizeSoftConstraints && (!partialViolations.ContainsHardViolation() || !TSParameters.FixAllHardFirst))
            {
                foreach (KeyValuePair<string, string> v in partialViolations.Violations)
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
                        foreach (FinalExam fe in candidate.schedule.FinalExams)
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
                        foreach (FinalExam fe in candidate.schedule.FinalExams)
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
                        foreach (FinalExam fe in candidate.schedule.FinalExams)
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
            foreach (KeyValuePair<string, string> v in partialViolations.Violations)
            {
                if (v.Key.Equals("supervisorAvailability"))
                {
                    string[] data = v.Value.Split(';');
                    int index = int.Parse(data[0]);
                    string name = data[1];
                    if (candidate.schedule.FinalExams[index].Supervisor.Name.Equals(name) || !TSParameters.CheckViolationPersistance)
                    {
                        int x = rand.Next(0, ctx.Students.Length);
                        while (index == x || !candidate.schedule.FinalExams[index].Student.Supervisor.Availability[x] || !candidate.schedule.FinalExams[x].Student.Supervisor.Availability[index]) x = rand.Next(0, ctx.Students.Length);

                        Student temp = candidate.schedule.FinalExams[index].Student;

                        candidate.schedule.FinalExams[index].Student = candidate.schedule.FinalExams[x].Student;
                        candidate.schedule.FinalExams[x].Student = temp;
                        candidate.schedule.FinalExams[index].Supervisor = candidate.schedule.FinalExams[index].Student.Supervisor;
                        candidate.schedule.FinalExams[x].Supervisor = candidate.schedule.FinalExams[x].Student.Supervisor;
                    }
                }
                else if (v.Key.Equals("studentDuplicated"))
                {
                    string[] data = v.Value.Split(';');
                    int missingID = -1;
                    int duplicatedID = -1;
                    for (int index = 0; index < ctx.Students.Length; index++)
                    {
                        int currentIDCount = int.Parse(data[index]);
                        if (currentIDCount == 0) missingID = currentIDCount;
                        else if (currentIDCount > 1) duplicatedID = currentIDCount;
                    }
                    if (!(duplicatedID == -1 || missingID == -1))
                    {
                        foreach (FinalExam fe in candidate.schedule.FinalExams)
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
                    if (candidate.schedule.FinalExams[index].President.Name.Equals(name) || !TSParameters.CheckViolationPersistance)
                    {
                        int x = rand.Next(0, ctx.Presidents.Length);
                        while (!ctx.Presidents[x].Availability[index])
                        {
                            x = rand.Next(0, ctx.Presidents.Length);
                        }
                        candidate.schedule.FinalExams[index].President = ctx.Presidents[x];
                    }

                }
                else if (v.Key.Equals("secretaryAvailability"))
                {
                    string[] data = v.Value.Split(';');
                    int index = int.Parse(data[0]);
                    string name = data[1];
                    if (candidate.schedule.FinalExams[index].Secretary.Name.Equals(name) || !TSParameters.CheckViolationPersistance)
                    {
                        int x = rand.Next(0, ctx.Secretaries.Length);
                        while (!ctx.Secretaries[x].Availability[index])
                        {
                            x = rand.Next(0, ctx.Secretaries.Length);
                        }
                        candidate.schedule.FinalExams[index].Secretary = ctx.Secretaries[x];
                    }
                }
                else if (v.Key.Equals("wrongExaminer"))
                {
                    string[] data = v.Value.Split(';');
                    int index = int.Parse(data[0]);
                    string name = data[1];
                    if (candidate.schedule.FinalExams[index].Examiner.Name.Equals(name) || !TSParameters.CheckViolationPersistance)
                    {
                        int instIndex = rand.Next(0, ctx.Instructors.Length);
                        Instructor newExaminer = ctx.Instructors[instIndex];
                        while (instIndex < ctx.Instructors.Length && !candidate.schedule.FinalExams[index].Student.ExamCourse.Instructors.Contains(newExaminer))
                        {
                            newExaminer = ctx.Instructors[instIndex];
                            instIndex = rand.Next(0, ctx.Instructors.Length);
                        }
                        if (candidate.schedule.FinalExams[index].Student.ExamCourse.Instructors.Contains(newExaminer)) candidate.schedule.FinalExams[index].Examiner = newExaminer;
                    }
                }
                else if (v.Key.Equals("examinerAvailability"))
                {
                    string[] data = v.Value.Split(';');
                    int index = int.Parse(data[0]);
                    string name = data[1];
                    if (candidate.schedule.FinalExams[index].Examiner.Name.Equals(name) || !TSParameters.CheckViolationPersistance)
                    {
                        int x = rand.Next(0, ctx.Instructors.Length);

                        candidate.schedule.FinalExams[index].Examiner = ctx.Instructors[x];
                    }
                }
                else if (v.Key.Equals("memberAvailability"))
                {
                    string[] data = v.Value.Split(';');
                    int index = int.Parse(data[0]);
                    string name = data[1];
                    if (candidate.schedule.FinalExams[index].Member.Name.Equals(name) || !TSParameters.CheckViolationPersistance)
                    {
                        int x = rand.Next(0, ctx.Members.Length);

                        candidate.schedule.FinalExams[index].Member = ctx.Members[x];
                    }
                }
                else if (v.Key.Equals("presidentChange"))
                {
                    string[] data = v.Value.Split(';');
                    int index = int.Parse(data[0]);
                    string presidentName = data[1];
                    int offset = int.Parse(data[2]);

                    if (candidate.schedule.FinalExams[index - offset].President.Name.Equals(presidentName) || !TSParameters.CheckViolationPersistance) candidate.schedule.FinalExams[index].President = ctx.GetInstructorByName(presidentName);
                }
                else if (v.Key.Equals("secretaryChange"))
                {
                    string[] data = v.Value.Split(';');
                    int index = int.Parse(data[0]);
                    string secretaryName = data[1];
                    int offset = int.Parse(data[2]);

                    if (candidate.schedule.FinalExams[index - offset].Secretary.Name.Equals(secretaryName) || !TSParameters.CheckViolationPersistance) candidate.schedule.FinalExams[index].Secretary = ctx.GetInstructorByName(secretaryName);
                }
                else if (v.Key.Equals("presidentIsSecretary"))
                {
                    string[] data = v.Value.Split(';');
                    int index = int.Parse(data[0]);
                    string name = data[1];
                    if (candidate.schedule.FinalExams[index].President.Name.Equals(name))
                    {
                        int x = rand.Next(0, ctx.Secretaries.Length);
                        while (!ctx.Secretaries[x].Name.Equals(candidate.schedule.FinalExams[index].Secretary.Name) || !TSParameters.CheckViolationPersistance)
                        {
                            x = rand.Next(0, ctx.Secretaries.Length);
                        }

                        candidate.schedule.FinalExams[index].Secretary = ctx.Secretaries[x];
                    }
                }
                else if (v.Key.Equals("presidentIsMember"))
                {
                    string[] data = v.Value.Split(';');
                    int index = int.Parse(data[0]);
                    string name = data[1];
                    if (candidate.schedule.FinalExams[index].President.Name.Equals(name) || !TSParameters.CheckViolationPersistance)
                    {
                        int x = rand.Next(0, ctx.Members.Length);
                        while (!ctx.Members[x].Name.Equals(candidate.schedule.FinalExams[index].Member.Name))
                        {
                            x = rand.Next(0, ctx.Members.Length);
                        }

                        candidate.schedule.FinalExams[index].Member = ctx.Members[x];
                    }
                }
                else if (v.Key.Equals("secretaryIsMember"))
                {
                    string[] data = v.Value.Split(';');
                    int index = int.Parse(data[0]);
                    string name = data[1];
                    if (candidate.schedule.FinalExams[index].Secretary.Name.Equals(name) || !TSParameters.CheckViolationPersistance)
                    {
                        int x = rand.Next(0, ctx.Members.Length);
                        while (!ctx.Members[x].Name.Equals(candidate.schedule.FinalExams[index].Member.Name))
                        {
                            x = rand.Next(0, ctx.Members.Length);
                        }

                        candidate.schedule.FinalExams[index].Member = ctx.Members[x];
                    }
                }
            }

            return candidate;
        }
    }
}
