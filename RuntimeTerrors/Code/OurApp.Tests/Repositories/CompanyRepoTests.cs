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
            TestDbSeeder.Seed();
            repo = new CompanyRepo();
        }

        [TestCleanup]
        public void Cleanup()
        {
            TestDbSeeder.Clean();
        }

        [TestMethod]
        public void GetCompanyById_ExistingCompany_ReturnsCompany()
        {
            // Arrange
            int id = TestDbSeeder.CompanyId;

            // Act
            var result = ((ICompanyRepo)repo).GetById(id);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(id, result.CompanyId);
        }

        [TestMethod]
        public void GetCompanyByName_ExistingCompany_ReturnsCompany()
        {
            // Arrange
            string name = "TestCompany";

            // Act
            var result = repo.GetCompanyByName(name);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(name, result.Name);
        }

        [TestMethod]
        public void GetAll_ReturnsSeededCompany()
        {
            // Act
            var companies = ((ICompanyRepo)repo).GetAll();

            // Assert
            Assert.IsTrue(companies.Count > 0);
        }

        [TestMethod]
        public void Remove_RemovesCompanyFromDatabase()
        {
            // Arrange
            int id = TestDbSeeder.CompanyId;

            // Act
            ((ICompanyRepo)repo).Remove(id);
            var result = ((ICompanyRepo)repo).GetById(id);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void Add_AddsCompanyToDatabase()
        {
            var company = new Company(
                "NewCompany",
                "",
                "",
                "logo.png",
                "",
                ""
            );

            // create minimal game structure required by repo
            var buddy = new Buddy(1, "Buddy", "Hello");
            var scenario = new Scenario("Scenario text");
            scenario.AddChoice(new AdviceChoice("Advice", "Reaction"));
            scenario.AddChoice(new AdviceChoice("Advice2", "Reaction2"));
            scenario.AddChoice(new AdviceChoice("Advice3", "Reaction3"));

            var scenarios = new List<Scenario> { scenario, scenario };

            company.game = new Game(buddy, scenarios, "Conclusion", false);

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

            // minimal game needed for repo update
            var buddy = new Buddy(1, "Buddy", "Hello");
            var scenario = new Scenario("Scenario text");
            scenario.AddChoice(new AdviceChoice("Advice", "Reaction"));
            scenario.AddChoice(new AdviceChoice("Advice2", "Reaction2"));
            scenario.AddChoice(new AdviceChoice("Advice3", "Reaction3"));

            var scenarios = new List<Scenario> { scenario, scenario };

            company.game = new Game(buddy, scenarios, "Conclusion", false);

            ((ICompanyRepo)repo).Update(company);

            var result = repo.GetCompanyByName("UpdatedCompany");

            Assert.IsNotNull(result);
        }
    }
}