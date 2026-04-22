using OurApp.Core.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OurApp.Tests.Validators
{
    [TestClass]
    public class EventsValidatorTests
    {
        [TestClass]
        public class EventValidatorTests
        {
            private EventValidator eventValidator;

            [TestInitialize]
            public void Setup()
            {
                eventValidator = new EventValidator();
            }
            [TestMethod]
            public void IsEventTitleValid_NormalTitle_ReturnsTrue()
            { 
                string title = "Conference";
                bool result = eventValidator.IsEventTitleValid(title);
                Assert.IsTrue(result);
            }

            [TestMethod]
            public void IsEventTitleValid_EmptyTitle_ThrowsException()
            {
                string title = "";
                var exception = Assert.ThrowsException<Exception>(() =>
                    eventValidator.IsEventTitleValid(title));
                Assert.AreEqual("Title is mandatory", exception.Message);
            }

            [TestMethod]
            public void IsEventTitleValid_TitleExactly200Characters_ReturnsTrue()
            {
                string title = new string('A', 200);
                bool result = eventValidator.IsEventTitleValid(title);
                Assert.IsTrue(result);
            }

            [TestMethod]
            public void IsEventTitleValid_Title201Characters_ThrowsException()
            {

                string title = new string('A', 201);
                var exception = Assert.ThrowsException<Exception>(() =>
                    eventValidator.IsEventTitleValid(title));
                Assert.AreEqual("Title is too long", exception.Message);
            }
            [TestMethod]
            public void IsEventDescriptionValid_NormalDescription_ReturnsTrue()
            {
                string description = "A very nice event.";
                bool result = eventValidator.IsEventDescriptionValid(description);
                Assert.IsTrue(result);
            }

            [TestMethod]
            public void IsEventDescriptionValid_EmptyDescription_ReturnsTrue()
            {
                string description = "";
                bool result = eventValidator.IsEventDescriptionValid(description);
                Assert.IsTrue(result);
            }

            [TestMethod]
            public void IsEventDescriptionValid_DescriptionExactly2000Characters_ReturnsTrue()
            {
                string description = new string('X', 2000);
                bool result = eventValidator.IsEventDescriptionValid(description);
                Assert.IsTrue(result);
            }

            [TestMethod]
            public void IsEventDescriptionValid_Description2001Characters_ThrowsException()
            {
                string description = new string('A', 2001);
                var exception = Assert.ThrowsException<Exception>(() =>
                    eventValidator.IsEventDescriptionValid(description));
                Assert.AreEqual("Description is too long", exception.Message);
            }
            [TestMethod]
            public void IsEventLocationValid_NormalLocation_ReturnsTrue()
            {
                // Arrange
                string location = "Cluj-Napoca";

                // Act
                bool result = eventValidator.IsEventLocationValid(location);

                // Assert
                Assert.IsTrue(result);
            }

            [TestMethod]
            public void IsEventLocationValid_EmptyLocation_ThrowsException()
            {
                // Arrange
                string location = "";

                // Act
                var exception = Assert.ThrowsException<Exception>(() =>
                    eventValidator.IsEventLocationValid(location));

                // Assert
                Assert.AreEqual("Location is mandatory", exception.Message);
            }

            [TestMethod]
            public void IsEventLocationValid_LocationExactly300Characters_ReturnsTrue()
            {
                // Arrange
                // Code: length > 300 throws, so exactly 300 must return true
                string location = new string('L', 300);

                // Act
                bool result = eventValidator.IsEventLocationValid(location);

                // Assert
                Assert.IsTrue(result);
            }

            [TestMethod]
            public void IsEventLocationValid_Location301Characters_ThrowsException()
            {
                string location = new string('L', 301);
                var exception = Assert.ThrowsException<Exception>(() =>
                    eventValidator.IsEventLocationValid(location));
                Assert.AreEqual("Location is too long", exception.Message);
            }
            [TestMethod]
            public void IsEventStartDateValid_FutureDate_ReturnsTrue()
            {
                DateTimeOffset startDate = DateTimeOffset.Now.AddDays(1);
                bool result = eventValidator.IsEventStartDateValid(startDate);
                Assert.IsTrue(result);
            }

            [TestMethod]
            public void IsEventStartDateValid_NullDate_ThrowsException()
            {
                DateTimeOffset? startDate = null;
                var exception = Assert.ThrowsException<Exception>(() =>
                    eventValidator.IsEventStartDateValid(startDate));
                Assert.AreEqual("Starting date is mandatory", exception.Message);
            }

            [TestMethod]
            public void IsEventStartDateValid_PastDate_ThrowsException()
            {
                DateTimeOffset startDate = DateTimeOffset.Now.AddDays(-1);
                var exception = Assert.ThrowsException<Exception>(() =>
                    eventValidator.IsEventStartDateValid(startDate));
                Assert.AreEqual("Event must start after creation", exception.Message);
            }
            [TestMethod]
            public void IsEventEndDateValid_FutureDate_ReturnsTrue()
            {
                DateTimeOffset endDate = DateTimeOffset.Now.AddDays(5);
                bool result = eventValidator.IsEventEndDateValid(endDate);
                Assert.IsTrue(result);
            }

            [TestMethod]
            public void IsEventEndDateValid_NullDate_ThrowsException()
            {
                DateTimeOffset? endDate = null;
                var exception = Assert.ThrowsException<Exception>(() =>
                    eventValidator.IsEventEndDateValid(endDate));
                Assert.AreEqual("Ending date is mandatory", exception.Message);
            }

            [TestMethod]
            public void IsEventEndDateValid_PastDate_ThrowsException()
            {
                DateTimeOffset endDate = DateTimeOffset.Now.AddDays(-1);
                var exception = Assert.ThrowsException<Exception>(() =>
                    eventValidator.IsEventEndDateValid(endDate));
                Assert.AreEqual("Event must end after creation", exception.Message);
            }
            [TestMethod]
            public void AreEventDatesCronologicallyValid_StartBeforeEnd_ReturnsTrue()
            { 
                DateTimeOffset startDate = DateTimeOffset.Now.AddDays(1);
                DateTimeOffset endDate = DateTimeOffset.Now.AddDays(3);
                bool result = eventValidator.AreEventDatesCronologicallyValid(startDate, endDate);
                Assert.IsTrue(result);
            }

            [TestMethod]
            public void AreEventDatesCronologicallyValid_StartAfterEnd_ThrowsException()
            {
                DateTimeOffset startDate = DateTimeOffset.Now.AddDays(5);
                DateTimeOffset endDate = DateTimeOffset.Now.AddDays(1);
                var exception = Assert.ThrowsException<Exception>(() =>
                    eventValidator.AreEventDatesCronologicallyValid(startDate, endDate));
                Assert.AreEqual("Event must begin before ending", exception.Message);
            }

            [TestMethod]
            public void AreEventDatesCronologicallyValid_StartEqualsEnd_ReturnsTrue()
            {
                DateTimeOffset date = DateTimeOffset.Now.AddDays(2);
                bool result = eventValidator.AreEventDatesCronologicallyValid(date, date);
                Assert.IsTrue(result);
            }
        }
    }
}
