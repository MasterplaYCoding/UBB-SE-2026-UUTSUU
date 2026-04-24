using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OurApp.Core.Models;
using OurApp.Core.Services;
using OurApp.Tests.Helpers;

namespace OurApp.Tests.Services
{
    [TestClass]
    public class EventsServiceTests
    {
        private const string DefaultPhoto = "photo.jpg";
        private const string UpdatedPhoto = "new_photo.jpg";

        private const string DefaultTitle = "Test Event";
        private const string TitleHackathon = "Hackathon";
        private const string TitleGeneric = "Event";
        private const string TitleSpecific = "Event Title";
        private const string TitleUpdated = "Updated Title";
        private const string TitleShort = "Title";

        private const string DefaultDesc = "Test Description";
        private const string DescLower = "description";
        private const string DescSpecific = "Description for event";
        private const string DescShort = "desc";
        private const string DescEvent = "Event Description";
        private const string DescUpdated = "Updated description";

        private const string LocClujNapoca = "Cluj-Napoca";
        private const string LocCluj = "Cluj";
        private const string LocTimisoara = "Timisoara";

        private const string CollabName = "Collaborator";

        private const int Year = 2026;
        private const int MonthJune = 6;
        private const int MonthJuly = 7;

        private const int Day1 = 1;
        private const int Day2 = 2;
        private const int Day3 = 3;
        private const int Day10 = 10;
        private const int Day15 = 15;

        private const int DefaultId = 1;
        private const int ExpectedHostId = 42;
        private const int AltEventId = 5;
        private const int CollabId = 10;

        private const int CountZero = 0;
        private const int CountOne = 1;
        private const int CountTwo = 2;

        private FakeEventsRepo fakeEventsRepo = null!;
        private EventsService eventsService = null!;

        private static Event MakeEvent()
        {
            return new Event(
                DefaultPhoto,
                DefaultTitle,
                DefaultDesc,
                new DateTime(Year, MonthJune, Day1),
                new DateTime(Year, MonthJune, Day2),
                LocClujNapoca,
                DefaultId,
                new List<Company>());
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
            eventsService.AddEvent(DefaultPhoto, TitleShort, DescLower,
                new DateTime(Year, MonthJune, Day1), new DateTime(Year, MonthJune, Day2), LocCluj, DefaultId, new List<Company>());
            Assert.IsNotNull(fakeEventsRepo.AddedEvent);
        }

        [TestMethod]
        public void AddEvent_ValidData_EventPassedToRepoHasCorrectTitle()
        {
            eventsService.AddEvent(DefaultPhoto, TitleHackathon, DescLower,
                new DateTime(Year, MonthJune, Day1), new DateTime(Year, MonthJune, Day2), LocCluj, DefaultId, new List<Company>());
            Assert.AreEqual(TitleHackathon, fakeEventsRepo.AddedEvent?.Title);
        }

        [TestMethod]
        public void AddEvent_ValidData_EventPassedToRepoHasCorrectDescription()
        {
            eventsService.AddEvent(DefaultPhoto, TitleGeneric, DescSpecific,
                new DateTime(Year, MonthJune, Day1), new DateTime(Year, MonthJune, Day2), LocCluj, DefaultId, new List<Company>());
            Assert.AreEqual(DescSpecific, fakeEventsRepo.AddedEvent?.Description);
        }

        [TestMethod]
        public void AddEvent_ValidData_EventPassedToRepoHasCorrectStartDate()
        {
            DateTime expectedStartDate = new DateTime(Year, MonthJune, Day1);
            eventsService.AddEvent(DefaultPhoto, TitleGeneric, DescSpecific,
                expectedStartDate, new DateTime(Year, MonthJune, Day2), LocCluj, DefaultId, new List<Company>());
            Assert.AreEqual(expectedStartDate, fakeEventsRepo.AddedEvent?.StartDate);
        }

        [TestMethod]
        public void AddEvent_ValidData_EventPassedToRepoHasCorrectEndDate()
        {
            DateTime expectedEndDate = new DateTime(Year, MonthJune, Day3);
            eventsService.AddEvent(DefaultPhoto, TitleGeneric, DescSpecific,
                new DateTime(Year, MonthJune, Day1), expectedEndDate, LocCluj, DefaultId, new List<Company>());
            Assert.AreEqual(expectedEndDate, fakeEventsRepo.AddedEvent?.EndDate);
        }

        [TestMethod]
        public void AddEvent_ValidData_EventPassedToRepoHasCorrectLocation()
        {
            eventsService.AddEvent(DefaultPhoto, TitleGeneric, DescSpecific,
                new DateTime(Year, MonthJune, Day1), new DateTime(Year, MonthJune, Day2), LocCluj, DefaultId, new List<Company>());
            Assert.AreEqual(LocCluj, fakeEventsRepo.AddedEvent?.Location);
        }

        [TestMethod]
        public void AddEvent_ValidData_EventPassedToRepoHasCorrectHostId()
        {
            eventsService.AddEvent(DefaultPhoto, TitleGeneric, DescSpecific,
                new DateTime(Year, MonthJune, Day1), new DateTime(Year, MonthJune, Day2), LocCluj, ExpectedHostId, new List<Company>());
            Assert.AreEqual(ExpectedHostId, fakeEventsRepo.AddedEvent?.HostID);
        }

        [TestMethod]
        public void AddEvent_ValidData_ReturnsTheCreatedEvent()
        {
            Event result = eventsService.AddEvent(DefaultPhoto, TitleShort, DescLower,
                new DateTime(Year, MonthJune, Day1), new DateTime(Year, MonthJune, Day2), LocCluj, DefaultId, new List<Company>());
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void AddEvent_ValidData_ReturnedEventHasCorrectTitle()
        {
            Event result = eventsService.AddEvent(DefaultPhoto, TitleSpecific, DescShort,
                new DateTime(Year, MonthJune, Day1), new DateTime(Year, MonthJune, Day2), LocCluj, DefaultId, new List<Company>());
            Assert.AreEqual(TitleSpecific, result.Title);
        }

        [TestMethod]
        public void AddEvent_ValidData_ReturnedEventHasCorrectDescription()
        {
            Event result = eventsService.AddEvent(DefaultPhoto, TitleGeneric, DescEvent,
                new DateTime(Year, MonthJune, Day1), new DateTime(Year, MonthJune, Day2), LocCluj, DefaultId, new List<Company>());
            Assert.AreEqual(DescEvent, result.Description);
        }

        [TestMethod]
        public void AddEvent_ValidData_ReturnedEventHasCorrectStartDate()
        {
            DateTime expectedStartDate = new DateTime(Year, MonthJune, Day1);
            Event result = eventsService.AddEvent(DefaultPhoto, TitleGeneric, DescSpecific,
                expectedStartDate, new DateTime(Year, MonthJune, Day2), LocCluj, DefaultId, new List<Company>());
            Assert.AreEqual(expectedStartDate, result.StartDate);
        }

        [TestMethod]
        public void AddEvent_ValidData_ReturnedEventHasCorrectEndDate()
        {
            DateTime expectedEndDate = new DateTime(Year, MonthJune, Day3);
            Event result = eventsService.AddEvent(DefaultPhoto, TitleGeneric, DescSpecific,
                new DateTime(Year, MonthJune, Day1), expectedEndDate, LocCluj, DefaultId, new List<Company>());
            Assert.AreEqual(expectedEndDate, result.EndDate);
        }

        [TestMethod]
        public void AddEvent_ValidData_ReturnedEventHasCorrectLocation()
        {
            Event result = eventsService.AddEvent(DefaultPhoto, TitleGeneric, DescSpecific,
                new DateTime(Year, MonthJune, Day1), new DateTime(Year, MonthJune, Day2), LocCluj, DefaultId, new List<Company>());
            Assert.AreEqual(LocCluj, result.Location);
        }

        [TestMethod]
        public void AddEvent_ValidData_ReturnedEventHasCorrectHostId()
        {
            Event result = eventsService.AddEvent(DefaultPhoto, TitleGeneric, DescSpecific,
                new DateTime(Year, MonthJune, Day1), new DateTime(Year, MonthJune, Day2), LocCluj, ExpectedHostId, new List<Company>());
            Assert.AreEqual(ExpectedHostId, result.HostID);
        }

        [TestMethod]
        public void AddEvent_WithOneCollaborator_EventPassedToRepoContainsCollaborator()
        {
            var collaborator = new Company(CollabName, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, CollabId);
            var collaborators = new List<Company> { collaborator };
            eventsService.AddEvent(DefaultPhoto, TitleGeneric, DescShort,
                new DateTime(Year, MonthJune, Day1), new DateTime(Year, MonthJune, Day2), LocCluj, DefaultId, collaborators);
            Assert.AreEqual(CountOne, fakeEventsRepo.AddedEvent?.Collaborators.Count);
        }

        [TestMethod]
        public void AddEvent_NullCollaboratorsList_EventPassedToRepoHasNoCollaborators()
        {
            eventsService.AddEvent(DefaultPhoto, TitleGeneric, DescShort,
                new DateTime(Year, MonthJune, Day1), new DateTime(Year, MonthJune, Day2), LocCluj, DefaultId, null!);
            Assert.AreEqual(CountZero, fakeEventsRepo.AddedEvent?.Collaborators.Count);
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
            eventsService.UpdateEvent(AltEventId, DefaultPhoto, TitleShort, DescShort,
                new DateTime(Year, MonthJune, Day1), new DateTime(Year, MonthJune, Day2), LocCluj);
            Assert.AreEqual(AltEventId, fakeEventsRepo.UpdatedEventId);
        }

        [TestMethod]
        public void UpdateEvent_ValidData_RepoReceivesCorrectPhoto()
        {
            eventsService.UpdateEvent(DefaultId, UpdatedPhoto, TitleShort, DescShort,
                new DateTime(Year, MonthJune, Day1), new DateTime(Year, MonthJune, Day2), LocCluj);
            Assert.AreEqual(UpdatedPhoto, fakeEventsRepo.UpdatedPhoto);
        }

        [TestMethod]
        public void UpdateEvent_ValidData_RepoReceivesCorrectTitle()
        {
            eventsService.UpdateEvent(DefaultId, DefaultPhoto, TitleUpdated, DescShort,
                new DateTime(Year, MonthJune, Day1), new DateTime(Year, MonthJune, Day2), LocCluj);
            Assert.AreEqual(TitleUpdated, fakeEventsRepo.UpdatedTitle);
        }

        [TestMethod]
        public void UpdateEvent_ValidData_RepoReceivesCorrectDescription()
        {
            eventsService.UpdateEvent(DefaultId, DefaultPhoto, TitleShort, DescUpdated,
                new DateTime(Year, MonthJune, Day1), new DateTime(Year, MonthJune, Day2), LocCluj);
            Assert.AreEqual(DescUpdated, fakeEventsRepo.UpdatedDescription);
        }

        [TestMethod]
        public void UpdateEvent_ValidData_RepoReceivesCorrectStartDate()
        {
            DateTime expectedStartDate = new DateTime(Year, MonthJuly, Day10);
            eventsService.UpdateEvent(DefaultId, DefaultPhoto, TitleShort, DescShort,
                expectedStartDate, new DateTime(Year, MonthJuly, Day15), LocCluj);
            Assert.AreEqual(expectedStartDate, fakeEventsRepo.UpdatedStartDate);
        }

        [TestMethod]
        public void UpdateEvent_ValidData_RepoReceivesCorrectEndDate()
        {
            DateTime expectedEndDate = new DateTime(Year, MonthJuly, Day15);
            eventsService.UpdateEvent(DefaultId, DefaultPhoto, TitleShort, DescShort,
                new DateTime(Year, MonthJuly, Day10), expectedEndDate, LocCluj);
            Assert.AreEqual(expectedEndDate, fakeEventsRepo.UpdatedEndDate);
        }

        [TestMethod]
        public void UpdateEvent_ValidData_RepoReceivesCorrectLocation()
        {
            eventsService.UpdateEvent(DefaultId, DefaultPhoto, TitleShort, DescShort,
                new DateTime(Year, MonthJune, Day1), new DateTime(Year, MonthJune, Day2), LocTimisoara);
            Assert.AreEqual(LocTimisoara, fakeEventsRepo.UpdatedLocation);
        }

        [TestMethod]
        public void GetCurrentEvents_RepoReturnsTwoEvents_ServiceReturnsTwoEvents()
        {
            fakeEventsRepo.CurrentEventsToReturn = new ObservableCollection<Event>
            {
                MakeEvent(),
                MakeEvent()
            };
            var result = eventsService.GetCurrentEvents(DefaultId);
            Assert.AreEqual(CountTwo, result.Count);
        }

        [TestMethod]
        public void GetCurrentEvents_RepoReturnsEmptyCollection_ServiceReturnsEmptyCollection()
        {
            fakeEventsRepo.CurrentEventsToReturn = new ObservableCollection<Event>();
            var result = eventsService.GetCurrentEvents(DefaultId);
            Assert.AreEqual(CountZero, result.Count);
        }

        [TestMethod]
        public void GetPastEvents_RepoReturnsTwoEvents_ServiceReturnsTwoEvents()
        {
            fakeEventsRepo.PastEventsToReturn = new ObservableCollection<Event>
            {
                MakeEvent(),
                MakeEvent()
            };
            var result = eventsService.GetPastEvents(DefaultId);
            Assert.AreEqual(CountTwo, result.Count);
        }

        [TestMethod]
        public void GetPastEvents_RepoReturnsEmptyCollection_ServiceReturnsEmptyCollection()
        {
            fakeEventsRepo.PastEventsToReturn = new ObservableCollection<Event>();
            var result = eventsService.GetPastEvents(DefaultId);
            Assert.AreEqual(CountZero, result.Count);
        }
    }
}