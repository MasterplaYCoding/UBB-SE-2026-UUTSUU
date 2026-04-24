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

            for (int index = 0; index < choices.Count; index++)
            {
                adviceTexts.Add(choices[index].Advice);
            }

            return adviceTexts;
        }

        public List<string> GetAdviceReactions()
        {
            List<string> adviceReactions = new List<string>();

            for (int index = 0; index < choices.Count; index++)
            {
                adviceReactions.Add(choices[index].Feedback);
            }

            return adviceReactions;
        }

        public string SelectChoice(int index)
        {
            if (index < 0 || index >= choices.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Invalid choice index");
            }

            return choices[index].IsChosen();
        }
    }
}