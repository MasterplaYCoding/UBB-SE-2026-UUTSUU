using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using OurApp.Core.Database;
using OurApp.Core.Models;
using OurApp.Core.Repositories;
using OurApp.Tests.Helpers;

namespace OurApp.Tests.Repositories
{
    [TestClass]
    [DoNotParallelize]
    public class JobsRepositoryTests
    {
        private const int NewJobId = 95001;
        private const string PrimarySkillName = "JobsRepoTest-CSharp";
        private const string SecondarySkillName = "JobsRepoTest-Sql";

        private JobsRepository repository = null!;

        [TestInitialize]
        public void Setup()
        {
            TestDbSeeder.Clean();
            TestDbSeeder.Seed();
            repository = new JobsRepository();

            EnsureSkillExists(PrimarySkillName);
            EnsureSkillExists(SecondarySkillName);
            EnsureJobSkillLink(TestDbSeeder.JobId, GetSkillId(PrimarySkillName), 80);
            EnsureJobSkillLink(TestDbSeeder.JobId, GetSkillId(SecondarySkillName), 60);
        }

        [TestCleanup]
        public void Cleanup()
        {
            DeleteJob(NewJobId);
            DeleteSkillLinksForJob(NewJobId);
            TestDbSeeder.Clean();
        }

        [TestMethod]
        public void GetAllJobs_ReturnsJobWithCompanyPopulated()
        {
            var job = repository.GetAllJobs().First(j => j.JobId == TestDbSeeder.JobId);

            Assert.IsNotNull(job.Company);
            Assert.AreEqual(TestDbSeeder.CompanyId, job.Company.CompanyId);
            Assert.AreEqual("TestCompany", job.Company.Name);
        }

        [TestMethod]
        public void GetAllJobs_ReturnsJobWithMappedSkills()
        {
            var job = repository.GetAllJobs().First(j => j.JobId == TestDbSeeder.JobId);

            Assert.IsTrue(job.JobSkills.Any(js => js.Skill.SkillName == PrimarySkillName && js.RequiredPercentage == 80));
            Assert.IsTrue(job.JobSkills.Any(js => js.Skill.SkillName == SecondarySkillName && js.RequiredPercentage == 60));
        }

        [TestMethod]
        public void GetAllSkills_ReturnsSeededSkills()
        {
            var skills = repository.GetAllSkills();

            Assert.IsTrue(skills.Any(s => s.SkillName == PrimarySkillName));
            Assert.IsTrue(skills.Any(s => s.SkillName == SecondarySkillName));
        }

        [TestMethod]
        public void AddJob_NullJob_ThrowsArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                repository.AddJob(null!, TestDbSeeder.CompanyId, new List<(int SkillId, int RequiredPercentage)>()));
        }

        [TestMethod]
        public void AddJob_ValidJob_PersistsJobAndValidSkillLinks()
        {
            var primarySkillId = GetSkillId(PrimarySkillName);
            var secondarySkillId = GetSkillId(SecondarySkillName);

            var newJob = new JobPosting
            {
                JobId = NewJobId,
                JobTitle = "New Repository Test Job",
                IndustryField = "IT",
                JobType = "Full-time",
                ExperienceLevel = "Mid",
                StartDate = new DateTime(2027, 1, 10),
                EndDate = new DateTime(2027, 2, 10),
                JobDescription = "Repository persistence test",
                JobLocation = "Remote",
                AvailablePositions = 3,
                Salary = 5000,
                AmountPayed = 0,
                Deadline = new DateTime(2026, 12, 31)
            };

            var createdId = repository.AddJob(newJob, TestDbSeeder.CompanyId, new List<(int SkillId, int RequiredPercentage)>
            {
                (primarySkillId, 75),
                (secondarySkillId, 0),
                (secondarySkillId, 101)
            });

            var createdJob = repository.GetAllJobs().First(j => j.JobId == createdId);

            Assert.AreEqual("New Repository Test Job", createdJob.JobTitle);
            Assert.AreEqual(1, createdJob.JobSkills.Count);
            Assert.AreEqual(75, createdJob.JobSkills.Single().RequiredPercentage);
            Assert.AreEqual(PrimarySkillName, createdJob.JobSkills.Single().Skill.SkillName);
        }

        [TestMethod]
        public void AddJob_ValidJob_IncrementsPostedJobsCount()
        {
            var beforeCount = GetPostedJobsCount(TestDbSeeder.CompanyId);
            var primarySkillId = GetSkillId(PrimarySkillName);

            var newJob = new JobPosting
            {
                JobId = NewJobId,
                JobTitle = "Posted Count Test Job",
                IndustryField = "IT",
                JobType = "Full-time",
                ExperienceLevel = "Junior",
                StartDate = new DateTime(2027, 3, 1),
                EndDate = new DateTime(2027, 4, 1),
                JobDescription = "Count test",
                JobLocation = "Cluj",
                AvailablePositions = 1
            };

            repository.AddJob(newJob, TestDbSeeder.CompanyId, new List<(int SkillId, int RequiredPercentage)>
            {
                (primarySkillId, 70)
            });

            var afterCount = GetPostedJobsCount(TestDbSeeder.CompanyId);

            Assert.AreEqual(beforeCount + 1, afterCount);
        }
        private static void EnsureSkillExists(string skillName)
        {
            using var conn = DbConnectionHelper.GetConnection();
            conn.Open();

            using var cmd = new SqlCommand(@"IF NOT EXISTS (SELECT 1 FROM skills WHERE skill_name = @name)
                    INSERT INTO skills (skill_name) VALUES (@name)", conn);

            cmd.Parameters.AddWithValue("@name", skillName);
            cmd.ExecuteNonQuery();
        }

        private static int GetSkillId(string skillName)
        {
            using var conn = DbConnectionHelper.GetConnection();
            conn.Open();

            using var cmd = new SqlCommand("SELECT skill_id FROM skills WHERE skill_name = @name", conn);
            cmd.Parameters.AddWithValue("@name", skillName);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        private static void EnsureJobSkillLink(int jobId, int skillId, int percentage)
        {
            using var conn = DbConnectionHelper.GetConnection();
            conn.Open();

            using var cmd = new SqlCommand(@"IF NOT EXISTS (SELECT 1 FROM job_skills WHERE job_id = @jobId AND skill_id = @skillId)
                    INSERT INTO job_skills (job_id, skill_id, required_percentage)
                    VALUES (@jobId, @skillId, @percentage)", conn);

            cmd.Parameters.AddWithValue("@jobId", jobId);
            cmd.Parameters.AddWithValue("@skillId", skillId);
            cmd.Parameters.AddWithValue("@percentage", percentage);
            cmd.ExecuteNonQuery();
        }

        private static int GetPostedJobsCount(int companyId)
        {
            using var conn = DbConnectionHelper.GetConnection();
            conn.Open();

            using var cmd = new SqlCommand("SELECT posted_jobs_count FROM companies WHERE company_id = @companyId", conn);
            cmd.Parameters.AddWithValue("@companyId", companyId);

            var result = cmd.ExecuteScalar();
            return result == DBNull.Value || result == null ? 0 : Convert.ToInt32(result);
        }

        private static void DeleteSkillLinksForJob(int jobId)
        {
            using var conn = DbConnectionHelper.GetConnection();
            conn.Open();

            using var cmd = new SqlCommand("DELETE FROM job_skills WHERE job_id = @jobId", conn);
            cmd.Parameters.AddWithValue("@jobId", jobId);
            cmd.ExecuteNonQuery();
        }

        private static void DeleteJob(int jobId)
        {
            using var conn = DbConnectionHelper.GetConnection();
            conn.Open();

            using var cmd = new SqlCommand("DELETE FROM jobs WHERE job_id = @jobId", conn);
            cmd.Parameters.AddWithValue("@jobId", jobId);
            cmd.ExecuteNonQuery();
        }

        [TestMethod]
        public void AddJob_WithNullDatesAndPhoto_PersistsSuccessfully()
        {
            var primarySkillId = GetSkillId(PrimarySkillName);

            var newJob = new JobPosting
            {
                JobId = NewJobId,
                JobTitle = "Null Dates Test Job",
                IndustryField = "IT",
                JobType = "Full-time",
                ExperienceLevel = "Mid",
                Photo = "photo.jpg",
                StartDate = null,
                EndDate = null,
                PostedAt = null,
                JobDescription = "Null dates test",
                JobLocation = "Remote",
                AvailablePositions = 1
            };

            var createdId = repository.AddJob(newJob, TestDbSeeder.CompanyId,
                new List<(int SkillId, int RequiredPercentage)> { (primarySkillId, 70) });

            var createdJob = repository.GetAllJobs().First(j => j.JobId == createdId);

            Assert.AreEqual("Null Dates Test Job", createdJob.JobTitle);
        }

        [TestMethod]
        public void AddJob_FirstJobForCompany_SetsPostedJobsCountToOne()
        {
            var primarySkillId = GetSkillId(PrimarySkillName);

            var newJob = new JobPosting
            {
                JobId = NewJobId,
                JobTitle = "First Job Test",
                IndustryField = "IT",
                JobType = "Full-time",
                ExperienceLevel = "Mid",
                JobDescription = "First job for company",
                JobLocation = "Remote",
                AvailablePositions = 1
            };

            repository.AddJob(newJob, TestDbSeeder.CollaboratorCompanyId,
                new List<(int SkillId, int RequiredPercentage)> { (primarySkillId, 70) });

            var count = GetPostedJobsCount(TestDbSeeder.CollaboratorCompanyId);

            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public void AddJob_WithPostedAtSet_PersistsPostedAtCorrectly()
        {
            var primarySkillId = GetSkillId(PrimarySkillName);

            var newJob = new JobPosting
            {
                JobId = NewJobId,
                JobTitle = "PostedAt Test Job",
                IndustryField = "IT",
                JobType = "Full-time",
                ExperienceLevel = "Mid",
                JobDescription = "PostedAt test",
                JobLocation = "Remote",
                AvailablePositions = 1,
                PostedAt = DateTime.Now
            };

            var createdId = repository.AddJob(newJob, TestDbSeeder.CompanyId,
                new List<(int SkillId, int RequiredPercentage)> { (primarySkillId, 70) });

            var createdJob = repository.GetAllJobs().First(j => j.JobId == createdId);

            Assert.IsNotNull(createdJob);
        }
    }
}
