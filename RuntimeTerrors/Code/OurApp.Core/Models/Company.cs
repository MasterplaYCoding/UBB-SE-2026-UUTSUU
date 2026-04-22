using System;
using System.Collections.Generic;

namespace OurApp.Core.Models
{
    public class Company
    {
        private const int DefaultCompanyId = 1;
        private const int DefaultPostedJobsCount = 0;
        private const int DefaultCollaboratorsCount = 0;

        public int CompanyId { get; set; }
        public string Name { get; set; }
        public string AboutUs { get; set; }
        public string ProfilePicturePath { get; set; }
        public string CompanyLogoPath { get; set; }
        public string Location { get; set; }
        public string Email { get; set; }

        public Game Game { get; set; }
        public int PostedJobsCount { get; set; }
        public int CollaboratorsCount { get; set; }

        public List<string> Collaborators { get; set; } = new();

        public Company() { }

        public Company(
            string name,
            string aboutus,
            string pfpUrl,
            string logoUrl,
            string location,
            string email,
            int companyId = DefaultCompanyId,
            int postedJobsCount = DefaultPostedJobsCount,
            int collaboratorsCount = DefaultCollaboratorsCount)
        {
            CompanyId = companyId;
            Name = name ?? string.Empty;
            AboutUs = aboutus ?? string.Empty;
            ProfilePicturePath = pfpUrl ?? string.Empty;
            CompanyLogoPath = logoUrl ?? string.Empty;
            Location = location ?? string.Empty;
            Email = email ?? string.Empty;
            PostedJobsCount = postedJobsCount;
            CollaboratorsCount = collaboratorsCount;
        }

        public override string ToString()
        {
            return $"Company[{CompanyId}]: {Name}, {Email}";
        }
    }
}