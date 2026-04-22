using Microsoft.VisualStudio.TestTools.UnitTesting;
using OurApp.Core.Models;
using OurApp.Core.Services;
using OurApp.Tests.Helpers;
using System;
using System.Collections.Generic;

namespace OurApp.Tests.Services
{
    [TestClass]
    public class GameServiceTests
    {
        private FakeGameRepository repo;
        private GameService service;

        [TestInitialize]
        public void Setup()
        {
            repo = new FakeGameRepository();
            service = new GameService(repo);
        }

        private Game CreateTestGame()
        {
            var buddy = new Buddy(1, "Buddy", "Hello there");

            var scenario = new Scenario("Scenario text");
            scenario.AddChoice(new AdviceChoice("Advice 1", "Reaction 1"));
            scenario.AddChoice(new AdviceChoice("Advice 2", "Reaction 2"));

            var scenarios = new List<Scenario> { scenario };

            return new Game(buddy, scenarios, "Good ending", true);
        }

        [TestMethod]
        public void LoadedGame_ReturnsGame()
        {
            repo.StoredGame = CreateTestGame();
            var game = service.LoadedGame();
            Assert.IsNotNull(game);
        }

        [TestMethod]
        public void LoadedGame_NoGame_ThrowsException()
        {
            repo.StoredGame = null;
            Action action = () => service.LoadedGame();
            Assert.ThrowsException<InvalidOperationException>(action);
        }

        [TestMethod]
        public void GetBuddyId_ReturnsCorrectId()
        {
            repo.StoredGame = CreateTestGame();
            int id = service.getBuddyId();
            Assert.AreEqual(1, id);
        }

        [TestMethod]
        public void Save_CallsRepository()
        {
            var game = CreateTestGame();
            service.Save(game);
            Assert.AreEqual(game, repo.SavedGame);
        }

        [TestMethod]
        public void Save_NullGame_ThrowsException()
        {
            Game game = null;
            Action action = () => service.Save(game);
            Assert.ThrowsException<ArgumentNullException>(action);
        }

        [TestMethod]
        public void GetStoredGame_WhenGameExists_ReturnsGame()
        {
            repo.StoredGame = CreateTestGame();
            var result = service.GetStoredGame();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void IsPublished_ReturnsTrue()
        {
            repo.StoredGame = CreateTestGame();
            var result = service.isPublished();
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ShowCoworker_ReturnsIntroduction()
        {
            repo.StoredGame = CreateTestGame();
            var intro = service.ShowCoworker();
            Assert.AreEqual("Hello there", intro);
        }

        [TestMethod]
        public void ShowScenarioText_ReturnsText()
        {
            repo.StoredGame = CreateTestGame();
            var text = service.ShowScenarioText(0);
            Assert.AreEqual("Scenario text", text);
        }

        [TestMethod]
        public void ShowScenarioText_InvalidIndex_ThrowsException()
        {
            repo.StoredGame = CreateTestGame();
            Action action = () => service.ShowScenarioText(5);
            Assert.ThrowsException<ArgumentOutOfRangeException>(action);
        }

        [TestMethod]
        public void ShowChoices_ReturnsChoices()
        {
            repo.StoredGame = CreateTestGame();
            var choices = service.ShowChoices(0);
            Assert.AreEqual(2, choices.Count);
        }

        [TestMethod]
        public void ShowConclusion_ReturnsConclusion()
        {
            repo.StoredGame = CreateTestGame();
            var result = service.ShowConclusion();
            Assert.AreEqual("Good ending", result);
        }

        [TestMethod]
        public void PublishGame_SetsGamePublished()
        {
            var game = CreateTestGame();
            game.Unpublish();
            service.PublishGame(game);
            Assert.IsTrue(game.IsPublished);
        }

        [TestMethod]
        public void UnpublishGame_SetsGameUnpublished()
        {
            var game = CreateTestGame();
            game.Publish();   

            service.UnpublishGame(game);
            Assert.IsFalse(game.IsPublished);
        }

        [TestMethod]
        public void GetStoredGame_NoGame_ReturnsEmptyGame()
        {
            repo.StoredGame = null;
            var result = service.GetStoredGame();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void IsPublished_NoGame_ReturnsFalse()
        {
            repo.StoredGame = null;
            var result = service.isPublished();
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ShowChoices_InvalidIndex_ThrowsException()
        {
            repo.StoredGame = CreateTestGame();
            Action action = () => service.ShowChoices(5);
            Assert.ThrowsException<ArgumentOutOfRangeException>(action);
        }

        [TestMethod]
        public void ChoiceMade_ReturnsReaction()
        {
            repo.StoredGame = CreateTestGame();
            var result = service.ChoiceMade(0, 0);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void CreateGameFromInput_CreatesGameCorrectly()
        {
            var scenarios = new List<(string, IReadOnlyList<(string, string)>)>
            {
                ("Scenario 1", new List<(string,string)>
                {
                    ("Advice","Reaction")
                }),
            
                ("Scenario 2", new List<(string,string)>
                {
                    ("Advice2","Reaction2")
                })
            };

            var game = service.CreateGameFromInput(
                1,
                "Buddy",
                "Hello",
                scenarios,
                "Good ending",
                true
            );

            Assert.IsNotNull(game);
            Assert.AreEqual("Good ending", game.Conclusion);
        }

        [TestMethod]
        public void ChoiceMade_InvalidScenarioIndex_ThrowsException()
        {
            repo.StoredGame = CreateTestGame();
            Action action = () => service.ChoiceMade(-1, 0);
            Assert.ThrowsException<ArgumentOutOfRangeException>(action);
        }
    }
}