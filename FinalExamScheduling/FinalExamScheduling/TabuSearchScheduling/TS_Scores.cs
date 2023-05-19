﻿namespace FinalExamScheduling.TabuSearchScheduling
{
    //This class was originally copied from the solution by Szilvia Erdős, but since then it has been expanded with a few constraints
    //The class contains the penalty points for the violation of each contraint
    static class TS_Scores
    {
        public const double WrongExaminer = 2000;

        public const double StudentDuplicated = 10000;

        public const double PresidentNotAvailable = 1000;
        public const double SecretaryNotAvailable = 1000;
        public const double ExaminerNotAvailable = 1000;
        public const double MemberNotAvailable = 5;
        public const double SupervisorNotAvailable = 5;

        public const double PresidentChange = 1000;
        public const double SecretaryChange = 1000;
        public const double PresidentChangeLong = 10;
        public const double SecretaryChangeLong = 10;

        public const double PresidentWorkloadWorst = 30;
        public const double PresidentWorkloadWorse = 20;
        public const double PresidentWorkloadBad = 10;
        public const double SecretaryWorkloadWorst = 30;
        public const double SecretaryWorkloadWorse = 20;
        public const double SecretaryWorkloadBad = 10;
        public const double MemberWorkloadWorst = 30;
        public const double MemberWorkloadWorse = 20;
        public const double MemberWorkloadBad = 10;

        public const double PresidentIsSecretary = 5000;
        public const double PresidentIsMember = 5000;
        public const double SecretaryIsMember = 5000;

        public const double SupervisorNotPresident = 1;
        public const double SupervisorNotSecretary = 1;
        public const double SupervisorNotMember = 1;
        public const double SupervisorNotExaminer = 1;
        public const double PresidentNotExaminer = 1;
        public const double SecretaryNotExaminer = 1;
        public const double MemberNotExaminer = 1;

        public const double WrongSupervisor = 2000;
    }
}
