using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OurApp.Core.Models;
using OurApp.Core.Services;
using OurApp.Tests.Helpers;

namespace OurApp.Tests.Services
{
    [TestClass]
    public class CollaboratorsServiceTests
    {
        private const string EventPhotoPath = "photo.jpg";
        private const string EventTitle = "Test Event";
        private const string EventDescription = "Test Description";
        private const string EventLocation = "Cluj-Napoca";

        private const string DefaultCompanyName = "Company";
        private const string AltCompanyName = "Corporation";
        private const string Company1Name = "Company1";
        private const string Company2Name = "Company2";
        private const string SingleCompanyName = "Corp";

        private const int Year = 2026;
        private const int Month = 6;
        private const int StartDay = 1;
        private const int EndDay = 2;

        private const int DefaultId = 1;
        private const int DefaultCompanyId = 10;
        private const int ExpectedUserId = 42;
        private const int AltEventId = 5;
        private const int AltCompanyId = 99;
        private const int Company1Id = 1;
        private const int Company2Id = 2;

        private const int CountZero = 0;
        private const int CountTwo = 2;

        private FakeCollaboratorsRepo fakeCollaboratorsRepo = null!;
        private CollaboratorsService collaboratorsService = null!;

        [TestInitialize]
        public void Setup()
        {
            fakeCollaboratorsRepo = new FakeCollaboratorsRepo();
            collaboratorsService = new CollaboratorsService(fakeCollaboratorsRepo);
        }

        private static Event MakeEvent()
        {
            return new Event(
                EventPhotoPath,
                EventTitle,
                EventDescription,
                new DateTime(Year, Month, StartDay),
                new DateTime(Year, Month, EndDay),
                EventLocation,
                DefaultId,
                new List<Company>())
            { Id = DefaultId };
        }

        private static Company MakeCompany()
        {
            return new Company(DefaultCompanyName, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, DefaultCompanyId);
        }

        [TestMethod]
        public void AddCollaborator_ValidInputs_AddCollaboratorToRepo()
        {
            Event eventToCollaborateOn = MakeEvent();
            Company companyToAdd = MakeCompany();

            collaboratorsService.AddCollaborator(eventToCollaborateOn, companyToAdd, DefaultId);

            Assert.IsNotNull(fakeCollaboratorsRepo.ReceivedEvent);
        }

        [TestMethod]
        public void AddCollaborator_ValidInputs_RepoReceivesCorrectEvent()
        {
            Event eventToCollaborateOn = MakeEvent();
            Company companyToAdd = MakeCompany();

            collaboratorsService.AddCollaborator(eventToCollaborateOn, companyToAdd, DefaultId);

            Assert.AreEqual(eventToCollaborateOn, fakeCollaboratorsRepo.ReceivedEvent);
        }

        [TestMethod]
        public void AddCollaborator_ValidInputs_RepoReceivesCorrectCompany()
        {
            Event eventToCollaborateOn = MakeEvent();
            Company companyToAdd = MakeCompany();

            collaboratorsService.AddCollaborator(eventToCollaborateOn, companyToAdd, DefaultId);

            Assert.AreEqual(companyToAdd, fakeCollaboratorsRepo.ReceivedCompany);
        }

        [TestMethod]
        public void AddCollaborator_ValidInputs_RepoReceivesCorrectLoggedInUserId()
        {
            Event eventToCollaborateOn = MakeEvent();
            Company companyToAdd = MakeCompany();

            collaboratorsService.AddCollaborator(eventToCollaborateOn, companyToAdd, ExpectedUserId);

            Assert.AreEqual(ExpectedUserId, fakeCollaboratorsRepo.ReceivedLoggedInUserId);
        }

        [TestMethod]
        public void AddCollaborator_ValidInputs_RepoReceivesEventWithCorrectId()
        {
            Event eventToCollaborateOn = MakeEvent();
            eventToCollaborateOn.Id = AltEventId;
            Company companyToAdd = MakeCompany();

            collaboratorsService.AddCollaborator(eventToCollaborateOn, companyToAdd, DefaultId);

            Assert.AreEqual(AltEventId, fakeCollaboratorsRepo.ReceivedEvent?.Id);
        }

        [TestMethod]
        public void AddCollaborator_ValidInputs_RepoReceivesCompanyWithCorrectId()
        {
            Event eventToCollaborateOn = MakeEvent();
            Company companyToAdd = new Company(AltCompanyName, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, AltCompanyId);

            collaboratorsService.AddCollaborator(eventToCollaborateOn, companyToAdd, DefaultId);

            Assert.AreEqual(AltCompanyId, fakeCollaboratorsRepo.ReceivedCompany?.CompanyId);
        }

        [TestMethod]
        public void GetAllCollaborators_RepoReturnsTwoCompanies_ServiceReturnsTwoCompanies()
        {
            fakeCollaboratorsRepo.CollaboratorsToReturn = new List<Company>
            {
                new Company(Company1Name, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, Company1Id),
                new Company(Company2Name, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, Company2Id)
            };

            List<Company> result = collaboratorsService.GetAllCollaborators(DefaultId);

            Assert.AreEqual(CountTwo, result.Count);
        }

        [TestMethod]
        public void GetAllCollaborators_RepoReturnsEmptyList_ServiceReturnsEmptyList()
        {
            fakeCollaboratorsRepo.CollaboratorsToReturn = new List<Company>();

            List<Company> result = collaboratorsService.GetAllCollaborators(DefaultId);

            Assert.AreEqual(CountZero, result.Count);
        }

        [TestMethod]
        public void GetAllCollaborators_RepoReturnsOneCompany_ServiceReturnsCorrectCompanyName()
        {
            fakeCollaboratorsRepo.CollaboratorsToReturn = new List<Company>
            {
                new Company(SingleCompanyName, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, DefaultId)
            };

            List<Company> result = collaboratorsService.GetAllCollaborators(DefaultId);

            Assert.AreEqual(SingleCompanyName, result[0].Name);
        }
    }
}