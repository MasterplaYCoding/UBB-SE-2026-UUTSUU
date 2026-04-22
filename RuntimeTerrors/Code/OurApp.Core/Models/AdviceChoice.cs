using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OurApp.Core.Models
{
    public class AdviceChoice
    {
        public string Advice { get; private set; }
        public string Feedback { get; private set; }

        public AdviceChoice(string advice, string feedback)
        {
            Advice = advice;
            Feedback = feedback;
        }

        public string IsChosen()
        {
            return Feedback;
        }
    }
}
