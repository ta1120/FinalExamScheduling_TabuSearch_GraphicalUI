using System;
using System.Collections.Generic;
using System.Linq;

namespace FinalExamScheduling.TabuSearchScheduling
{
    class TabuList
    {
        private List<TabuListElement> tabuElements;
        private int listLength;

        public TabuList()
        {
            tabuElements = new List<TabuListElement>();
            ChangeListParameter_Mode(TSParameters.Mode);
        }

        public List<TabuListElement> GetTabuList() { return tabuElements; }

        public void Add(TabuListElement element)
        {
            foreach (TabuListElement tabu in tabuElements)
            {
                if (tabu.Exam.IsEqualExam(element.Exam) && tabu.ExamSlot.Equals(element.ExamSlot))
                {
                    tabuElements.Remove(tabu);
                    tabuElements.Add(element);
                    return;
                }
            }
            if (tabuElements.Count < listLength) tabuElements.Add(element);
            else
            {
                tabuElements.RemoveAt(0);
                tabuElements.Add(element);
            }
        }

        public void DecreaseIterationsLeft()
        {
            if (tabuElements.Count > 0)
            {
                foreach (TabuListElement element in tabuElements) element.TabuIterationsLeft -= 1;
                
                RemoveInactive();
            }
        }

        public void RemoveInactive()
        {
            List<TabuListElement> toBeRemoved = new List<TabuListElement>();

            foreach (TabuListElement tabu in tabuElements)
            {
                if (tabu.TabuIterationsLeft < 1) toBeRemoved.Add(tabu);
            }
            foreach (TabuListElement tabu in toBeRemoved)
            {
                tabuElements.Remove(tabu);
            }
        }

        public void ChangeListParameter_Mode(string mode)
        {
            tabuElements.Clear();
            switch (mode)
            {
                case "Random":
                    listLength = TSParameters.Random.TabuListLength;
                    break;
                case "Heuristic":
                    listLength = TSParameters.Heuristic.TabuListLength;
                    break;
                default:
                    listLength = TSParameters.Random.TabuListLength;
                    break;
            }
        }
    }
}
