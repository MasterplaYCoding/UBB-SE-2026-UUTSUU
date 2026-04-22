using OurApp.Core.Models;
using OurApp.Core.Repositories;
using OurApp.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OurApp.Tests.Repositories
{
    [TestClass]
    [DoNotParallelize]
    public class EventsRepoTests
    {
        private EventsRepo eventsRepo;
        private Event insertedEvent;
        private static Event MakeCurrentEvent()
        {
            return new Event(
                null,
                "Test Event",
                "Test description",
                new DateTime(2027, 1, 1),
                new DateTime(2027, 1, 2),
                "Cluj-Napoca",
                TestDbSeeder.CompanyId,
                new List<Company>()
            );
        }
        private static Event MakePastEvent()
        {
            return new Event(
                null,
                "Past Test Event",
                "Past test description",
                new DateTime(2020, 1, 1),
                new DateTime(2020, 1, 2),
                "Cluj-Napoca",
                TestDbSeeder.CompanyId,
                new List<Company>()
            );
        }

        [TestInitialize]
        public void Setup()
        {
            eventsRepo = new EventsRepo();
            insertedEvent = MakeCurrentEvent();
            TestDbSeeder.Seed();
        }

        [TestCleanup]
        public void TearDown()
        {
            eventsRepo.RemoveEventFromRepo(insertedEvent);
            TestDbSeeder.Clean();
        }
        [TestMethod]
        public void AddEventToRepo_ValidEvent_SetsEventId()
        {
            eventsRepo.AddEventToRepo(insertedEvent);
            Assert.IsTrue(insertedEvent.Id > 0);
        }

        [TestMethod]
        public void AddEventToRepo_ValidEvent_InsertedEventTitleIsPersistedCorrectly()
        {
            eventsRepo.AddEventToRepo(insertedEvent);
            var result = eventsRepo.getCurrentEventsFromRepo(TestDbSeeder.CompanyId);
            Assert.AreEqual("Test Event", result[0].Title);
        }

        [TestMethod]
        public void AddEventToRepo_ValidEvent_InsertedEventLocationIsPersistedCorrectly()
        {
            eventsRepo.AddEventToRepo(insertedEvent);
            var result = eventsRepo.getCurrentEventsFromRepo(TestDbSeeder.CompanyId);
            Assert.AreEqual("Cluj-Napoca", result[0].Location);
        }

        [TestMethod]
        public void AddEventToRepo_ValidEvent_InsertedEventStartDateIsPersistedCorrectly()
        {
            eventsRepo.AddEventToRepo(insertedEvent);
            var result = eventsRepo.getCurrentEventsFromRepo(TestDbSeeder.CompanyId);
            Assert.AreEqual(new DateTime(2027, 1, 1), result[0].StartDate);
        }

        [TestMethod]
        public void AddEventToRepo_ValidEvent_InsertedEventEndDateIsPersistedCorrectly()
        {

            eventsRepo.AddEventToRepo(insertedEvent);
            var result = eventsRepo.getCurrentEventsFromRepo(TestDbSeeder.CompanyId);
            Assert.AreEqual(new DateTime(2027, 1, 2), result[0].EndDate);
        }

        [TestMethod]
        public void RemoveEventFromRepo_ExistingEvent_CurrentEventsCountDecreases()
        {
            eventsRepo.AddEventToRepo(insertedEvent);
            int countBefore = eventsRepo.getCurrentEventsFromRepo(TestDbSeeder.CompanyId).Count;
            eventsRepo.RemoveEventFromRepo(insertedEvent);
            int countAfter = eventsRepo.getCurrentEventsFromRepo(TestDbSeeder.CompanyId).Count;
            insertedEvent = MakeCurrentEvent();
            Assert.AreEqual(countBefore - 1, countAfter);
        }

        [TestMethod]
        public void UpdateEventToRepo_ValidData_UpdatedTitleIsPersistedInDb()
        {
            eventsRepo.AddEventToRepo(insertedEvent);
            eventsRepo.UpdateEventToRepo(insertedEvent.Id, null, "Updated Title", "Test description", new DateTime(2027, 1, 1), new DateTime(2027, 1, 2), "Cluj-Napoca");

            var result = eventsRepo.getCurrentEventsFromRepo(TestDbSeeder.CompanyId);
            var updatedEvent = result.First(e => e.Id == insertedEvent.Id);

            Assert.AreEqual("Updated Title", updatedEvent.Title);
        }

        [TestMethod]
        public void UpdateEventToRepo_ValidData_UpdatedLocationIsPersistedInDb()
        {
            eventsRepo.AddEventToRepo(insertedEvent);
            eventsRepo.UpdateEventToRepo(insertedEvent.Id, null, "Updated Title", "Test description", new DateTime(2027, 1, 1), new DateTime(2027, 1, 2), "Bucharest");

            var result = eventsRepo.getCurrentEventsFromRepo(TestDbSeeder.CompanyId);
            var updatedEvent = result.First(e => e.Id == insertedEvent.Id);

            Assert.AreEqual("Bucharest", updatedEvent.Location);
        }

        [TestMethod]
        public void UpdateEventToRepo_ValidData_UpdatedStartDateIsPersistedInDb()
        {
            eventsRepo.AddEventToRepo(insertedEvent);
            DateTime expectedStartDate = new DateTime(2027, 6, 1);
            eventsRepo.UpdateEventToRepo(insertedEvent.Id, null, "Test Event", "Test description", expectedStartDate, new DateTime(2027, 6, 2), "Cluj-Napoca");

            var result = eventsRepo.getCurrentEventsFromRepo(TestDbSeeder.CompanyId);
            var updatedEvent = result.First(e => e.Id == insertedEvent.Id);

            Assert.AreEqual(expectedStartDate, updatedEvent.StartDate);
        }

        [TestMethod]
        public void GetCurrentEventsFromRepo_AfterAddingCurrentEvent_CountIncreasesByOne()
        {
            int countBefore = eventsRepo.getCurrentEventsFromRepo(TestDbSeeder.CompanyId).Count;
            eventsRepo.AddEventToRepo(insertedEvent);
            int countAfter = eventsRepo.getCurrentEventsFromRepo(TestDbSeeder.CompanyId).Count;
            Assert.AreEqual(countBefore + 1, countAfter);
        }

        [TestMethod]
        public void GetCurrentEventsFromRepo_PastEventAdded_CountDoesNotIncrease()
        {
            insertedEvent = MakePastEvent();
            int countBefore = eventsRepo.getCurrentEventsFromRepo(TestDbSeeder.CompanyId).Count;
            eventsRepo.AddEventToRepo(insertedEvent);
            int countAfter = eventsRepo.getCurrentEventsFromRepo(TestDbSeeder.CompanyId).Count;
            Assert.AreEqual(countBefore, countAfter);
        }
        [TestMethod]
        public void GetPastEventsFromRepo_AfterAddingPastEvent_CountIncreasesByOne()
        {
            insertedEvent = MakePastEvent();
            int countBefore = eventsRepo.getPastEventsFromRepo(TestDbSeeder.CompanyId).Count;
            eventsRepo.AddEventToRepo(insertedEvent);
            int countAfter = eventsRepo.getPastEventsFromRepo(TestDbSeeder.CompanyId).Count;
            Assert.AreEqual(countBefore + 1, countAfter);
        }

        [TestMethod]
        public void GetPastEventsFromRepo_CurrentEventAdded_CountDoesNotIncrease()
        {
            int countBefore = eventsRepo.getPastEventsFromRepo(TestDbSeeder.CompanyId).Count;
            eventsRepo.AddEventToRepo(insertedEvent);
            int countAfter = eventsRepo.getPastEventsFromRepo(TestDbSeeder.CompanyId).Count;
            Assert.AreEqual(countBefore, countAfter);
        }

        [TestMethod]
        public void GetMaxEventId_WhenEventsExist_ReturnsPositiveValue()
        {
            eventsRepo.AddEventToRepo(insertedEvent);

            var result = eventsRepo.GetMaxEventId();

            Assert.IsTrue(result > 0);
        }

        [TestMethod]
        public void AddEventToRepo_WithPhotoAndNullDescription_CanBeRetrievedAfterwards()
        {
            var eventWithPhoto = new Event(
                "photo.jpg",
                "Test Event With Photo",
                null,
                new DateTime(2027, 1, 1),
                new DateTime(2027, 1, 2),
                "Cluj-Napoca",
                TestDbSeeder.CompanyId,
                new List<Company>()
            );

            eventsRepo.AddEventToRepo(eventWithPhoto);
            var result = eventsRepo.getCurrentEventsFromRepo(TestDbSeeder.CompanyId);
            var found = result.First(e => e.Id == eventWithPhoto.Id);

            eventsRepo.RemoveEventFromRepo(eventWithPhoto);

            Assert.IsNotNull(found);
        }

        [TestMethod]
        public void UpdateEventToRepo_WithNullDescription_UpdateIsPersistedInDb()
        {
            eventsRepo.AddEventToRepo(insertedEvent);
            eventsRepo.UpdateEventToRepo(insertedEvent.Id, null, "Updated Title", null, new DateTime(2027, 1, 1), new DateTime(2027, 1, 2), "Cluj-Napoca");

            var result = eventsRepo.getCurrentEventsFromRepo(TestDbSeeder.CompanyId);
            var updatedEvent = result.First(e => e.Id == insertedEvent.Id);

            Assert.AreEqual("Updated Title", updatedEvent.Title);
        }

        [TestMethod]
        public void AddEventToRepo_NullTitle_ThrowsException()
        {
            var invalidEvent = new Event(
                null,
                null,
                "description",
                new DateTime(2027, 1, 1),
                new DateTime(2027, 1, 2),
                "Cluj-Napoca",
                TestDbSeeder.CompanyId,
                new List<Company>()
            );

            Assert.ThrowsException<Microsoft.Data.SqlClient.SqlException>(() =>
                eventsRepo.AddEventToRepo(invalidEvent));
        }
    }
}