using OurApp.Core.Models;
using OurApp.Core.Repositories;
using OurApp.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OurApp.Tests.Repositories
{
    [TestClass]
    [DoNotParallelize]
    public class ApplicantRepositoryTests
    {
        private ApplicantRepository? sut;

        [TestInitialize]
        public void Setup()
        {
            TestDbSeeder.Clean();
            TestDbSeeder.Seed();
            sut = new ApplicantRepository();
        }

        [TestCleanup]
        public void Cleanup()
        {
            TestDbSeeder.Clean();
        }

        private static Applicant MakeApplicant()
        {
            return new Applicant
            {
                ApplicantId = TestDbSeeder.ApplicantId,
                Job = new JobPosting { JobId = TestDbSeeder.JobId },
                User = new User(TestDbSeeder.UserId, "Test User", "test@test.com"),
                ApplicationStatus = "OnHold",
                AppliedAt = DateTime.UtcNow
            };
        }

        [TestMethod]
        public void AddApplicant_ValidApplicant_CanBeRetrievedAfterwards()
        {
            sut.RemoveApplicant(TestDbSeeder.ApplicantId);
            sut.AddApplicant(MakeApplicant());

            var result = sut.GetApplicantById(TestDbSeeder.ApplicantId);
            Assert.IsNotNull(result);
        }
        
        [TestMethod]
        public void AddApplicant_ValidApplicant_StoresCorrectId()
        {
            sut.RemoveApplicant(TestDbSeeder.ApplicantId);
            sut.AddApplicant(MakeApplicant());

            var result = sut.GetApplicantById(TestDbSeeder.ApplicantId);
            Assert.AreEqual(TestDbSeeder.ApplicantId, result.ApplicantId);
        }

        [TestMethod]
        public void AddApplicant_ValidApplicant_StoresCorrectStatus()
        {
            sut.RemoveApplicant(TestDbSeeder.ApplicantId);
            sut.AddApplicant(MakeApplicant());

            var result = sut.GetApplicantById(TestDbSeeder.ApplicantId);
            Assert.AreEqual("OnHold", result.ApplicationStatus);
        }

        [TestMethod]
        public void GetApplicantById_ExistingId_ReturnsApplicant()
        {
            var result = sut.GetApplicantById(TestDbSeeder.ApplicantId);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void GetApplicantById_ExistingId_ReturnsCorrectId()
        {
            var result = sut.GetApplicantById(TestDbSeeder.ApplicantId);
            Assert.AreEqual(TestDbSeeder.ApplicantId, result.ApplicantId);
        }

        [TestMethod]
        public void GetApplicantById_NonExistingId_ReturnsNull()
        {
            var result = sut.GetApplicantById(99999);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetApplicantsByJob_ExistingJob_ReturnsApplicants()
        {
            var job = new JobPosting { JobId = TestDbSeeder.JobId };
            var result = sut.GetApplicantsByJob(job);
            Assert.IsTrue(result.Any());
        }

        [TestMethod]
        public void GetApplicantsByJob_NullJob_ReturnsEmptyList()
        {
            var result = sut.GetApplicantsByJob(null);
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public void GetApplicantsByJob_NonExistingJob_ReturnsEmptyList()
        {
            var job = new JobPosting { JobId = 99999 };

            var result = sut.GetApplicantsByJob(job);
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public void GetApplicantsByCompany_ExistingCompany_ReturnsApplicants()
        {
            var result = sut.GetApplicantsByCompany(TestDbSeeder.CompanyId);
            Assert.IsTrue(result.Any());
        }

        [TestMethod]
        public void GetApplicantsByCompany_NonExistingCompany_ReturnsEmptyList()
        {
            var result = sut.GetApplicantsByCompany(99999);
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public void UpdateApplicant_ChangesGrade_GradeIsPersistedToDb()
        {
            var applicant = sut.GetApplicantById(TestDbSeeder.ApplicantId);
            applicant.AppTestGrade = 8.5m;

            sut.UpdateApplicant(applicant);
            var result = sut.GetApplicantById(TestDbSeeder.ApplicantId);

            Assert.AreEqual(8.5m, result.AppTestGrade);
        }

        [TestMethod]
        public void UpdateApplicant_ChangesStatus_StatusIsPersistedToDb()
        {
            var applicant = sut.GetApplicantById(TestDbSeeder.ApplicantId);
            applicant.ApplicationStatus = "Rejected";

            sut.UpdateApplicant(applicant);
            var result = sut.GetApplicantById(TestDbSeeder.ApplicantId);

            Assert.AreEqual("Rejected", result.ApplicationStatus);
        }

        [TestMethod]
        public void UpdateApplicant_SetsAllGrades_AllGradesPersistedToDb()
        {
            var applicant = sut.GetApplicantById(TestDbSeeder.ApplicantId);
            applicant.AppTestGrade = 9.0m;
            applicant.CvGrade = 8.5m;
            applicant.CompanyTestGrade = 7.5m;
            applicant.InterviewGrade = 8.0m;

            sut.UpdateApplicant(applicant);
            var result = sut.GetApplicantById(TestDbSeeder.ApplicantId);

            Assert.AreEqual(9.0m, result.AppTestGrade);
            Assert.AreEqual(8.5m, result.CvGrade);
            Assert.AreEqual(7.5m, result.CompanyTestGrade);
            Assert.AreEqual(8.0m, result.InterviewGrade);
        }

        [TestMethod]
        public void RemoveApplicant_ExistingApplicant_CanNoLongerBeRetrieved()
        {
            sut.RemoveApplicant(TestDbSeeder.ApplicantId);
            
            var result = sut.GetApplicantById(TestDbSeeder.ApplicantId);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void AddApplicant_WithNullOptionalFields_CanBeRetrievedAfterwards()
        {
            sut.RemoveApplicant(TestDbSeeder.ApplicantId);
            var applicant = new Applicant
            {
                ApplicantId = TestDbSeeder.ApplicantId,
                Job = null,
                User = new User(TestDbSeeder.UserId, "Test User", "test@test.com"),
                ApplicationStatus = null,
                AppTestGrade = null,
                CvGrade = null,
                CompanyTestGrade = null,
                InterviewGrade = null,
                RecommendedFromCompany = null,
                AppliedAt = DateTime.UtcNow
            };

            sut.AddApplicant(applicant);
            var result = sut.GetApplicantById(TestDbSeeder.ApplicantId);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void AddApplicant_WithRecommendedCompany_CanBeRetrievedAfterwards()
        {
            sut.RemoveApplicant(TestDbSeeder.ApplicantId);
            var applicant = new Applicant
            {
                ApplicantId = TestDbSeeder.ApplicantId,
                Job = new JobPosting { JobId = TestDbSeeder.JobId },
                User = new User(TestDbSeeder.UserId, "Test User", "test@test.com"),
                ApplicationStatus = "OnHold",
                RecommendedFromCompany = new Company { CompanyId = TestDbSeeder.CollaboratorCompanyId },
                AppliedAt = DateTime.UtcNow
            };

            sut.AddApplicant(applicant);
            var result = sut.GetApplicantById(TestDbSeeder.ApplicantId);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void AddApplicant_WithAllGrades_StoresGradesCorrectly()
        {
            sut.RemoveApplicant(TestDbSeeder.ApplicantId);
            var applicant = new Applicant
            {
                ApplicantId = TestDbSeeder.ApplicantId,
                Job = new JobPosting { JobId = TestDbSeeder.JobId },
                User = new User(TestDbSeeder.UserId, "Test User", "test@test.com"),
                ApplicationStatus = "OnHold",
                AppTestGrade = 8.0m,
                CvGrade = 7.5m,
                CompanyTestGrade = 9.0m,
                InterviewGrade = 8.5m,
                AppliedAt = DateTime.UtcNow
            };

            sut.AddApplicant(applicant);
            var result = sut.GetApplicantById(TestDbSeeder.ApplicantId);

            Assert.AreEqual(8.0m, result.AppTestGrade);
        }

        [TestMethod]
        public void UpdateApplicant_WithNullStatus_CanBeRetrievedAfterwards()
        {
            var applicant = sut.GetApplicantById(TestDbSeeder.ApplicantId);
            applicant.ApplicationStatus = null;

            sut.UpdateApplicant(applicant);
            var result = sut.GetApplicantById(TestDbSeeder.ApplicantId);

            Assert.IsNull(result.ApplicationStatus);
        }
    }
}