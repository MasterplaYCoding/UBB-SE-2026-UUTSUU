using Microsoft.VisualStudio.TestTools.UnitTesting;
using OurApp.Core.Models;
using OurApp.Core.Repositories;
using OurApp.Core.Services;
using System;
using OurApp.Tests.Helpers;

namespace OurApp.Tests.Services
{
    [TestClass]
    public class CompanyServiceTests
    {
        private FakeCompanyRepository fakeRepo;
        private CompanyService service;

        [TestInitialize]
        public void Setup()
        {
            fakeRepo = new FakeCompanyRepository();
            service = new CompanyService(fakeRepo);
        }

        [TestMethod]
        public void AddCompany_ValidCompany_CallsRepositoryAdd()
        {
            string name = "TestCompany";
            string about = "We build software.";
            string pfp = "image.png";
            string logo = "logo.png";
            string location = "Bucharest";
            string email = "test@test.com";

            service.AddCompany(name, about, pfp, logo, location, email);

            Assert.IsNotNull(fakeRepo.LastAdded);
            Assert.AreEqual(name, fakeRepo.LastAdded.Name);
        }

        [TestMethod]
        public void AddCompany_InvalidName_ThrowsException()
        {
            string name = "";
            string about = "";
            string pfp = "";
            string logo = "logo.png";
            string location = "";
            string email = "";

            Action action = () => service.AddCompany(name, about, pfp, logo, location, email);

            Assert.ThrowsException<Exception>(action);
        }

        [TestMethod]
        public void GetCompanyById_ExistingCompany_ReturnsCompany()
        {
            Company company = new Company("Test", "", "", "logo.png", "", "");
            company.CompanyId = 1;
            fakeRepo.StoredCompany = company;

            Company result = service.GetCompanyById(1);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.CompanyId);
        }

        [TestMethod]
        public void UpdateCompany_ValidCompany_CallsRepositoryUpdate()
        {
            Company company = new Company("Test", "", "", "logo.png", "", "");
            service.UpdateCompany(company);
            Assert.IsNotNull(fakeRepo.LastUpdated);
        }

        [TestMethod]
        public void RemoveCompany_CallsRepositoryRemove()
        {
            int id = 5;
            service.RemoveCompany(id);
            Assert.AreEqual(id, fakeRepo.LastRemovedId);
        }

        [TestMethod]
        public void GetCompanyByName_ReturnsCompanyFromRepository()
        {
            Company company = new Company("Google", "", "", "logo.png", "", "");
            fakeRepo.StoredCompany = company;

            Company result = service.GetCompanyByName("Google");

            Assert.IsNotNull(result);
            Assert.AreEqual("Google", result.Name);
        }

        [TestMethod]
        public void PrintAll_CallsRepositoryPrintAll()
        {
            service.PrintAll();
            Assert.IsTrue(fakeRepo.PrintAllCalled);
        }
    }
}