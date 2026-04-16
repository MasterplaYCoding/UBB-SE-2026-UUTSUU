using OurApp.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OurApp.Core.Repositories
{
    public interface IEventsRepo
    {
        void AddEventToRepo(Event e);
        void RemoveEventFromRepo(Event e);
        ObservableCollection<Event> getCurrentEventsFromRepo(int loggedInUser);
        ObservableCollection<Event> getPastEventsFromRepo(int loggedInUser);
        void UpdateEventToRepo(int id, string photo, string title, string description, DateTime start, DateTime end, string location);

    }
}
