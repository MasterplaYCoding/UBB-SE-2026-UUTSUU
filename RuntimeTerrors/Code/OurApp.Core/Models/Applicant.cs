using System;

namespace OurApp.Core.Models
{
    public class Applicant
    {
        public int ApplicantId { get; set; }
        public JobPosting Job { get; set; } = null!;
        public User User { get; set; } = null!;

        // Grades start as null until evaluated
        public decimal? AppTestGrade { get; set; }
        public decimal? CvGrade { get; set; }
        public decimal? CompanyTestGrade { get; set; }
        public decimal? InterviewGrade { get; set; }

        //"Failed", "On Hold", "Accepted", "Recommended"
        public string ApplicationStatus { get; set; } = null;
        public DateTime AppliedAt { get; set; }
        public Company RecommendedFromCompany { get; set; }
    }
}
