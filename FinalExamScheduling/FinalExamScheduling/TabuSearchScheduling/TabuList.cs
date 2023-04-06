using System;
using System.Collections.Generic;
using System.Linq;

namespace FinalExamScheduling.TabuSearchScheduling
{
    class TabuList
    {
        public List<TabuListElement> tabuList;
        public int listLength;

        public TabuList()
        {
            tabuList = new List<TabuListElement>();
            ChangeListParametersForMode(TSParameters.Mode);
        }

        public List<TabuListElement> GetTabuList() { return tabuList; }

        public void Add(TabuListElement element)
        {
            foreach (TabuListElement tabu in tabuList)
            {
                if (tabu.Exam.IsEqualExam(element.Exam) && tabu.ExamSlot.Equals(element.ExamSlot))
                {
                    tabuList.Remove(tabu);
                    tabuList.Add(element);
                    return;
                }
            }
            if (tabuList.Count < listLength) tabuList.Add(element);
            else
            {
                tabuList.RemoveAt(0);
                tabuList.Add(element);
            }
        }

        public void DecreaseIterationsLeft()
        {
            if (tabuList.Count > 0)
            {
                foreach (TabuListElement element in tabuList) element.TabuIterationsLeft -= 1;
                
                RemoveInactive();
            }
        }

        public void RemoveInactive()
        {
            List<TabuListElement> toBeRemoved = new List<TabuListElement>();

            foreach (TabuListElement tabu in tabuList)
            {
                if (tabu.TabuIterationsLeft < 1) toBeRemoved.Add(tabu);
            }
            foreach (TabuListElement tabu in toBeRemoved)
            {
                tabuList.Remove(tabu);
            }
        }

        public void ChangeListParametersForMode(string mode)
        {
            tabuList.Clear();
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
