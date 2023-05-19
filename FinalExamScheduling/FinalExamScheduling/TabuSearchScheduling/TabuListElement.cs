using FinalExamScheduling.Model;

namespace FinalExamScheduling.TabuSearchScheduling
{
    //Representing a tabu step. Strongly dependent on the tabu step definition chosen during the design phase.
    class TabuListElement
    {
        //The iterations left of the elements lifecycle, before it is to be disposed
        public int TabuIterationsLeft;

        //The current iteration of the algorithm defines a tabu step as:

        //A certain exam
        public FinalExam Exam;
        //At a certain time (exam slot / time slot)
        public int ExamSlot;

        public TabuListElement(FinalExam _exam, int slot)
        {
            Exam = _exam;
            ExamSlot = slot;

            //In Tandem mode the deafult TTL of the Heuristic mode is used
            switch (TSParameters.Mode)
            {
                case "Random":
                    TabuIterationsLeft = TSParameters.Random.TabuLifeIterations;
                    break;
                case "Heuristic":
                    TabuIterationsLeft = TSParameters.Heuristic.TabuLifeIterations;
                    break;
                default:
                    TabuIterationsLeft = TSParameters.Heuristic.TabuLifeIterations;
                    break;
            }
        }
    }
}
