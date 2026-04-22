using System;
using System.Collections.Generic;

namespace OurApp.Core.Models
{
    public class Scenario
    {
        public string Description { get; private set; }

        private List<AdviceChoice> _choices;
        public IReadOnlyList<AdviceChoice> AdviceChoices => _choices;

        public Scenario(string description)
        {
            Description = description;
            _choices = new List<AdviceChoice>();
        }

        public void AddChoice(AdviceChoice choice)
        {
            _choices.Add(choice);
        }

        public List<string> GetAdviceTexts()
        {
            List<string> adviceTexts = new List<string>();

            for (int i = 0; i < _choices.Count; i++)
            {
                adviceTexts.Add(_choices[i].Advice);
            }

            return adviceTexts;
        }

        public List<string> GetAdviceReactions()
        {
            List<string> adviceReactions = new List<string>();

            for (int i = 0; i < _choices.Count; i++)
            {
                adviceReactions.Add(_choices[i].Feedback);
            }

            return adviceReactions;
        }

        public string SelectChoice(int index)
        {
            if (index < 0 || index >= _choices.Count)
                throw new ArgumentOutOfRangeException(nameof(index), "Invalid choice index");

            return _choices[index].IsChosen();
        }
    }
}