using System;

namespace FinalExamScheduling.Model
{
    [Flags]
    public enum Roles
    {
        Unknown = 0,
        President = 1,
        Member = 2,
        Secretary = 4
    }

    public class Instructor : Entity
    {
        public string Name;

        public bool[] Availability;
        public Roles Roles;

    }
}
