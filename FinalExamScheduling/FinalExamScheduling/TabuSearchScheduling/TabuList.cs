using System;
using System.Collections.Generic;
using System.Linq;

namespace FinalExamScheduling.TabuSearchScheduling
{
    //Used for storing tabu steps (elements)
    class TabuList
    {
        //The list of the tabu steps (TabuListElement)
        private List<TabuListElement> tabuElements;

        //This is stored to limit the possible elements in the list at a time
        private int listLength;

        public TabuList()
        {
            tabuElements = new List<TabuListElement>();
            ChangeListParameter_Mode(TSParameters.Mode);
        }

        //Returns the tabu list
        public List<TabuListElement> GetTabuList() { return tabuElements; }

        //Adds a new list element
        public void Add(TabuListElement element)
        {
            foreach (TabuListElement tabu in tabuElements)
            {
                //If the same element was already in the list, the old one is removed, and the new one is added (TTL will be reset). 
                if (tabu.Exam.IsEqualExam(element.Exam) && tabu.ExamSlot.Equals(element.ExamSlot))
                {
                    tabuElements.Remove(tabu);
                    tabuElements.Add(element);
                    return;
                }
            }

            //If the list has reached the max capacity, the first element is disposed of, before adding a new element
            if (tabuElements.Count < listLength) tabuElements.Add(element);
            else
            {
                tabuElements.RemoveAt(0);
                tabuElements.Add(element);
            }
        }

        //Decreases the TTL (iterations left of lifecycle, "Time-to-Live") for each elemt in the list. Elements that reach 0 are removed by RemoveInactive
        public void DecreaseIterationsLeft()
        {
            if (tabuElements.Count > 0)
            {
                foreach (TabuListElement element in tabuElements) element.TabuIterationsLeft -= 1;
                
                RemoveInactive();
            }
        }

        //Elements with 0 TTL are removed from the tabu list
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

        //This is used to set the tabu list to the correct length in the beginning and during mode changing (Tandem)
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
