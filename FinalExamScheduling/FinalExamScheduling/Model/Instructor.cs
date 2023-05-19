using System;

namespace FinalExamScheduling.Model
{
    //Code provided by Szilvia Erdős
    //Used for defining the roles an instructor might fulfill
    [Flags]
    public enum Roles
    {
        Unknown = 0,
        President = 1,
        Member = 2,
        Secretary = 4
    }

    //Code provided by Szilvia Erdős
    //Used for storing data about instructors
    public class Instructor : Entity
    {
        //Name of the instructor
        public string Name;

        //Availability - each position of the array repersents an exam slot (sequentially)
        public bool[] Availability;

        //Used for storing fulfillable roles
        public Roles Roles;

    }
}
