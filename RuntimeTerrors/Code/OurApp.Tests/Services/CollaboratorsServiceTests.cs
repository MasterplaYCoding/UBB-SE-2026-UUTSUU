using OurApp.Core.Models;
using OurApp.Core.Services;
using OurApp.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OurApp.Tests.Services
{
    [TestClass]
    public class CollaboratorsServiceTests
    {
        private FakeCollaboratorsRepo fakeCollaboratorsRepo;
        private CollaboratorsService collaboratorsService;

        private static Event MakeEvent()
        {
            return new Event(
                "photo.jpg",
                "Test Event",
                "Test Description",
                new DateTime(2026, 6, 1),
                new DateTime(2026, 6, 2),
                "Cluj-Napoca",
                1,
                new List<Company>()
            )
            { Id = 1 };
        }

        private static Company MakeCompany()
        {
            return new Company("Company", "", "", "", "", "", 10);
        }

        [TestInitialize]
        public void Setup()
        {
            fakeCollaboratorsRepo = new FakeCollaboratorsRepo();
            collaboratorsService = new CollaboratorsService(fakeCollaboratorsRepo);
        }
        [TestMethod]
        public void AddCollaborator_ValidInputs_AddCollaboratorToRepo()
        {
            Event eventToCollaborateOn = MakeEvent();
            Company companyToAdd = MakeCompany();
            collaboratorsService.AddCollaborator(eventToCollaborateOn, companyToAdd, 1);
            Assert.IsNotNull(fakeCollaboratorsRepo.ReceivedEvent);
        }

        [TestMethod]
        public void AddCollaborator_ValidInputs_RepoReceivesCorrectEvent()
        {
            Event eventToCollaborateOn = MakeEvent();
            Company companyToAdd = MakeCompany();
            collaboratorsService.AddCollaborator(eventToCollaborateOn, companyToAdd, 1);
            Assert.AreEqual(eventToCollaborateOn, fakeCollaboratorsRepo.ReceivedEvent);
        }

        [TestMethod]
        public void AddCollaborator_ValidInputs_RepoReceivesCorrectCompany()
        {
            Event eventToCollaborateOn = MakeEvent();
            Company companyToAdd = MakeCompany();
            collaboratorsService.AddCollaborator(eventToCollaborateOn, companyToAdd, 1);
            Assert.AreEqual(companyToAdd, fakeCollaboratorsRepo.ReceivedCompany);
        }

        [TestMethod]
        public void AddCollaborator_ValidInputs_RepoReceivesCorrectLoggedInUserId()
        {
            Event eventToCollaborateOn = MakeEvent();
            Company companyToAdd = MakeCompany();
            int expectedUserId = 42;
            collaboratorsService.AddCollaborator(eventToCollaborateOn, companyToAdd, expectedUserId);
            Assert.AreEqual(expectedUserId, fakeCollaboratorsRepo.ReceivedLoggedInUserId);
        }

        [TestMethod]
        public void AddCollaborator_ValidInputs_RepoReceivesEventWithCorrectId()
        {
            Event eventToCollaborateOn = MakeEvent();
            eventToCollaborateOn.Id = 5;
            Company companyToAdd = MakeCompany();
            collaboratorsService.AddCollaborator(eventToCollaborateOn, companyToAdd, 1);
            Assert.AreEqual(5, fakeCollaboratorsRepo.ReceivedEvent.Id);
        }

        [TestMethod]
        public void AddCollaborator_ValidInputs_RepoReceivesCompanyWithCorrectId()
        {
            Event eventToCollaborateOn = MakeEvent();
            Company companyToAdd = new Company("Corporation", "", "", "", "", "", 99);
            collaboratorsService.AddCollaborator(eventToCollaborateOn, companyToAdd, 1);
            Assert.AreEqual(99, fakeCollaboratorsRepo.ReceivedCompany.CompanyId);
        }
        [TestMethod]
        public void GetAllCollaborators_RepoReturnsTwoCompanies_ServiceReturnsTwoCompanies()
        {
            fakeCollaboratorsRepo.CollaboratorsToReturn = new List<Company>
            {
                new Company("Company1", "", "", "", "", "", 1),
                new Company("Company2", "", "", "", "", "", 2)
            };

            List<Company> result = collaboratorsService.GetAllCollaborators(1);
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public void GetAllCollaborators_RepoReturnsEmptyList_ServiceReturnsEmptyList()
        { 
            fakeCollaboratorsRepo.CollaboratorsToReturn = new List<Company>();
            List<Company> result = collaboratorsService.GetAllCollaborators(1);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetAllCollaborators_RepoReturnsOneCompany_ServiceReturnsCorrectCompanyName()
        { 
            fakeCollaboratorsRepo.CollaboratorsToReturn = new List<Company>
            {
                new Company("Corp", "", "", "", "", "", 1)
            };
            List<Company> result = collaboratorsService.GetAllCollaborators(1);
            Assert.AreEqual("Corp", result[0].Name);
        }
    }
}
