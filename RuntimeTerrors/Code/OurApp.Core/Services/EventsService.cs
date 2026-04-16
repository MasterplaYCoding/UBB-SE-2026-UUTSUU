using CommunityToolkit.Mvvm.DependencyInjection;
using OurApp.Core.Models;
using OurApp.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OurApp.Core.Services
{
    public class EventsService : IEventsService
    {
        IEventsRepo eventsRepository;
        //Company company;

        /// <summary>
        /// Events service constructor
        /// </summary>
        /// <param name="eventsRepo"> events repository </param>
        public EventsService(IEventsRepo eventsRepo)//, Company Company)
        {
            this.eventsRepository = eventsRepo;
            //this.company = Company;
        }

        /// <summary>
        /// Function that creates a new event
        /// </summary>
        /// <param name="eventPhoto"> the generated image path </param>
        /// <param name="eventTitle"> the event's title </param>
        /// <param name="eventDescription"> the event's description </param>
        /// <param name="eventStartDate"> the event's starting date </param>
        /// <param name="eventEndDate"> the event's ending date </param>
        /// <param name="eventLocation"> the event's location </param>
        /// <param name="collaborators"> a list of all the companies collaborating on the event </param>
        public Event AddEvent(string eventPhoto, string eventTitle, string eventDescription, DateTime eventStartDate, DateTime eventEndDate, string eventLocation, int hostId, List<Company> collaborators)
        {
            Event eventToBeAdded = new Event(eventPhoto, eventTitle, eventDescription, eventStartDate, eventEndDate, eventLocation, hostId, collaborators ?? new List<Company>());
            this.eventsRepository.AddEventToRepo(eventToBeAdded);
            //this.company.CollaboratorsCount += collaborators.Count;
            return eventToBeAdded;
        }

        /// <summary>
        /// Function that updates the information of an event
        /// </summary>
        /// <param name="eventIdToBeUpdated"> the id of the event that's updated </param>
        /// <param name="newEventPhoto"> the updated photo path </param>
        /// <param name="newEventTitle"> the updated title of the event </param>
        /// <param name="newEventDescription"> the updated description of the event </param>
        /// <param name="newEventStartDate"> the updated starting date of the event </param>
        /// <param name="newEventEndDate"> the updated ending date of the event </param>
        /// <param name="newEventLocation"> the updated location of the event </param>
        public void UpdateEvent(int eventIdToBeUpdated, string newEventPhoto, string newEventTitle, string newEventDescription, DateTime newEventStartDate, DateTime newEventEndDate, string newEventLocation)
        {
            this.eventsRepository.UpdateEventToRepo(eventIdToBeUpdated, newEventPhoto, newEventTitle, newEventDescription, newEventStartDate, newEventEndDate, newEventLocation);
        }


        /// <summary>
        /// Function that deletes an event
        /// </summary>
        /// <param name="eventToBeRemoved"> event selected to be removed </param>
        public void DeleteEvent(Event eventToBeRemoved)
        {
            this.eventsRepository.RemoveEventFromRepo(eventToBeRemoved);
        }


        /// <summary>
        /// Function that returns a collection of all the current events
        /// </summary>
        /// <returns> ObservableCollection of the current events </returns>
        public ObservableCollection<Event> GetCurrentEvents(int loggedInUserID)
        {
            return this.eventsRepository.getCurrentEventsFromRepo(loggedInUserID);
        }

        /// <summary>
        /// Function that returns a collection of all the past events
        /// </summary>
        /// <returns> ObservableCollection of the past events </returns>
        public ObservableCollection<Event> GetPastEvents(int loggedInUserID)
        {
            return this.eventsRepository.getPastEventsFromRepo(loggedInUserID);
        }
    }
}
