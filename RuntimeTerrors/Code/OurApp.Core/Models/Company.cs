using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OurApp.Core.Models
{
    public class Company
    {
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

        public List<string> Collaborators { get; set; } = new ();

        public Company() { }
        public Company(
            string name,
            string aboutus,
            string pfpUrl,
            string logoUrl,
            string location,
            string email,
            int companyId = 1,
            int postedJobsCount = 0,
            int collaboratorsCount = 0)
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
