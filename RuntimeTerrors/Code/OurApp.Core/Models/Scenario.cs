using System;
using System.Collections.Generic;

namespace OurApp.Core.Models
{
    public class Scenario
    {
        public string Description { get; private set; }

        private List<AdviceChoice> choices;
        public IReadOnlyList<AdviceChoice> AdviceChoices => choices;

        public Scenario(string description)
        {
            Description = description;
            choices = new List<AdviceChoice>();
        }

        public void AddChoice(AdviceChoice choice)
        {
            choices.Add(choice);
        }

        public List<string> GetAdviceTexts()
        {
            List<string> adviceTexts = new List<string>();

            for (int i = 0; i < choices.Count; i++)
            {
                adviceTexts.Add(choices[i].Advice);
            }

            return adviceTexts;
        }

        public List<string> GetAdviceReactions()
        {
            List<string> adviceReactions = new List<string>();

            for (int i = 0; i < choices.Count; i++)
            {
                adviceReactions.Add(choices[i].Feedback);
            }

            return adviceReactions;
        }

        public string SelectChoice(int index)
        {
            if (index < 0 || index >= choices.Count)
                throw new ArgumentOutOfRangeException(nameof(index), "Invalid choice index");

            return choices[index].IsChosen();
        }
    }
}