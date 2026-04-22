using OurApp.Core.Models;
using OurApp.Core.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;

namespace OurApp.Core.Services
{
    public class ProfileCompletionCalculator
    {
        //Company has:
        //    this.Name = name;
        //    this.AboutUs = aboutus;
        //    this.Pfp_url = pfp_url;
        //    this.Logo_url = logo_url;
        //    this.Location = location;
        //    this.Email = email;

        private readonly IJobsRepository _jobsRepository;
        private readonly IApplicantRepository _applRepository;

        public ProfileCompletionCalculator(IJobsRepository jobsRepository, IApplicantRepository applicantRepository)
        {
            _jobsRepository = jobsRepository;
            _applRepository = applicantRepository;
        }

            public (int percentage, List<string> remainingTasks) Calculate(Company company)
        {
            int total = 5;
            int done = 0;
            var tasks = new List<string>();

            if (!string.IsNullOrEmpty(company.ProfilePicturePath)) done++;
            else tasks.Add("Upload company picture");

            if (!string.IsNullOrEmpty(company.AboutUs)) done++;
            else tasks.Add("Add company description");

            if (company.PostedJobsCount >= 5) done++;
            else tasks.Add("Post at least 5 jobs");

            if (company.CollaboratorsCount >= 2 || company.CollaboratorsCount >= 2) done++;
            else tasks.Add("Add 2 collaborators");

            if (IsMiniGameComplete(company.Game)) done++;
            else tasks.Add("Complete mini-game");

            return ((done * 100) / total, tasks);
        }

        private static bool IsMiniGameComplete(Game g)
        {
            return g != null && g.IsPublished;
        }

        public (List<string> skillNames, List<int> percents) GetSkillsTop3(int companyId)
        {
            var jobs = _jobsRepository
                .GetAllJobs()
                .Where(j => j.Company != null && j.Company.CompanyId == companyId)
                .ToList();

            var skillCounts = new Dictionary<string, int>();
            int total = 0;

            foreach (var job in jobs)
            {
                if (job.JobSkills == null) continue;

                foreach (var js in job.JobSkills)
                {
                    var name = js.Skill?.SkillName;
                    if (string.IsNullOrEmpty(name)) continue;

                    if (!skillCounts.ContainsKey(name))
                        skillCounts[name] = 0;

                    skillCounts[name] += js.RequiredPercentage;
                    total += js.RequiredPercentage;
                }
            }

            var skillNames = new List<string>();
            var percents = new List<int>();

            if (total == 0)
                return (skillNames, percents);

            var top3 = skillCounts
                .OrderByDescending(kv => kv.Value)
                .Take(3);

            foreach (var kv in top3)
            {
                skillNames.Add(kv.Key);
                percents.Add((int)Math.Round((double)kv.Value * 100 / total));
            }

            return (skillNames, percents);
        }

        public string applicantsMessage(int companyId)
        {
            var applicants = _applRepository.GetApplicantsByCompany(companyId);

            int current = applicants
                .Count(a => a.AppliedAt >= DateTime.Now.AddDays(-7));

            int previous = applicants
                .Count(a => a.AppliedAt < DateTime.Now.AddDays(-7));

            if (previous == 0)
            {
                if (current == 0)
                    return "No applicants yet. Start posting jobs!";

                return $"Great start! You have {current} new applicants.";
            }

            double change = ((double)(current - previous) / previous) * 100;

            if (change < 0)
            {
                return $"You have {Math.Abs((int)change)}% fewer applicants than last week.";
            }
            else
            {
                return $"Congrats! You have {(int)change}% more applicants than last week.";
            }
        }
    }
}
