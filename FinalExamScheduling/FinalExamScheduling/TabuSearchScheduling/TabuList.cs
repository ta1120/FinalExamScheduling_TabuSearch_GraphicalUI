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
            switch (TSParameters.Mode)
            {
                case "Random":
                    listLength = TSParameters.Random.TabuListLength;
                    break;
                case "Greedy":
                    listLength = TSParameters.Greedy.TabuListLength;
                    break;
                default:
                    listLength = TSParameters.Greedy.TabuListLength;
                    break;
            }

            tabuList = new List<TabuListElement>();
        }

        public List<TabuListElement> GetTabuList() { return tabuList.ToList(); }

        public void Add(TabuListElement element)
        {
            foreach (TabuListElement tabu in tabuList)
            {
                if (tabu.Attribute.Equals(element.Attribute) && tabu.Value.Equals(element.Value))
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
                //TODO: remove elements reaching 0
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

        public void PrintTabuList()
        {
            foreach (TabuListElement element in tabuList) Console.WriteLine("Tabu: " + element.Attribute + " - " + element.Value + " - TTL:" + element.TabuIterationsLeft);
        }

        public void ChangeListParametersForMode(string mode)
        {
            tabuList.Clear();
            switch (mode)
            {
                case "Random":
                    listLength = TSParameters.Random.TabuListLength;
                    break;
                case "Greedy":
                    listLength = TSParameters.Greedy.TabuListLength;
                    break;
                default:
                    listLength = TSParameters.Random.TabuListLength;
                    break;
            }
        }
    }
}
