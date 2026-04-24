using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OurApp.Core.Models;
using OurApp.Core.Services;
using OurApp.Tests.Helpers;

namespace OurApp.Tests.Services
{
    [TestClass]
    public class CompanyServiceTests
    {
        private const string ValidCompanyName = "TestCompany";
        private const string ValidAbout = "We build software.";
        private const string ValidPfp = "image.png";
        private const string ValidLogo = "logo.png";
        private const string ValidLocation = "Bucharest";
        private const string ValidEmail = "test@test.com";

        private const string ShortTestName = "Test";
        private const string SpecificCompanyName = "Google";

        private const int ValidCompanyId = 1;
        private const int RemoveCompanyId = 5;

        private FakeCompanyRepository fakeRepo = null!;
        private CompanyService service = null!;

        [TestInitialize]
        public void Setup()
        {
            fakeRepo = new FakeCompanyRepository();
            service = new CompanyService(fakeRepo);
        }

        [TestMethod]
        public void AddCompany_ValidCompany_CallsRepositoryAdd()
        {
            service.AddCompany(ValidCompanyName, ValidAbout, ValidPfp, ValidLogo, ValidLocation, ValidEmail);

            Assert.IsNotNull(fakeRepo.LastAdded);
            Assert.AreEqual(ValidCompanyName, fakeRepo.LastAdded.Name);
        }

        [TestMethod]
        public void AddCompany_InvalidName_ThrowsException()
        {
            Action action = () => service.AddCompany(string.Empty, string.Empty, string.Empty, ValidLogo, string.Empty, string.Empty);

            Assert.ThrowsException<Exception>(action);
        }

        [TestMethod]
        public void GetCompanyById_ExistingCompany_ReturnsCompany()
        {
            Company company = new Company(ShortTestName, string.Empty, string.Empty, ValidLogo, string.Empty, string.Empty);
            company.CompanyId = ValidCompanyId;
            fakeRepo.StoredCompany = company;

            Company result = service.GetCompanyById(ValidCompanyId);

            Assert.IsNotNull(result);
            Assert.AreEqual(ValidCompanyId, result.CompanyId);
        }

        [TestMethod]
        public void UpdateCompany_ValidCompany_CallsRepositoryUpdate()
        {
            Company company = new Company(ShortTestName, string.Empty, string.Empty, ValidLogo, string.Empty, string.Empty);
            service.UpdateCompany(company);
            Assert.IsNotNull(fakeRepo.LastUpdated);
        }

        [TestMethod]
        public void RemoveCompany_CallsRepositoryRemove()
        {
            service.RemoveCompany(RemoveCompanyId);
            Assert.AreEqual(RemoveCompanyId, fakeRepo.LastRemovedId);
        }

        [TestMethod]
        public void GetCompanyByName_ReturnsCompanyFromRepository()
        {
            Company company = new Company(SpecificCompanyName, string.Empty, string.Empty, ValidLogo, string.Empty, string.Empty);
            fakeRepo.StoredCompany = company;

            Company result = service.GetCompanyByName(SpecificCompanyName);

            Assert.IsNotNull(result);
            Assert.AreEqual(SpecificCompanyName, result.Name);
        }

        [TestMethod]
        public void PrintAll_CallsRepositoryPrintAll()
        {
            service.PrintAll();
            Assert.IsTrue(fakeRepo.PrintAllCalled);
        }
    }
}