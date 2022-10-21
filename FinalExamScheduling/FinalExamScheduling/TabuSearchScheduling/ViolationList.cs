using FinalExamScheduling.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinalExamScheduling.TabuSearchScheduling
{
    class ViolationList
    {
        public List<KeyValuePair<string, string>> Violations { get; set; }

        //private Context ctx;

        public List<Func<Schedule, ViolationList>> ViolationFunctions;

        public ViolationList()
        {
            Violations = new List<KeyValuePair<string, string>>();
        }

        public void AddViolation(string param, string value)
        {
            Violations.Add(new KeyValuePair<string, string>(param, value));
        }

        public bool ContainsHardViolation()
        {

            foreach(KeyValuePair<string, string> v in Violations)
            {
                if (v.Key == "wrongExaminer" 
                    || v.Key == "studentDuplicated" 
                    || v.Key == "presidentAvailability" 
                    || v.Key == "secretaryAvailability" 
                    || v.Key == "examinerAvailability" 
                    || v.Key == "presidentChange" 
                    || v.Key == "secretaryChange" 
                    || v.Key == "presidentIsSecretary" 
                    || v.Key == "PresidentIsMember" 
                    || v.Key == "secretaryIsMember" 
                    || v.Key == "wrongSupervisor") return true;
            }

            return false;
        }

        public void AddViolation(KeyValuePair<string, string> v)
        {
            Violations.Add(v);
        }

        public void printViolations()
        {
            if (this.Violations == null) return;

            foreach (KeyValuePair<string, string> v in this.Violations)
            {
                Console.WriteLine("Violation: " + v.Key + " - " + v.Value);
            }
        }

        public ViolationList Evaluate(SolutionCandidate cand)
        {
            Schedule sch = cand.Schedule;

            ViolationList vi = new ViolationList();

            ViolationFunctions = new List<Func<Schedule, ViolationList>>()
            {
                GetWrongExaminerViolations,
                GetStudentDuplicatedViolations,
                GetPresidentNotAvailableViolations,
                GetSecretaryNotAvailableViolations,
                GetExaminerNotAvailableViolations,
                GetMemberNotAvailableViolations,
                GetSupervisorNotAvailableViolations,

                GetPresidentChangeViolations,
                GetSecretaryChangeViolations,
                GetPresidentChangeLongViolations,
                GetSecretaryChangeLongViolations,

                GetPresidentWorkloadViolations,
                GetSecretaryWorkloadViolations,
                GetMemberWorkloadViolations,

                GetSupervisorNotPresidentViolations,
                GetSupervisorNotSecretaryViolations,
                GetExaminerNotPresidentViolations,
                
                GetPresidentIsSecretaryViolations,
                GetPresidentIsMemberViolations,
                GetSecretaryIsMemberViolations,
                
                GetSecretaryNotExaminerViolations,
                GetMemberNotExaminerViolations,
                GetSupervisorNotMemberViolations,
                GetSupervisorNotExaminerViolations,
                
                GetWrongSupervisorViolations

           };

            var tasks = ViolationFunctions.Select(cf => Task.Run(() => cf(sch))).ToArray();
            Task.WaitAll(tasks);
            foreach (var task in tasks)
            {
                ViolationList viRes = task.Result;
                foreach (KeyValuePair<string, string> v in viRes.Violations)
                {
                    vi.AddViolation(v);
                }
            }

            return vi;
        }

        public ViolationList GetWrongExaminerViolations(Schedule sch)
        {
            ViolationList vl = new ViolationList();
            if (!TSParameters.SolveWrongExaminer) return vl;
            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                if (!sch.FinalExams[i].Student.ExamCourse.Instructors.ToArray().Contains(sch.FinalExams[i].Examiner)) vl.AddViolation("wrongExaminer", i.ToString() + ';' + sch.FinalExams[i].Examiner.Name);
            }
            return vl;
        }

        public ViolationList GetStudentDuplicatedViolations(Schedule sch)
        {
            ViolationList vl = new ViolationList();
            if (!TSParameters.SolveStudentDuplicated) return vl;
            List<Student> studentBefore = new List<Student>();
            int[] count = new int[100];
            foreach (var fe in sch.FinalExams)
            {
                count[fe.Student.Id]++;
            }
            string data = "";
            bool duplicated = false;
            for (int i = 0; i < 100; i++)
            {
                data += count[i].ToString() + ';';
                if (count[i] > 1) duplicated = true;
            }
            if (duplicated)
            {
                vl.AddViolation("studentDuplicated", data);
            }
            return vl;
        }

        public ViolationList GetPresidentNotAvailableViolations(Schedule sch)
        {
            ViolationList vl = new ViolationList();
            if (!TSParameters.SolvePresidentAvailability) return vl;
            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                if (sch.FinalExams[i].President.Availability[i] == false)
                {
                    vl.AddViolation("presidentAvailability", i.ToString() + ";" + sch.FinalExams[i].President.Name);
                }
            }
            return vl;
        }

        public ViolationList GetSecretaryNotAvailableViolations(Schedule sch)
        {
            ViolationList vl = new ViolationList();
            if (!TSParameters.SolveSecretaryAvailability) return vl;
            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                if (sch.FinalExams[i].Secretary.Availability[i] == false)
                {
                    vl.AddViolation("secretaryAvailability", i.ToString() + ";" + sch.FinalExams[i].Secretary.Name);
                }
            }
            return vl;
        }

        public ViolationList GetExaminerNotAvailableViolations(Schedule sch)
        {
            ViolationList vl = new ViolationList();
            if (!TSParameters.SolveExaminerAvailability) return vl;
            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                if (sch.FinalExams[i].Examiner.Availability[i] == false)
                {
                    vl.AddViolation("examinerAvailability", i.ToString() + ";" + sch.FinalExams[i].Examiner.Name);
                }
            }
            return vl;
        }

        public ViolationList GetMemberNotAvailableViolations(Schedule sch)
        {
            ViolationList vl = new ViolationList();
            if (!TSParameters.SolveMemberAvailability) return vl;
            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                if (sch.FinalExams[i].Member.Availability[i] == false)
                {
                    vl.AddViolation("memberAvailability", i.ToString() + ";" + sch.FinalExams[i].Member.Name);
                }
            }
            return vl;
        }

        public ViolationList GetSupervisorNotAvailableViolations(Schedule sch)
        {
            ViolationList vl = new ViolationList();
            if (!TSParameters.SolveSupervisorAvailability) return vl;
            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                if (sch.FinalExams[i].Supervisor.Availability[i] == false)
                {
                    vl.AddViolation("supervisorAvailability", i.ToString() + ";" + sch.FinalExams[i].Supervisor.Name);
                }
            }
            return vl;
        }

        public ViolationList GetPresidentChangeViolations(Schedule sch)
        {
            ViolationList vl = new ViolationList();
            for (int i = 0; i < sch.FinalExams.Length; i += TSParameters.ExamBlockLength)
            {
                for(int offset = 1;offset < TSParameters.ExamBlockLength;offset++)
                {
                    if (!sch.FinalExams[i].President.Name.Equals(sch.FinalExams[i + offset].President.Name))
                    {
                        vl.AddViolation("presidentChange", (i + offset).ToString() + ";" + sch.FinalExams[i].President.Name + ";" + offset.ToString());
                    }
                }
            }
            return vl;
        }

        public ViolationList GetSecretaryChangeViolations(Schedule sch)
        {
            ViolationList vl = new ViolationList();
            if (!TSParameters.SolveSecretaryChange) return vl;
            for (int i = 0; i < sch.FinalExams.Length; i += TSParameters.ExamBlockLength)
            {
                for (int offset = 1; offset < TSParameters.ExamBlockLength; offset++)
                {
                    if (!sch.FinalExams[i].Secretary.Name.Equals(sch.FinalExams[i + offset].Secretary.Name))
                    {
                        vl.AddViolation("secretaryChange", (i + offset).ToString() + ";" + sch.FinalExams[i].Secretary.Name + ";" + offset.ToString());
                    }
                }
            }
            return vl;
        }


        public ViolationList GetPresidentWorkloadViolations(Schedule schedule)
        {
            ViolationList vl = new ViolationList();
            if (!TSParameters.SolvePresidentWorkload) return vl;
            List<KeyValuePair<string, int>> workloads = new List<KeyValuePair<string, int>>();

            foreach (FinalExam fe in schedule.FinalExams)
            {
                bool found = false;
                int oldValue = 0;
                KeyValuePair<string, int> wlToBeRemoved = new KeyValuePair<string, int>();
                foreach (KeyValuePair<string, int> wl in workloads)
                {
                    if (wl.Key.Equals(fe.President.Name))
                    {
                        found = true;
                        oldValue = wl.Value;
                        wlToBeRemoved = wl;
                    }
                }
                if (!found)
                {
                    workloads.Add(new KeyValuePair<string, int>(fe.President.Name, 1));
                }
                else
                {
                    workloads.Remove(wlToBeRemoved);

                    workloads.Add(new KeyValuePair<string, int>(fe.President.Name, (oldValue + 1)));
                }
            }
            if (workloads.Count > 1)
            {
                KeyValuePair<string, int> max = workloads.ElementAt(0);
                KeyValuePair<string, int> min = max;

                for (int i = 1; i < workloads.Count; i++)
                {
                    KeyValuePair<string, int> wl = workloads.ElementAt(i);
                    if (wl.Value < min.Value) min = wl;
                    if (wl.Value > max.Value) max = wl;
                }
                if (!ReferenceEquals(min, max)) vl.AddViolation("presidentWorkload", max.Key + ";" + max.Value + ";" + min.Key + ";" + min.Value);
            }

            return vl;
        }

        public ViolationList GetSecretaryWorkloadViolations(Schedule schedule)
        {
            ViolationList vl = new ViolationList();
            if (!TSParameters.SolveSecretaryWorkload) return vl;
            List<KeyValuePair<string, int>> workloads = new List<KeyValuePair<string, int>>();

            foreach (FinalExam fe in schedule.FinalExams)
            {
                bool found = false;
                int oldValue = 0;
                KeyValuePair<string, int> wlToBeRemoved = new KeyValuePair<string, int>();
                foreach (KeyValuePair<string, int> wl in workloads)
                {
                    if (wl.Key.Equals(fe.Secretary.Name))
                    {
                        found = true;
                        oldValue = wl.Value;
                        wlToBeRemoved = wl;
                    }
                }
                if (!found)
                {
                    workloads.Add(new KeyValuePair<string, int>(fe.Secretary.Name, 1));
                }
                else
                {
                    workloads.Remove(wlToBeRemoved);

                    workloads.Add(new KeyValuePair<string, int>(fe.Secretary.Name, (oldValue + 1)));
                }
            }
            if (workloads.Count > 1)
            {
                KeyValuePair<string, int> max = workloads.ElementAt(0);
                KeyValuePair<string, int> min = max;

                for (int i = 1; i < workloads.Count; i++)
                {
                    KeyValuePair<string, int> wl = workloads.ElementAt(i);
                    if (wl.Value < min.Value) min = wl;
                    if (wl.Value > max.Value) max = wl;
                }
                if (!ReferenceEquals(min, max)) vl.AddViolation("secretaryWorkload", max.Key + ";" + max.Value + ";" + min.Key + ";" + min.Value);
            }

            return vl;
        }
        public ViolationList GetMemberWorkloadViolations(Schedule schedule)
        {
            ViolationList vl = new ViolationList();
            if (!TSParameters.SolveMemberWorkload) return vl;
            List<KeyValuePair<string, int>> workloads = new List<KeyValuePair<string, int>>();

            foreach (FinalExam fe in schedule.FinalExams)
            {
                bool found = false;
                int oldValue = 0;
                KeyValuePair<string, int> wlToBeRemoved = new KeyValuePair<string, int>();
                foreach (KeyValuePair<string, int> wl in workloads)
                {
                    if (wl.Key.Equals(fe.Member.Name))
                    {
                        found = true;
                        oldValue = wl.Value;
                        wlToBeRemoved = wl;
                    }
                }
                if (!found)
                {
                    workloads.Add(new KeyValuePair<string, int>(fe.Member.Name, 1));
                }
                else
                {
                    workloads.Remove(wlToBeRemoved);

                    workloads.Add(new KeyValuePair<string, int>(fe.Member.Name, (oldValue + 1)));
                }
            }
            if (workloads.Count > 1)
            {
                KeyValuePair<string, int> max = workloads.ElementAt(0);
                KeyValuePair<string, int> min = max;

                for (int i = 1; i < workloads.Count; i++)
                {
                    KeyValuePair<string, int> wl = workloads.ElementAt(i);
                    if (wl.Value < min.Value) min = wl;
                    if (wl.Value > max.Value) max = wl;
                }
                if (!ReferenceEquals(min, max)) vl.AddViolation("memberWorkload", max.Key + ";" + max.Value + ";" + min.Key + ";" + min.Value);
            }

            return vl;
        }



        public ViolationList GetSupervisorNotPresidentViolations(Schedule sch)
        {
            ViolationList vl = new ViolationList();
            if (!TSParameters.SolveSupervisorNotPresident) return vl;
            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                FinalExam fi = sch.FinalExams[i];
                if ((fi.Supervisor.Roles & Roles.President) == Roles.President && fi.Supervisor != fi.President)
                {
                    vl.AddViolation("supervisorNotPresident", i.ToString() + ";" + fi.Supervisor.Name);
                }
            }
            return vl;
        }

        public ViolationList GetSupervisorNotSecretaryViolations(Schedule sch)
        {
            ViolationList vl = new ViolationList();
            if (!TSParameters.SolveSupervisorNotSecretary) return vl;
            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                FinalExam fi = sch.FinalExams[i];
                if ((fi.Supervisor.Roles & Roles.Secretary) == Roles.Secretary && fi.Supervisor != fi.Secretary)
                {
                    vl.AddViolation("supervisorNotSecretary", i.ToString() + ";" + fi.Supervisor.Name);
                }
            }
            return vl;
        }

        public ViolationList GetExaminerNotPresidentViolations(Schedule sch)
        {
            ViolationList vl = new ViolationList();
            if (!TSParameters.SolveExaminerNotPresident) return vl;
            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                FinalExam fi = sch.FinalExams[i];
                if ((fi.Examiner.Roles & Roles.President) == Roles.President && fi.Examiner != fi.President)
                {
                    vl.AddViolation("examinerNotPresident", i.ToString() + ";" + fi.Examiner.Name);
                }
            }
            return vl;
        }

        public ViolationList GetPresidentIsSecretaryViolations(Schedule sch)
        {
            ViolationList vl = new ViolationList();
            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                FinalExam fi = sch.FinalExams[i];
                if (fi.President.Name.Equals(fi.Secretary.Name))
                {
                    vl.AddViolation("presidentIsSecretary", i.ToString() + ";" + sch.FinalExams[i].President.Name);
                }
            }
            return vl;
        }

        public ViolationList GetPresidentIsMemberViolations(Schedule sch)
        {
            ViolationList vl = new ViolationList();
            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                FinalExam fi = sch.FinalExams[i];
                if (fi.President.Name.Equals(fi.Member.Name))
                {
                    vl.AddViolation("presidentIsMember", i.ToString() + ";" + sch.FinalExams[i].President.Name);
                }
            }
            return vl;
        }

        public ViolationList GetSecretaryIsMemberViolations(Schedule sch)
        {
            ViolationList vl = new ViolationList();
            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                FinalExam fi = sch.FinalExams[i];
                if (fi.Member.Name.Equals(fi.Secretary.Name))
                {
                    vl.AddViolation("secretaryIsMember", i.ToString() + ";" + sch.FinalExams[i].Secretary.Name);
                }
            }
            return vl;
        }
        public ViolationList GetSecretaryNotExaminerViolations(Schedule sch)
        {
            ViolationList vl = new ViolationList();
            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                FinalExam fi = sch.FinalExams[i];
                if (!fi.Secretary.Name.Equals(fi.Examiner.Name) && fi.Student.ExamCourse.Instructors.Contains(fi.Secretary))
                {
                    vl.AddViolation("secretaryNotExaminer", i.ToString() + ";" + fi.Secretary.Name);
                }
            }
            return vl;
        }
        public ViolationList GetMemberNotExaminerViolations(Schedule sch)
        {
            ViolationList vl = new ViolationList();
            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                FinalExam fi = sch.FinalExams[i];
                if (!fi.Member.Name.Equals(fi.Examiner.Name) && fi.Student.ExamCourse.Instructors.Contains(fi.Member))
                {
                    vl.AddViolation("memberNotExaminer", i.ToString() + ";" + fi.Member.Name);
                }
            }
            return vl;
        }
        public ViolationList GetSupervisorNotExaminerViolations(Schedule sch)
        {
            ViolationList vl = new ViolationList();
            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                FinalExam fi = sch.FinalExams[i];
                if (!fi.Supervisor.Name.Equals(fi.Examiner.Name) && fi.Student.ExamCourse.Instructors.Contains(fi.Supervisor))
                {
                    vl.AddViolation("supervisorNotExaminer", i.ToString() + ";" + fi.Supervisor.Name);
                }
            }
            return vl;
        }
        public ViolationList GetSupervisorNotMemberViolations(Schedule sch)
        {
            ViolationList vl = new ViolationList();
            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                FinalExam fi = sch.FinalExams[i];
                if (!fi.Supervisor.Name.Equals(fi.Member.Name) && (fi.Supervisor.Roles & Roles.Member) == Roles.Member)
                {
                    vl.AddViolation("supervisorNotMember", i.ToString() + ";" + fi.Supervisor.Name);
                }
            }
            return vl;
        }
        public ViolationList GetWrongSupervisorViolations(Schedule sch)
        {
            ViolationList vl = new ViolationList();
            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                FinalExam fi = sch.FinalExams[i];
                if (!fi.Supervisor.Name.Equals(fi.Student.Supervisor.Name))
                {
                    vl.AddViolation("wrongSupervisor", i.ToString());
                }
            }
            return vl;
        }

        public ViolationList GetPresidentChangeLongViolations(Schedule sch)
        {
            ViolationList vl = new ViolationList();
            int ctr = 1;
            Instructor currentPresident = sch.FinalExams[0].President;

            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                if (ctr > 10)
                {
                    ctr = 1;
                    currentPresident = sch.FinalExams[i].President;
                }
                else
                {
                    ctr++;
                    if (!sch.FinalExams[i].President.Name.Equals(currentPresident.Name)) vl.AddViolation("presidentChangeLong",i.ToString() + ";" + currentPresident.Name);
                }

            }
            return vl;
        }
        public ViolationList GetSecretaryChangeLongViolations(Schedule sch)
        {
            ViolationList vl = new ViolationList();
            int ctr = 1;
            Instructor currentSecretary = sch.FinalExams[0].Secretary;

            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                if (ctr > 10)
                {
                    ctr = 1;
                    currentSecretary = sch.FinalExams[i].Secretary;
                }
                else
                {
                    ctr++;
                    if (!sch.FinalExams[i].Secretary.Name.Equals(currentSecretary.Name)) vl.AddViolation("secretaryChangeLong", i.ToString() + ";" + currentSecretary.Name);
                }

            }
            return vl;
        }

    }
}

