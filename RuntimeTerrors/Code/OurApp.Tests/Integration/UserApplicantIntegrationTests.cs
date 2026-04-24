using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OurApp.Core.Models;
using OurApp.Core.Repositories;
using OurApp.Tests.Helpers;

namespace OurApp.Tests.Integration
{
    [TestClass]
    [DoNotParallelize]
    public class UserApplicantIntegrationTests
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

        [TestMethod]
        public void GetApplicantById_ReturnsApplicantWithUserPopulated()
        {
            var result = sut.GetApplicantById(TestDbSeeder.ApplicantId);
            Assert.IsNotNull(result.User);
        }

        [TestMethod]
        public void GetApplicantById_ReturnsApplicantWithCorrectUserName()
        {
            var result = sut.GetApplicantById(TestDbSeeder.ApplicantId);
            Assert.AreEqual("Test User", result.User.Name);
        }

        [TestMethod]
        public void GetApplicantById_ReturnsApplicantWithCorrectUserEmail()
        {
            var result = sut.GetApplicantById(TestDbSeeder.ApplicantId);
            Assert.AreEqual("test@test.com", result.User.Email);
        }

        [TestMethod]
        public void GetApplicantById_ReturnsApplicantWithCorrectUserId()
        {
            var result = sut.GetApplicantById(TestDbSeeder.ApplicantId);
            Assert.AreEqual(TestDbSeeder.UserId, result.User.Id);
        }

        [TestMethod]
        public void GetApplicantsByJob_ReturnsApplicantsWithUserPopulated()
        {
            var job = new JobPosting { JobId = TestDbSeeder.JobId };

            var result = sut.GetApplicantsByJob(job);
            Assert.IsTrue(result.All(a => a.User != null));
        }

        [TestMethod]
        public void GetApplicantsByCompany_ReturnsApplicantsWithUserPopulated()
        {
            var result = sut.GetApplicantsByCompany(TestDbSeeder.CompanyId);
            Assert.IsTrue(result.All(a => a.User != null));
        }

        [TestMethod]
        public void GetApplicantById_ReturnsApplicantWithJobPopulated()
        {
            var result = sut.GetApplicantById(TestDbSeeder.ApplicantId);
            Assert.IsNotNull(result.Job);
        }

        [TestMethod]
        public void GetApplicantById_ReturnsApplicantWithCorrectJobId()
        {
            var result = sut.GetApplicantById(TestDbSeeder.ApplicantId);
            Assert.AreEqual(TestDbSeeder.JobId, result.Job.JobId);
        }
    }
}