namespace FinalExamScheduling.Model
{
    public class FinalExam : Entity
    {
        public Student Student = null;


        public Instructor Supervisor = null;


        public Instructor President = null;


        public Instructor Secretary = null;


        public Instructor Member = null;


        public Instructor Examiner = null;

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

        public string toString()
        {
            string fe = "";
            fe += ("Student: " + Student.Name);
            fe += (";President: " + President.Name);
            fe += (";Supervisor: " + Supervisor.Name);
            fe += (";Secretary: " + Secretary.Name);
            fe += (";Member: " + Member.Name);
            fe += (";Examiner: " + Examiner.Name);

            return fe;
        }

        public void setStudent(Student s) { Student = s; }
    }
}
