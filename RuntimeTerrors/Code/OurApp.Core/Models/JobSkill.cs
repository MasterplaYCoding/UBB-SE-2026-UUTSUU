using System;

namespace OurApp.Core.Models
{
    public class JobSkill
    {
        public Skill Skill { get; set; }
        public JobPosting Job { get; set; }
        public int RequiredPercentage { get; set; }
    }
}
