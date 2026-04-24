using System;
using System.Collections.Generic;
using OurApp.Core.Models;
using OurApp.Core.Repositories;
using OurApp.Tests.Helpers;

namespace OurApp.Tests.Repositories
{
    [TestClass]
    [DoNotParallelize]
    public class CollaboratorsRepoTests
    {
        private CollaboratorsRepo collaboratorsRepo;
        private EventsRepo eventsRepo;
        private CompanyRepo companyRepo;
        private Event insertedEvent;

        private static Event MakeEvent()
        {
            return new Event(
                null,
                "Collaborator Test Event",
                "Test description",
                new DateTime(2027, 2, 1),
                new DateTime(2027, 2, 2),
                "Cluj-Napoca",
                TestDbSeeder.CompanyId,
                new List<Company>());
        }

        [TestInitialize]
        public void Setup()
        {
            collaboratorsRepo = new CollaboratorsRepo();
            eventsRepo = new EventsRepo();
            companyRepo = new CompanyRepo();
            insertedEvent = MakeEvent();
            TestDbSeeder.Seed();
        }

        [TestCleanup]
        public void TearDown()
        {
            eventsRepo.RemoveEventFromRepo(insertedEvent);
            TestDbSeeder.Clean();
        }
        [TestMethod]
        public void GetAllCollaborators_SeededCompanyId_ReturnsNotNull()
        {
            List<Company> result = collaboratorsRepo.GetAllCollaborators(TestDbSeeder.CompanyId);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void GetAllCollaborators_NonExistentCompanyId_ReturnsEmptyList()
        {
            int nonExistentCompanyId = -999;
            List<Company> result = collaboratorsRepo.GetAllCollaborators(nonExistentCompanyId);
            Assert.AreEqual(0, result.Count);
        }
        [TestMethod]
        public void AddCollaboratorToRepo_ValidInputs_CollaboratorsListCountIncreasesByOne()
        {
            eventsRepo.AddEventToRepo(insertedEvent);
            int countBefore = collaboratorsRepo.GetAllCollaborators(TestDbSeeder.CompanyId).Count;

            var collaborator = new Company("TestCollaboratorCompany", string.Empty, string.Empty, "logo.png", string.Empty, string.Empty)
            {
                CompanyId = TestDbSeeder.CollaboratorCompanyId
            };
            collaboratorsRepo.AddCollaboratorToRepo(insertedEvent, collaborator, TestDbSeeder.CompanyId);

            int countAfter = collaboratorsRepo.GetAllCollaborators(TestDbSeeder.CompanyId).Count;
            Assert.AreEqual(countBefore + 1, countAfter);
        }

        [TestMethod]
        public void AddCollaboratorToRepo_ValidInputs_AddedCollaboratorHasCorrectName()
        {
            eventsRepo.AddEventToRepo(insertedEvent);

            var collaborator = new Company("TestCollaboratorCompany", string.Empty, string.Empty, "logo.png", string.Empty, string.Empty)
            {
                CompanyId = TestDbSeeder.CollaboratorCompanyId
            };
            collaboratorsRepo.AddCollaboratorToRepo(insertedEvent, collaborator, TestDbSeeder.CompanyId);

            List<Company> result = collaboratorsRepo.GetAllCollaborators(TestDbSeeder.CompanyId);
            Assert.AreEqual("TestCollaboratorCompany", result[0].Name);
        }

        [TestMethod]
        public void AddCollaboratorToRepo_FirstTimeCollaboration_HostCollaboratorsCountIncreasesByOne()
        {
            eventsRepo.AddEventToRepo(insertedEvent);

            var companyBefore = companyRepo.GetCompanyByName("TestCompany");
            int countBefore = companyBefore.CollaboratorsCount;

            var collaborator = new Company("TestCollaboratorCompany", string.Empty, string.Empty, "logo.png", string.Empty, string.Empty)
            {
                CompanyId = TestDbSeeder.CollaboratorCompanyId
            };
            collaboratorsRepo.AddCollaboratorToRepo(insertedEvent, collaborator, TestDbSeeder.CompanyId);

            var companyAfter = companyRepo.GetCompanyByName("TestCompany");
            int countAfter = companyAfter.CollaboratorsCount;

            Assert.AreEqual(countBefore + 1, countAfter);
        }

        [TestMethod]
        public void AddCollaboratorToRepo_InvalidEventId_ThrowsException()
        {
            var invalidEvent = new Event(null, "Invalid", "desc",
                new DateTime(2027, 1, 1), new DateTime(2027, 1, 2),
                "Cluj-Napoca", TestDbSeeder.CompanyId, new List<Company>());
            invalidEvent.Id = 99999;

            var collaborator = new Company("TestCollaboratorCompany", string.Empty, string.Empty, "logo.png", string.Empty, string.Empty)
            {
                CompanyId = TestDbSeeder.CollaboratorCompanyId
            };

            Assert.ThrowsException<Microsoft.Data.SqlClient.SqlException>(() =>
                collaboratorsRepo.AddCollaboratorToRepo(invalidEvent, collaborator, TestDbSeeder.CompanyId));
        }
    }
}