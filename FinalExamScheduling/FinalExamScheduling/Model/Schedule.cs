using System.Linq;


namespace FinalExamScheduling.Model
{
    //Based on code provided by Szilvia Erdős
    //Used for representing a complete final exam schedule
    public class Schedule
    {
        public FinalExam[] FinalExams;

        public Schedule(int examCount)
        {
            FinalExams = new FinalExam[examCount];
        }

        //Used for copying a schedule
        public Schedule Clone()
        {
            return new Schedule(FinalExams.Length)
            {
                FinalExams = FinalExams.Select(exam => (FinalExam) exam.Clone()).ToArray(),
            };
        }
    }
}
