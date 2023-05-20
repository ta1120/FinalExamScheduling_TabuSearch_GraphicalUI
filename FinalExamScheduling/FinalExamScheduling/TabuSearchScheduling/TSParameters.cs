namespace FinalExamScheduling.TabuSearchScheduling
{
    //This class contains the "global" parameters used by the algorithm
    static class TSParameters
    {
        //######################################
        //Parameters available to the user (GUI)
        //######################################

        //Switches
        public static bool AllowShuffleWhenStuck = false;

        public static bool OptimizeSoftConstraints = true;

        public static bool RestartUntilTargetReached = true;

        public static bool FixAllHardFirst = true;

        //Mode of neighbour generation: Random/Heuristic/Tandem
        public static string Mode = "Tandem";

        //Tabu parameters for both modes are defined in nested classes
        public class Random
        {
            public static int TabuLifeIterations = 10;

            public static int TabuListLength = 10; 
        }

        public class Heuristic
        {
            public static int TabuLifeIterations = 5;

            public static int TabuListLength = 5;
        }

        //Other, numeric parameters

        public static int ViolationsToFixPerGeneration = 50;

        public static int WriteOutLimit = -1; 

        public static int MaxShuffles = 1;

        public static int ShufflePercentage = 20;

        public static int GeneratedCandidates = 20; 

        public static int AllowedIdleIterations = 20; 

        public static double TargetScore = 600; 

        public static int MaxFailedNeighbourGenerations = 1; 

        public static int TandemIdleSwitches = 50;

        /*
        * ##########################################
        * Parameters for testing during developement
        * ##########################################
        */

        public const bool LogIterationalProgress = true;

        public const bool GetInfo = true;

        public const int ExamBlockLength = 5;

        public static bool CheckViolationPersistance = true;

        //Optimalization switches for distinct constraints, used for testing purposes only
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
