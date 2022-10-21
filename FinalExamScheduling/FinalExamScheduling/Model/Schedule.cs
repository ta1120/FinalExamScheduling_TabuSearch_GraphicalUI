using System.Linq;


namespace FinalExamScheduling.Model
{
    public class Schedule
    {
        public FinalExam[] FinalExams;
        public FinalExamDetail[] Details;
        public Schedule(int examCount)
        {
            FinalExams = new FinalExam[examCount];
        }

        public Schedule Clone()
        {
            return new Schedule(FinalExams.Length)
            {
                FinalExams = FinalExams.Select(a => (FinalExam)a.Clone()).ToArray(),
                Details = Details
            };
        }

        public bool Equals(Schedule other)
        {
            for (int i = 0; i < FinalExams.Length; i++)
            {
                if (!this.FinalExams[i].Student.Name.Equals(other.FinalExams[i].Student.Name))
                {
                    return false;
                }
                if (!this.FinalExams[i].Supervisor.Name.Equals(other.FinalExams[i].Supervisor.Name))
                {
                    return false;
                }
                if (!this.FinalExams[i].President.Name.Equals(other.FinalExams[i].President.Name))
                {
                    return false;
                }
                if (!this.FinalExams[i].Secretary.Name.Equals(other.FinalExams[i].Secretary.Name))
                {
                    return false;
                }
                if (!this.FinalExams[i].Member.Name.Equals(other.FinalExams[i].Member.Name))
                {
                    return false;
                }
                if (!this.FinalExams[i].Examiner.Name.Equals(other.FinalExams[i].Examiner.Name))
                {
                    return false;
                }
            }
            return true;
        }

        /*public string ToString(SchedulingFitness fitness)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Score for instructor not available: {fitness.GetInstructorAvailableScore(this)}");
            sb.AppendLine($"Score for role: {fitness.GetRolesScore(this)}");
            sb.AppendLine($"Score for multiple students: {fitness.GetStudentDuplicatedScore(this)}" );
            sb.AppendLine($"Score for Presidents Workload: {fitness.GetPresidentWorkloadScore(this)}" );
            sb.AppendLine($"Score for Secretary Workload: {fitness.GetSecretaryWorkloadScore(this)}" );
            sb.AppendLine($"Score for Member Workload: {fitness.GetMemberWorkloadScore(this)}" );
            sb.AppendLine($"Score for Presidents Change: {fitness.GetPresidentChangeScore(this)}" );
            sb.AppendLine($"Score for Secretary Change: {fitness.GetSecretaryChangeScore(this)}");
            return sb.ToString();
        }*/


    }
}
