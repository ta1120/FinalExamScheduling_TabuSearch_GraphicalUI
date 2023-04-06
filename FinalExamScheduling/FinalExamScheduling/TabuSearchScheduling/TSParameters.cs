namespace FinalExamScheduling.TabuSearchScheduling
{
    static class TSParameters
    {

        //Switches
        public static bool AllowShuffleWhenStuck = false;

        public static bool OptimizeSoftConstraints = false;

        public static bool RestartUntilTargetReached = true;

        public const bool LogIterationalProgress = true;

        public static bool CheckViolationPersistance = true;

        public static bool FixAllHardFirst = true;

        //Mode of neighbour generation: Random/Heuristic/Tandem
        public static string Mode = "Tandem";

        //Tabu parameters
        public class Random
        {
            public static int TabuLifeIterations = 5; //15

            public static int TabuListLength = 1; //40
        }

        public class Heuristic
        {
            public static int TabuLifeIterations = 1; //10

            public static int TabuListLength = 1; //3
        }

        //Other parameters

        public const int NeighbourDifferentAttributeCount = 50;

        public const int ExamBlockLength = 5;

        public static int WriteOutLimit = 60; //If scores under this are reached, the results will be written out to file. Set to negative value to write all results to file

        public static int MaxShuffles = 1;

        public static int ShufflePercentage = 20;

        public static int GeneratedCandidates = 15; //25

        public static int AllowedIdleIterations = 10; //10

        public static double TargetScore = 40; //40 is the best reachable score for the original input file

        public const bool GetInfo = true;

        public static int MaxFailedNeighbourGenerations = 5; //3

        public static int TandemIdleSwitches = 5; //1

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
