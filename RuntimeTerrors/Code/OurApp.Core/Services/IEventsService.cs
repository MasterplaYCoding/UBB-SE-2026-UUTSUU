using OurApp.Core.Models;
using System.Collections.ObjectModel;

namespace OurApp.Core.Services
{
    public interface IEventsService
    {
        Event AddEvent(string eventPhoto, string eventTitle, string eventDescription, DateTime eventStartDate, DateTime eventEndDate, string eventLocation, int hostId, List<Company> collaborators);
        void DeleteEvent(Event eventToBeRemoved);
        ObservableCollection<Event> GetCurrentEvents(int loggedInUserID);
        ObservableCollection<Event> GetPastEvents(int loggedInUserID);
        void UpdateEvent(int eventIdToBeUpdated, string newEventPhoto, string newEventTitle, string newEventDescription, DateTime newEventStartDate, DateTime newEventEndDate, string newEventLocation);
    }
}