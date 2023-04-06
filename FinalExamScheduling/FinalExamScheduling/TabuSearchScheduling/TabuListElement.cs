using FinalExamScheduling.Model;

namespace FinalExamScheduling.TabuSearchScheduling
{
    class TabuListElement
    {
        public FinalExam Exam;
        public int TabuIterationsLeft;
        public int ExamSlot;

        public TabuListElement(FinalExam _exam, int slot)
        {
            Exam = _exam;
            ExamSlot = slot;
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
