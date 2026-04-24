using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OurApp.Core.Models;
using OurApp.Core.Repositories;
using OurApp.Tests.Helpers;

namespace OurApp.Tests.Repositories
{
    [TestClass]
    [DoNotParallelize]
    public class CompanyRepoTests
    {
        private CompanyRepo repo;

        [TestInitialize]
        public void Setup()
        {
            TestDbSeeder.Clean();
            TestDbSeeder.Seed();
            repo = new CompanyRepo();
        }

        [TestCleanup]
        public void Cleanup()
        {
            TestDbSeeder.Clean();
        }

        private Game CreateDummyGame()
        {
            var buddy = new Buddy(1, "Buddy", "Hello");

            var scenario = new Scenario("Scenario text");
            scenario.AddChoice(new AdviceChoice("Advice1", "Reaction1"));
            scenario.AddChoice(new AdviceChoice("Advice2", "Reaction2"));
            scenario.AddChoice(new AdviceChoice("Advice3", "Reaction3"));

            var scenarios = new List<Scenario> { scenario, scenario };

            return new Game(buddy, scenarios, "Conclusion", true);
        }

        [TestMethod]
        public void GetCompanyById_ExistingCompany_ReturnsCompany()
        {
            int id = TestDbSeeder.CompanyId;

            var result = ((ICompanyRepo)repo).GetById(id);

            Assert.IsNotNull(result);
            Assert.AreEqual(id, result.CompanyId);
        }

        [TestMethod]
        public void GetCompanyByName_ExistingCompany_ReturnsCompany()
        {
            var result = repo.GetCompanyByName("TestCompany");
            Assert.IsNotNull(result);
            Assert.AreEqual("TestCompany", result.Name);
        }

        [TestMethod]
        public void GetAll_ReturnsSeededCompanies()
        {
            var companies = ((ICompanyRepo)repo).GetAll();
            Assert.IsTrue(companies.Count > 0);
        }

        [TestMethod]
        public void Remove_RemovesCompanyFromDatabase()
        {
            int id = TestDbSeeder.CompanyId;
            ((ICompanyRepo)repo).Remove(id);
            var result = ((ICompanyRepo)repo).GetById(id);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void Add_AddsCompanyToDatabase()
        {
            var company = new Company("NewCompany", string.Empty, string.Empty, "logo.png", string.Empty, string.Empty);

            company.Game = CreateDummyGame();

            ((ICompanyRepo)repo).Add(company);

            var result = repo.GetCompanyByName("NewCompany");
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Update_UpdatesCompanyName()
        {
            var company = ((ICompanyRepo)repo).GetById(TestDbSeeder.CompanyId);
            Assert.IsNotNull(company);

            company.Name = "UpdatedCompany";
            company.Game = CreateDummyGame();

            ((ICompanyRepo)repo).Update(company);

            var result = repo.GetCompanyByName("UpdatedCompany");
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void GetGame_WhenCompanyLoaded_ReturnsGame()
        {
            ((ICompanyRepo)repo).GetById(TestDbSeeder.CompanyId);
            var game = repo.GetGame();
            Assert.IsNotNull(game);
        }

        [TestMethod]
        public void SaveGame_WhenCompanyLoaded_SavesWithoutException()
        {
            ((ICompanyRepo)repo).GetById(TestDbSeeder.CompanyId);
            var game = CreateDummyGame();
            repo.SaveGame(game);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void PrintAll_WhenCalled_DoesNotThrow()
        {
            repo.PrintAll();
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Add_InvalidCompanyName_ThrowsException()
        {
            var company = new Company(string.Empty, string.Empty, string.Empty, "logo.png", string.Empty, string.Empty);
            company.Game = CreateDummyGame();
            Assert.ThrowsException<System.ArgumentException>(() => ((ICompanyRepo)repo).Add(company));
        }

        [TestMethod]
        public void GetCompanyByName_InvalidName_ReturnsNull()
        {
            var result = repo.GetCompanyByName(string.Empty);
            Assert.IsNull(result);
        }
    }
}