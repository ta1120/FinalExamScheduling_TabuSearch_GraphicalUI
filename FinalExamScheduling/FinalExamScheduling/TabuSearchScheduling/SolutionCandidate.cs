using FinalExamScheduling.Model;

namespace FinalExamScheduling.TabuSearchScheduling
{
    //This object is used to represent a possible solution
    class SolutionCandidate
    {
        //The main part of a solution is the schedule 
        public Schedule schedule;

        //A solution has a cost property, this is called "score" in the current implementation, but this is to be changed in the next iteration to "cost"
        public double score;

        //The last property used to describe a solution is the Violation List
        public ViolationList vl;

        public SolutionCandidate(Schedule sch)
        {
            schedule = sch.Clone();
            score = -1;
        }
        
        //Used for copying the object
        public SolutionCandidate Clone()
        {
            return new SolutionCandidate(schedule.Clone()) { vl = vl, score = score };
        }

    }
}
