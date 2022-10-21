namespace FinalExamScheduling.Model
{
    public class Student : Entity
    {
        public string Name;
        public string Neptun;
        public Instructor Supervisor;
        public Course ExamCourse;
        public int TimeSlot;
    }
}
