namespace FinalExamScheduling.Model
{
    //Based on code provided by Szilvia Erdős
    //Used for storing data about a student
    public class Student : Entity
    {
        //Name of student
        public string Name;

        //Supervising instructor of the student
        public Instructor Supervisor;

        //Exam course, which the student is taking
        public Course ExamCourse;
    }
}
