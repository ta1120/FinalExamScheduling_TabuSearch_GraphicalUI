using FinalExamScheduling.Model;

namespace FinalExamScheduling.TabuSearchScheduling
{
    class SolutionCandidate
    {
        public Schedule schedule;

        public double score;

        public ViolationList vl;

        public SolutionCandidate(Schedule sch)
        {
            schedule = sch.Clone();
            score = -1;
        }

        public SolutionCandidate Clone()
        {
            return new SolutionCandidate(schedule.Clone()) { vl = vl, score = score };
        }

    }
}
