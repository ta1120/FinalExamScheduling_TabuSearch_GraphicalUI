namespace FinalExamScheduling.Model
{
    //Code provided by Szilvia Erdős
    //Used for storing the data of an exam course
    public class Course : Entity
    {
        public string Name;
        public string CourseCode;
        public Instructor[] Instructors;

    }
}
