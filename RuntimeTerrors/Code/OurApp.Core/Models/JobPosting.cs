using System;

namespace OurApp.Core.Models
{
    public class JobPosting
    {
        public int JobId { get; set; }
        
        public Company Company { get; set; }
        
        public string Photo { get; set; }
        public string JobTitle { get; set; }//*
        public string IndustryField { get; set; }//*dropdown menu w options: IT, Business, Healthcare, Education, etc.
        public string JobType { get; set; }//*Type: dropdown menu with options - can be multiple choice (part-time, full-time, volunteer, internship, remote, hybrid, etc)
        public string ExperienceLevel { get; set; }//*Experience level: dropdown menu with options (internship, entry level, mid-senior level, director, executive)

        public DateTime? StartDate { get; set; }//*
        public DateTime? EndDate { get; set; }//*
        
        public string JobDescription { get; set; }//*
        public string JobLocation { get; set; }//*
        public int AvailablePositions { get; set; }//<0
        
        public DateTime? PostedAt { get; set; }//*Automatically getTime()
        public int? Salary { get; set; }//<0
        public int? AmountPayed { get; set; }//<=0 (0 by default)
        public DateTime? Deadline { get; set; }
        public System.Collections.Generic.ICollection<JobSkill> JobSkills { get; set; } = new System.Collections.Generic.List<JobSkill>();

        //*Required skills: checkboxes with different skills options (Python, Java, C++, etc.) and a corresponding percentage representing the minimum required knowledge for the job;

    }
}
