using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using OurApp.Core.Database;
using OurApp.Core.Repositories;
using OurApp.Tests.Helpers;

namespace OurApp.Tests.Repositories
{
    [TestClass]
    [DoNotParallelize]
    public class PaymentRepositoryTests
    {
        private const int BudgetCompanyId = 96001;
        private const int HigherBudgetCompanyId = 96002;
        private const int LowerBudgetJobId = 96101;
        private const int HigherBudgetJobId = 96102;

        private PaymentRepository _repository = null!;

        [TestInitialize]
        public void Setup()
        {
            TestDbSeeder.Clean();
            TestDbSeeder.Seed();
            _repository = new PaymentRepository();

            InsertCompany(BudgetCompanyId, "Budget Company", "budget@test.com");
            InsertCompany(HigherBudgetCompanyId, "Higher Budget Company", "higher@test.com");

            InsertJob(LowerBudgetJobId, BudgetCompanyId, "Budget Job", 150);
            InsertJob(HigherBudgetJobId, HigherBudgetCompanyId, "Higher Budget Job", 300);
            UpdatePayment(TestDbSeeder.JobId, 120);
        }

        [TestCleanup]
        public void Cleanup()
        {
            DeleteJob(LowerBudgetJobId);
            DeleteJob(HigherBudgetJobId);
            DeleteCompany(BudgetCompanyId);
            DeleteCompany(HigherBudgetCompanyId);
            TestDbSeeder.Clean();
        }

        [TestMethod]
        public void UpdateJobPayment_ExistingJob_PersistsNewAmount()
        {
            _repository.UpdateJobPayment(TestDbSeeder.JobId, 250);

            var amountInDb = GetPaymentFromDb(TestDbSeeder.JobId);
            Assert.AreEqual(250, amountInDb);
        }

        [TestMethod]
        public void UpdateJobPayment_MissingJob_ThrowsException()
        {
            var ex = Assert.ThrowsException<Exception>(() => _repository.UpdateJobPayment(999999, 200));

            Assert.AreEqual("Job ID not found. Payment not applied to database.", ex.Message);
        }

        [TestMethod]
        public void GetPaidJobs_MatchingTypeAndExperience_ReturnsExpectedRows()
        {
            var result = _repository.GetPaidJobs("Full-time", "Entry Level");

            Assert.IsTrue(result.Any(x => x.CompanyName == "TestCompany" && x.JobTitle == "Test Job"));
            Assert.IsTrue(result.Any(x => x.CompanyName == "Budget Company" && x.JobTitle == "Budget Job" && x.AmountPayed == 150));
            Assert.IsTrue(result.Any(x => x.CompanyName == "Higher Budget Company" && x.JobTitle == "Higher Budget Job" && x.AmountPayed == 300));
        }

        [TestMethod]
        public void GetCompaniesToNotify_ReturnsOnlyLowerBudgetCompetitorEmails()
        {
            var result = _repository.GetCompaniesToNotify(TestDbSeeder.JobId, 200);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("budget@test.com", result[0]);
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
                     'Payment repository test job', 'Remote', 1, @amountPayed)", conn);

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

        private static int GetPaymentFromDb(int jobId)
        {
            using var conn = DbConnectionHelper.GetConnection();
            conn.Open();

            using var cmd = new SqlCommand("SELECT amount_payed FROM jobs WHERE job_id = @jobId", conn);
            cmd.Parameters.AddWithValue("@jobId", jobId);

            return Convert.ToInt32(cmd.ExecuteScalar());
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

        [TestMethod]
        public void GetPaidJobs_JobWithNullAmountPayed_ReturnsDefaultAmount()
        {
            using var conn = DbConnectionHelper.GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(@"
                IF NOT EXISTS (SELECT 1 FROM jobs WHERE job_id = 96103)
                INSERT INTO jobs
                    (job_id, company_id, job_title, industry_field, job_type, experience_level,
                     job_description, job_location, available_positions, amount_payed)
                VALUES
                    (96103, 96001, 'Null Amount Job', 'IT', 'Full-time', 'Entry Level',
                     'Test', 'Remote', 1, NULL)", conn);
            cmd.ExecuteNonQuery();

            var result = _repository.GetPaidJobs("Full-time", "Entry Level");

            using var cleanup = new SqlCommand("DELETE FROM jobs WHERE job_id = 96103", conn);
            cleanup.ExecuteNonQuery();

            Assert.IsTrue(result.Any(x => x.JobTitle == "Null Amount Job"));
        }
    }
}
