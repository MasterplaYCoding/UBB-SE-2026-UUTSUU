using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using OurApp.Core.Models;
using OurApp.Core.Services;

namespace OurApp.Core.ViewModels
{
    public partial class PastEventsViewModel : ObservableObject
    {
        private readonly IEventsService eventsService;
        private readonly SessionService sessionService;

        public ObservableCollection<Event> PastEventsCollection { get; }

        /// <summary>
        /// Past Events View Model constructor
        /// </summary>
        /// <param name="eventsService"> events service </param>
        /// <param name="sessionService"> session service - the logged in user </param>
        public PastEventsViewModel(IEventsService eventsService, SessionService sessionService)
        {
            this.eventsService = eventsService;
            this.sessionService = sessionService;

            PastEventsCollection = this.eventsService.GetPastEvents(this.sessionService.LoggedInUser.CompanyId);
        }
    }
}