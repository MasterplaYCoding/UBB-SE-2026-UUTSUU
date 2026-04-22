using OurApp.Core.Models;
using OurApp.Core.Repositories;

namespace OurApp.Tests.Helpers
{
    internal class FakeJobsRepository : IJobsRepository
    {
        public List<JobPosting> Jobs = new List<JobPosting>();
        public List<Skill> Skills = new List<Skill>();

        public IEnumerable<JobPosting> GetAllJobs()
        {
            return Jobs;
        }

        public IReadOnlyList<Skill> GetAllSkills()
        {
            return Skills;
        }

        public int AddJob(JobPosting job, int companyId, IReadOnlyList<(int SkillId, int RequiredPercentage)> skillLinks)
        {
            Jobs.Add(job);
            return job.JobId;
        }
    }
}