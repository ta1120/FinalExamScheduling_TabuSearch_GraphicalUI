namespace FinalExamScheduling.TabuSearchScheduling
{
    static class TSParameters
    {

        //Switches
        public const bool AllowShuffleWhenStuck = false;

        public const bool MuteConsoleUnlessDone = false;

        public const bool PrintDetails = false;

        public const bool OptimizeSoftConstraints = false;

        public const bool RestartUntilTargetReached = true;

        public const bool LogIterationalProgress = true;


        //Mode of neighbour generation: Random/Greedy/Tandem
        public const string Mode = "Tandem";

        //Tabu parameters
        public class Random
        {
            public const int TabuLifeIterations = 5; //15

            public const int TabuListLength = 1; //40
        }

        public class Greedy
        {
            public const int TabuLifeIterations = 1; //10

            public const int TabuListLength = 1; //3
        }

        //Other parameters

        public const int ExamBlockLength = 5;

        public const int WriteOutLimit = 60; //If scores under this are reached, the results will be written out to file. Set to negative value to write all results to file

        public const int MaxShuffles = 1;

        public const int ShufflePercentage = 20;

        public const int GeneratedCandidates = 15; //25

        public const int AllowedIdleIterations = 10; //10

        public const double TargetScore = 40; //40 is the best reachable score for the original input file

        public const bool GetInfo = true;

        public const int MaxFailedNeighbourGenerations = 5; //3

        public const int TandemIdleSwitches = 5; //1

        //Optimalization switches for distinct constraints
        public const bool SolveWrongExaminer = true;
        public const bool SolveStudentDuplicated = true;
        public const bool SolvePresidentAvailability = true;
        public const bool SolveSecretaryAvailability = true;
        public const bool SolveExaminerAvailability = true;
        public const bool SolveSupervisorAvailability = true;
        public const bool SolveMemberAvailability = true;
        public const bool SolvePresidentChange = true;
        public const bool SolveSecretaryChange = true;

        public const bool SolvePresidentWorkload = true;
        public const bool SolveSecretaryWorkload = true;
        public const bool SolveMemberWorkload = true;
        public const bool SolveExaminerNotPresident = true;
        public const bool SolveSupervisorNotPresident = true;
        public const bool SolveSupervisorNotSecretary = true;
    }
}
