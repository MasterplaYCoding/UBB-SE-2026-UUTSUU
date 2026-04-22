using OurApp.Core.Models;
using OurApp.Core.Services;
using OurApp.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OurApp.Tests.Services
{
    [TestClass]
    public class EventsServiceTests
    {
        private FakeEventsRepo fakeEventsRepo;
        private EventsService eventsService;

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
            );
        }

        [TestInitialize]
        public void Setup()
        {
            fakeEventsRepo = new FakeEventsRepo();
            eventsService = new EventsService(fakeEventsRepo);
        }

        [TestMethod]
        public void AddEvent_ValidData_AddEventToRepo()
        {
            eventsService.AddEvent("photo.jpg", "Title", "description",
                new DateTime(2026, 6, 1), new DateTime(2026, 6, 2), "Cluj", 1, new List<Company>());
            Assert.IsNotNull(fakeEventsRepo.AddedEvent);
        }


        [TestMethod]
        public void AddEvent_ValidData_EventPassedToRepoHasCorrectTitle()
        {
            string expectedTitle = "Hackathon";
            eventsService.AddEvent("photo.jpg", expectedTitle, "description",
                new DateTime(2026, 6, 1), new DateTime(2026, 6, 2), "Cluj", 1, new List<Company>());
            Assert.AreEqual(expectedTitle, fakeEventsRepo.AddedEvent.Title);
        }

        [TestMethod]
        public void AddEvent_ValidData_EventPassedToRepoHasCorrectDescription()
        {
            string expectedDescription = "Description for event";
            eventsService.AddEvent("photo.jpg", "Event", expectedDescription,
                new DateTime(2026, 6, 1), new DateTime(2026, 6, 2), "Cluj", 1, new List<Company>());
            Assert.AreEqual(expectedDescription, fakeEventsRepo.AddedEvent.Description);
        }

        [TestMethod]
        public void AddEvent_ValidData_EventPassedToRepoHasCorrectStartDate()
        {
            DateTime expectedStartDate = new DateTime(2026, 6, 1);
            eventsService.AddEvent("photo.jpg", "Event", "Description for event",
                expectedStartDate, new DateTime(2026, 6, 2), "Cluj", 1, new List<Company>());
            Assert.AreEqual(expectedStartDate, fakeEventsRepo.AddedEvent.StartDate);
        }

        [TestMethod]
        public void AddEvent_ValidData_EventPassedToRepoHasCorrectEndDate()
        {
            DateTime expectedEndDate = new DateTime(2026, 6, 3);
            eventsService.AddEvent("photo.jpg", "Event", "Description for event",
                new DateTime(2026, 6, 1), expectedEndDate, "Cluj", 1, new List<Company>());
            Assert.AreEqual(expectedEndDate, fakeEventsRepo.AddedEvent.EndDate);
        }

        [TestMethod]
        public void AddEvent_ValidData_EventPassedToRepoHasCorrectLocation()
        {
            string expectedLocation = "Cluj";
            eventsService.AddEvent("photo.jpg", "Event", "Description for event",
                new DateTime(2026, 6, 1), new DateTime(2026, 6, 2), expectedLocation, 1, new List<Company>());
            Assert.AreEqual(expectedLocation, fakeEventsRepo.AddedEvent.Location);
        }
        [TestMethod]
        public void AddEvent_ValidData_EventPassedToRepoHasCorrectHostId()
        {
            int expectedHostId = 42;
            eventsService.AddEvent("photo.jpg", "Event", "Description for event",
                new DateTime(2026, 6, 1), new DateTime(2026, 6, 2), "Cluj", expectedHostId, new List<Company>());
            Assert.AreEqual(expectedHostId, fakeEventsRepo.AddedEvent.HostID);
        }

        [TestMethod]
        public void AddEvent_ValidData_ReturnsTheCreatedEvent()
        {
            Event result = eventsService.AddEvent("photo.jpg", "Title", "description",
                new DateTime(2026, 6, 1), new DateTime(2026, 6, 2), "Cluj", 1, new List<Company>());
            Assert.IsNotNull(result);
        }
        [TestMethod]
        public void AddEvent_ValidData_ReturnedEventHasCorrectTitle()
        {
            string expectedTitle = "Event Title";
            Event result = eventsService.AddEvent("photo.jpg", expectedTitle, "desc",
                new DateTime(2026, 6, 1), new DateTime(2026, 6, 2), "Cluj", 1, new List<Company>());
            Assert.AreEqual(expectedTitle, result.Title);
        }
        [TestMethod]
        public void AddEvent_ValidData_ReturnedEventHasCorrectDescription()
        {
            string expectedDescription = "Event Description";
            Event result = eventsService.AddEvent("photo.jpg", "Event", expectedDescription,
                new DateTime(2026, 6, 1), new DateTime(2026, 6, 2), "Cluj", 1, new List<Company>());
            Assert.AreEqual(expectedDescription, result.Description);
        }
        [TestMethod]
        public void AddEvent_ValidData_ReturnedEventHasCorrectStartDate()
        {
            DateTime expectedStartDate = new DateTime(2026, 6, 1);
            Event result = eventsService.AddEvent("photo.jpg", "Event", "Description for event",
                expectedStartDate, new DateTime(2026, 6, 2), "Cluj", 1, new List<Company>());
            Assert.AreEqual(expectedStartDate, result.StartDate);
        }

        [TestMethod]
        public void AddEvent_ValidData_ReturnedEventHasCorrectEndDate()
        {
            DateTime expectedEndDate = new DateTime(2026, 6, 3);
            Event result = eventsService.AddEvent("photo.jpg", "Event", "Description for event",
                new DateTime(2026, 6, 1), expectedEndDate, "Cluj", 1, new List<Company>());
            Assert.AreEqual(expectedEndDate, result.EndDate);
        }
        [TestMethod]
        public void AddEvent_ValidData_ReturnedEventHasCorrectLocation()
        {
            string expectedLocation = "Cluj";
            Event result = eventsService.AddEvent("photo.jpg", "Event", "Description for event",
                new DateTime(2026, 6, 1), new DateTime(2026, 6, 2), expectedLocation, 1, new List<Company>());
            Assert.AreEqual(expectedLocation, result.Location);
        }

        [TestMethod]
        public void AddEvent_ValidData_ReturnedEventHasCorrectHostId()
        {
            int expectedHostId = 42;
            Event result = eventsService.AddEvent("photo.jpg", "Event", "Description for event",
                new DateTime(2026, 6, 1), new DateTime(2026, 6, 2), "Cluj", expectedHostId, new List<Company>());
            Assert.AreEqual(expectedHostId, result.HostID);
        }
        [TestMethod]
        public void AddEvent_WithOneCollaborator_EventPassedToRepoContainsCollaborator()
        {
            var collaborator = new Company("Collaborator", "", "", "", "", "", 10);
            var collaborators = new List<Company> { collaborator };
            eventsService.AddEvent("photo.jpg", "Event", "desc",
                new DateTime(2026, 6, 1), new DateTime(2026, 6, 2), "Cluj", 1, collaborators);
            Assert.AreEqual(1, fakeEventsRepo.AddedEvent.Collaborators.Count);
        }

        [TestMethod]
        public void AddEvent_NullCollaboratorsList_EventPassedToRepoHasNoCollaborators()
        {
            eventsService.AddEvent("photo.jpg", "Event", "desc",
                new DateTime(2026, 6, 1), new DateTime(2026, 6, 2), "Cluj", 1, null);
            Assert.AreEqual(0, fakeEventsRepo.AddedEvent.Collaborators.Count);
        }

        [TestMethod]
        public void DeleteEvent_ValidEvent_CorrectEventPassedToRepoForDeletion()
        {
            Event eventToDelete = MakeEvent();
            eventsService.DeleteEvent(eventToDelete);
            Assert.AreEqual(eventToDelete, fakeEventsRepo.RemovedEvent);
        }
        [TestMethod]
        public void DeleteEvent_ValidEvent_CallsRemoveEventFromRepo()
        { 
            Event eventToDelete = MakeEvent();
            eventsService.DeleteEvent(eventToDelete);
            Assert.IsNotNull(fakeEventsRepo.RemovedEvent);
        }
        [TestMethod]
        public void UpdateEvent_ValidData_RepoReceivesCorrectEventId()
        { 
            int expectedId = 5;
            eventsService.UpdateEvent(expectedId, "photo.jpg", "Title", "desc",
                new DateTime(2026, 6, 1), new DateTime(2026, 6, 2), "Cluj");
            Assert.AreEqual(expectedId, fakeEventsRepo.UpdatedEventId);
        }

        [TestMethod]
        public void UpdateEvent_ValidData_RepoReceivesCorrectPhoto()
        { 
            string expectedPhoto = "new_photo.jpg";
            eventsService.UpdateEvent(1, expectedPhoto, "Title", "desc",
                new DateTime(2026, 6, 1), new DateTime(2026, 6, 2), "Cluj");
            Assert.AreEqual(expectedPhoto, fakeEventsRepo.UpdatedPhoto);
        }

        [TestMethod]
        public void UpdateEvent_ValidData_RepoReceivesCorrectTitle()
        {
            string expectedTitle = "Updated Title";
            eventsService.UpdateEvent(1, "photo.jpg", expectedTitle, "desc",
                new DateTime(2026, 6, 1), new DateTime(2026, 6, 2), "Cluj");
            Assert.AreEqual(expectedTitle, fakeEventsRepo.UpdatedTitle);
        }

        [TestMethod]
        public void UpdateEvent_ValidData_RepoReceivesCorrectDescription()
        {
            string expectedDescription = "Updated description";
            eventsService.UpdateEvent(1, "photo.jpg", "Title", expectedDescription,
                new DateTime(2026, 6, 1), new DateTime(2026, 6, 2), "Cluj");
            Assert.AreEqual(expectedDescription, fakeEventsRepo.UpdatedDescription);
        }

        [TestMethod]
        public void UpdateEvent_ValidData_RepoReceivesCorrectStartDate()
        {
            DateTime expectedStartDate = new DateTime(2026, 7, 10);
            eventsService.UpdateEvent(1, "photo.jpg", "Title", "desc",
                expectedStartDate, new DateTime(2026, 7, 15), "Cluj");
            Assert.AreEqual(expectedStartDate, fakeEventsRepo.UpdatedStartDate);
        }

        [TestMethod]
        public void UpdateEvent_ValidData_RepoReceivesCorrectEndDate()
        {        
            DateTime expectedEndDate = new DateTime(2026, 7, 15);
            eventsService.UpdateEvent(1, "photo.jpg", "Title", "desc",
                new DateTime(2026, 7, 10), expectedEndDate, "Cluj");
            Assert.AreEqual(expectedEndDate, fakeEventsRepo.UpdatedEndDate);
        }

        [TestMethod]
        public void UpdateEvent_ValidData_RepoReceivesCorrectLocation()
        {
            string expectedLocation = "Timisoara";
            eventsService.UpdateEvent(1, "photo.jpg", "Title", "desc",
                new DateTime(2026, 6, 1), new DateTime(2026, 6, 2), expectedLocation);
            Assert.AreEqual(expectedLocation, fakeEventsRepo.UpdatedLocation);
        }
        [TestMethod]
        public void GetCurrentEvents_RepoReturnsTwoEvents_ServiceReturnsTwoEvents()
        {
            fakeEventsRepo.CurrentEventsToReturn = new ObservableCollection<Event>
            {
                MakeEvent(),
                MakeEvent()
            };
            var result = eventsService.GetCurrentEvents(1);
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public void GetCurrentEvents_RepoReturnsEmptyCollection_ServiceReturnsEmptyCollection()
        {
            fakeEventsRepo.CurrentEventsToReturn = new ObservableCollection<Event>();
            var result = eventsService.GetCurrentEvents(1);
            Assert.AreEqual(0, result.Count);
        }
        [TestMethod]
        public void GetPastEvents_RepoReturnsTwoEvents_ServiceReturnsTwoEvents()
        {
            fakeEventsRepo.PastEventsToReturn = new ObservableCollection<Event>
            {
                MakeEvent(),
                MakeEvent()
            };
            var result = eventsService.GetPastEvents(1);
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public void GetPastEvents_RepoReturnsEmptyCollection_ServiceReturnsEmptyCollection()
        {
            fakeEventsRepo.PastEventsToReturn = new ObservableCollection<Event>();
            var result = eventsService.GetPastEvents(1);
            Assert.AreEqual(0, result.Count);
        }
    }
}
  
