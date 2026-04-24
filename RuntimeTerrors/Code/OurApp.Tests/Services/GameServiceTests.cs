using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OurApp.Core.Models;
using OurApp.Core.Services;
using OurApp.Tests.Helpers;

namespace OurApp.Tests.Services
{
    [TestClass]
    public class GameServiceTests
    {
        private const string DefaultBuddyName = "Buddy";
        private const string DefaultBuddyIntro = "Hello there";
        private const string AltBuddyIntro = "Hello";
        private const string DefaultScenarioText = "Scenario text";
        private const string AdviceOne = "Advice 1";
        private const string ReactionOne = "Reaction 1";
        private const string AdviceTwo = "Advice 2";
        private const string ReactionTwo = "Reaction 2";
        private const string DefaultConclusion = "Good ending";

        private const string ScenarioOneText = "Scenario 1";
        private const string AltAdviceOne = "Advice";
        private const string AltReactionOne = "Reaction";
        private const string ScenarioTwoText = "Scenario 2";
        private const string AltAdviceTwo = "Advice2";
        private const string AltReactionTwo = "Reaction2";

        private const int DefaultBuddyId = 1;
        private const int ValidIndex = 0;
        private const int ExpectedChoicesCount = 2;
        private const int InvalidIndexPositive = 5;
        private const int InvalidIndexNegative = -1;

        private FakeGameRepository repo = null!;
        private GameService service = null!;

        [TestInitialize]
        public void Setup()
        {
            repo = new FakeGameRepository();
            service = new GameService(repo);
        }

        private Game CreateTestGame()
        {
            var buddy = new Buddy(DefaultBuddyId, DefaultBuddyName, DefaultBuddyIntro);

            var scenario = new Scenario(DefaultScenarioText);
            scenario.AddChoice(new AdviceChoice(AdviceOne, ReactionOne));
            scenario.AddChoice(new AdviceChoice(AdviceTwo, ReactionTwo));

            var scenarios = new List<Scenario> { scenario };

            return new Game(buddy, scenarios, DefaultConclusion, true);
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
            int id = service.GetBuddyId();
            Assert.AreEqual(DefaultBuddyId, id);
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
            Game game = null!;
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
            var result = service.IsPublished();
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ShowCoworker_ReturnsIntroduction()
        {
            repo.StoredGame = CreateTestGame();
            var intro = service.ShowCoworker();
            Assert.AreEqual(DefaultBuddyIntro, intro);
        }

        [TestMethod]
        public void ShowScenarioText_ReturnsText()
        {
            repo.StoredGame = CreateTestGame();
            var text = service.ShowScenarioText(ValidIndex);
            Assert.AreEqual(DefaultScenarioText, text);
        }

        [TestMethod]
        public void ShowScenarioText_InvalidIndex_ThrowsException()
        {
            repo.StoredGame = CreateTestGame();
            Action action = () => service.ShowScenarioText(InvalidIndexPositive);
            Assert.ThrowsException<ArgumentOutOfRangeException>(action);
        }

        [TestMethod]
        public void ShowChoices_ReturnsChoices()
        {
            repo.StoredGame = CreateTestGame();
            var choices = service.ShowChoices(ValidIndex);
            Assert.AreEqual(ExpectedChoicesCount, choices.Count);
        }

        [TestMethod]
        public void ShowConclusion_ReturnsConclusion()
        {
            repo.StoredGame = CreateTestGame();
            var result = service.ShowConclusion();
            Assert.AreEqual(DefaultConclusion, result);
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
            var result = service.IsPublished();
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ShowChoices_InvalidIndex_ThrowsException()
        {
            repo.StoredGame = CreateTestGame();
            Action action = () => service.ShowChoices(InvalidIndexPositive);
            Assert.ThrowsException<ArgumentOutOfRangeException>(action);
        }

        [TestMethod]
        public void ChoiceMade_ReturnsReaction()
        {
            repo.StoredGame = CreateTestGame();
            var result = service.ChoiceMade(ValidIndex, ValidIndex);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void CreateGameFromInput_CreatesGameCorrectly()
        {
            var scenarios = new List<(string, IReadOnlyList<(string, string)>)>
            {
                (ScenarioOneText, new List<(string, string)>
                {
                    (AltAdviceOne, AltReactionOne)
                }),

                (ScenarioTwoText, new List<(string, string)>
                {
                    (AltAdviceTwo, AltReactionTwo)
                })
            };

            var game = service.CreateGameFromInput(
                DefaultBuddyId,
                DefaultBuddyName,
                AltBuddyIntro,
                scenarios,
                DefaultConclusion,
                true);

            Assert.IsNotNull(game);
            Assert.AreEqual(DefaultConclusion, game.Conclusion);
        }

        [TestMethod]
        public void ChoiceMade_InvalidScenarioIndex_ThrowsException()
        {
            repo.StoredGame = CreateTestGame();
            Action action = () => service.ChoiceMade(InvalidIndexNegative, ValidIndex);
            Assert.ThrowsException<ArgumentOutOfRangeException>(action);
        }
    }
}