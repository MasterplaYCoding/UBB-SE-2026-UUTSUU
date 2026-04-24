using System;
using System.Collections.Generic;
using OurApp.Core.Models;
using OurApp.Core.Repositories;
using OurApp.Tests.Helpers;

namespace OurApp.Tests.Integration
{
    // Integration tests – Event + Collaborator + Company tables working together
    [TestClass]
    [DoNotParallelize]
    public class EventCollaboratorIntegrationTests
    {
        private EventsRepo eventsRepo;
        private CollaboratorsRepo collaboratorsRepo;
        private CompanyRepo companyRepo;
        private Event insertedEvent;

        private static Event MakeEvent()
        {
            return new Event(
                null,
                "Integration Test Event",
                "Description",
                new DateTime(2027, 3, 1),
                new DateTime(2027, 3, 2),
                "Cluj-Napoca",
                TestDbSeeder.CompanyId,
                new List<Company>());
        }

        [TestInitialize]
        public void Setup()
        {
            eventsRepo = new EventsRepo();
            collaboratorsRepo = new CollaboratorsRepo();
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
        public void AddEventToRepo_WithCollaborator_CollaboratorAppearsInGetAllCollaborators()
        {
            Company collaborator = new Company("Collab Co", string.Empty, string.Empty, "logo.png", string.Empty, string.Empty, TestDbSeeder.CompanyId + 1);
            insertedEvent = new Event(
                null, "Integration Test Event", "Description",
                new DateTime(2027, 3, 1), new DateTime(2027, 3, 2),
                "Cluj-Napoca", TestDbSeeder.CompanyId,
                new List<Company> { collaborator });
            int countBefore = collaboratorsRepo.GetAllCollaborators(TestDbSeeder.CompanyId).Count;
            eventsRepo.AddEventToRepo(insertedEvent);
            int countAfter = collaboratorsRepo.GetAllCollaborators(TestDbSeeder.CompanyId).Count;
            Assert.AreEqual(countBefore + 1, countAfter);
        }

        [TestMethod]
        public void AddEventToRepo_WithNewCollaborator_CompanyCollaboratorsCountIncreasesByOne()
        {
            Company collaborator = new Company("Collab Co", string.Empty, string.Empty, "logo.png", string.Empty, string.Empty, TestDbSeeder.CompanyId + 1);
            insertedEvent = new Event(
                null, "Integration Test Event", "Description",
                new DateTime(2027, 3, 1), new DateTime(2027, 3, 2),
                "Cluj-Napoca", TestDbSeeder.CompanyId,
                new List<Company> { collaborator });
            int countBefore = ((ICompanyRepo)companyRepo).GetById(TestDbSeeder.CompanyId).CollaboratorsCount;
            eventsRepo.AddEventToRepo(insertedEvent);
            int countAfter = ((ICompanyRepo)companyRepo).GetById(TestDbSeeder.CompanyId).CollaboratorsCount;

            Assert.AreEqual(countBefore + 1, countAfter);
        }

        [TestMethod]
        public void AddCollaboratorToRepo_AfterAddingEvent_CollaboratorAppearsInGetAllCollaborators()
        {
            eventsRepo.AddEventToRepo(insertedEvent);
            Company collaborator = new Company("Collab Co", string.Empty, string.Empty, "logo.png", string.Empty, string.Empty, TestDbSeeder.CompanyId + 1);
            int countBefore = collaboratorsRepo.GetAllCollaborators(TestDbSeeder.CompanyId).Count;
            collaboratorsRepo.AddCollaboratorToRepo(insertedEvent, collaborator, TestDbSeeder.CompanyId);
            int countAfter = collaboratorsRepo.GetAllCollaborators(TestDbSeeder.CompanyId).Count;
            Assert.AreEqual(countBefore + 1, countAfter);
        }

        [TestMethod]
        public void AddCollaboratorToRepo_FirstTimeCollaboration_CompanyCollaboratorsCountIncreasesByOne()
        {
            eventsRepo.AddEventToRepo(insertedEvent);
            Company collaborator = new Company("Collab Co", string.Empty, string.Empty, "logo.png", string.Empty, string.Empty, TestDbSeeder.CompanyId + 1);
            int countBefore = ((ICompanyRepo)companyRepo).GetById(TestDbSeeder.CompanyId).CollaboratorsCount;
            collaboratorsRepo.AddCollaboratorToRepo(insertedEvent, collaborator, TestDbSeeder.CompanyId);
            int countAfter = ((ICompanyRepo)companyRepo).GetById(TestDbSeeder.CompanyId).CollaboratorsCount;
            Assert.AreEqual(countBefore + 1, countAfter);
        }

        [TestMethod]
        public void RemoveEventFromRepo_AfterAddingCollaborator_CollaboratorNoLongerAppearsInGetAll()
        {
            eventsRepo.AddEventToRepo(insertedEvent);
            Company collaborator = new Company("Collab Co", string.Empty, string.Empty, "logo.png", string.Empty, string.Empty, TestDbSeeder.CompanyId + 1);
            collaboratorsRepo.AddCollaboratorToRepo(insertedEvent, collaborator, TestDbSeeder.CompanyId);
            int countBefore = collaboratorsRepo.GetAllCollaborators(TestDbSeeder.CompanyId).Count;
            eventsRepo.RemoveEventFromRepo(insertedEvent);
            int countAfter = collaboratorsRepo.GetAllCollaborators(TestDbSeeder.CompanyId).Count;
            insertedEvent = MakeEvent();
            Assert.AreEqual(countBefore - 1, countAfter);
        }
    }
}