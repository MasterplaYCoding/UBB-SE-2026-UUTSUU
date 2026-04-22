using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using OurApp.Core.Database;
using OurApp.Core.Repositories;
using OurApp.Tests.Helpers;

namespace OurApp.Tests.Integration
{
    [TestClass]
    [DoNotParallelize]
    public class JobsPaymentIntegrationTests
    {
        private const int CompetitorCompanyId = 97001;
        private const int CompetitorJobId = 97101;
        private const string IntegrationSkillName = "JobsIntegrationSkill";

        private JobsRepository _jobsRepository = null!;
        private PaymentRepository _paymentRepository = null!;

        [TestInitialize]
        public void Setup()
        {
            TestDbSeeder.Clean();
            TestDbSeeder.Seed();

            _jobsRepository = new JobsRepository();
            _paymentRepository = new PaymentRepository();

            EnsureSkillExists(IntegrationSkillName);
            EnsureJobSkillLink(TestDbSeeder.JobId, GetSkillId(IntegrationSkillName), 90);

            InsertCompany(CompetitorCompanyId, "Integration Competitor", "integration@test.com");
            InsertJob(CompetitorJobId, CompetitorCompanyId, "Integration Competitor Job", 100);
            UpdatePayment(TestDbSeeder.JobId, 120);
        }

        [TestCleanup]
        public void Cleanup()
        {
            DeleteJob(CompetitorJobId);
            DeleteCompany(CompetitorCompanyId);
            TestDbSeeder.Clean();
        }

        [TestMethod]
        public void GetAllJobs_ReturnsRelatedCompanyAndSkillsTogether()
        {
            var job = _jobsRepository.GetAllJobs().First(j => j.JobId == TestDbSeeder.JobId);

            Assert.IsNotNull(job.Company);
            Assert.AreEqual("TestCompany", job.Company.Name);
            Assert.IsTrue(job.JobSkills.Any(js => js.Skill.SkillName == IntegrationSkillName));
        }

        [TestMethod]
        public void GetCompaniesToNotify_TraversesJobsAndCompaniesTables()
        {
            var emails = _paymentRepository.GetCompaniesToNotify(TestDbSeeder.JobId, 200);

            Assert.AreEqual(1, emails.Count);
            Assert.AreEqual("integration@test.com", emails[0]);
        }

        private static void EnsureSkillExists(string skillName)
        {
            using var conn = DbConnectionHelper.GetConnection();
            conn.Open();

            using var cmd = new SqlCommand(@"
                IF NOT EXISTS (SELECT 1 FROM skills WHERE skill_name = @name)
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

            using var cmd = new SqlCommand(@"
                IF NOT EXISTS (SELECT 1 FROM job_skills WHERE job_id = @jobId AND skill_id = @skillId)
                    INSERT INTO job_skills (job_id, skill_id, required_percentage)
                    VALUES (@jobId, @skillId, @percentage)", conn);

            cmd.Parameters.AddWithValue("@jobId", jobId);
            cmd.Parameters.AddWithValue("@skillId", skillId);
            cmd.Parameters.AddWithValue("@percentage", percentage);
            cmd.ExecuteNonQuery();
        }

        private static void InsertCompany(int companyId, string companyName, string email)
        {
            using var conn = DbConnectionHelper.GetConnection();
            conn.Open();

            using var cmd = new SqlCommand(@"
                IF NOT EXISTS (SELECT 1 FROM companies WHERE company_id = @companyId)
                INSERT INTO companies (company_id, company_name, logo_picture_url, email, collaborators_count, posted_jobs_count)
                VALUES (@companyId, @companyName, 'logo.png', @email, 0, 0)", conn);

            cmd.Parameters.AddWithValue("@companyId", companyId);
            cmd.Parameters.AddWithValue("@companyName", companyName);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.ExecuteNonQuery();
        }

        private static void InsertJob(int jobId, int companyId, string title, int amountPayed)
        {
            using var conn = DbConnectionHelper.GetConnection();
            conn.Open();

            using var cmd = new SqlCommand(@"
                IF NOT EXISTS (SELECT 1 FROM jobs WHERE job_id = @jobId)
                INSERT INTO jobs
                    (job_id, company_id, job_title, industry_field, job_type, experience_level,
                     job_description, job_location, available_positions, amount_payed)
                VALUES
                    (@jobId, @companyId, @title, 'IT', 'Full-time', 'Entry Level',
                     'Integration test job', 'Remote', 1, @amountPayed)", conn);

            cmd.Parameters.AddWithValue("@jobId", jobId);
            cmd.Parameters.AddWithValue("@companyId", companyId);
            cmd.Parameters.AddWithValue("@title", title);
            cmd.Parameters.AddWithValue("@amountPayed", amountPayed);
            cmd.ExecuteNonQuery();
        }

        private static void UpdatePayment(int jobId, int amount)
        {
            using var conn = DbConnectionHelper.GetConnection();
            conn.Open();

            using var cmd = new SqlCommand("UPDATE jobs SET amount_payed = @amount WHERE job_id = @jobId", conn);
            cmd.Parameters.AddWithValue("@jobId", jobId);
            cmd.Parameters.AddWithValue("@amount", amount);
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

        private static void DeleteCompany(int companyId)
        {
            using var conn = DbConnectionHelper.GetConnection();
            conn.Open();

            using var cmd = new SqlCommand("DELETE FROM companies WHERE company_id = @companyId", conn);
            cmd.Parameters.AddWithValue("@companyId", companyId);
            cmd.ExecuteNonQuery();
        }
    }
}

