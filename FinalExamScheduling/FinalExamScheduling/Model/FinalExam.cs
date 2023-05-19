using System.Windows;

namespace FinalExamScheduling.Model
{
    //Based on code provided by Szilvia Erdős
    //An object representing a final exam
    public class FinalExam : Entity
    {
        public Student Student = null;
        public Instructor Supervisor = null;
        public Instructor President = null;
        public Instructor Secretary = null;
        public Instructor Member = null;
        public Instructor Examiner = null;

        //Used for copying an object
        public FinalExam Clone()
        {
            return new FinalExam
            {
                Student = Student,
                Supervisor = Supervisor,
                President = President,
                Secretary = Secretary,
                Member = Member,
                Examiner = Examiner
            };
        }

        //Will compare the received exam to the one it is called on, returns true, when the exams have only identical people scheduled
        public bool IsEqualExam(FinalExam exam)
        {
            if (
                this.Student.Name.Equals(exam.Student.Name)
                &&
                this.Supervisor.Name.Equals(exam.Supervisor.Name)
                &&
                this.President.Name.Equals(exam.President.Name)
                &&
                this.Secretary.Name.Equals(exam.Secretary.Name)
                &&
                this.Member.Name.Equals(exam.Member.Name)
                &&
                this.Examiner.Name.Equals(exam.Examiner.Name)
            ) return true;
            
            return false;
        }
    }
}
