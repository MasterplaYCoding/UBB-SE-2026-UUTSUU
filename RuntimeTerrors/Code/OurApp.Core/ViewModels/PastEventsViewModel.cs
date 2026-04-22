using CommunityToolkit.Mvvm.ComponentModel;
using OurApp.Core.Models;
using OurApp.Core.Services;
using System.Collections.ObjectModel;

namespace OurApp.Core.ViewModels
{
    public partial class PastEventsViewModel : ObservableObject
    {
        private readonly IEventsService _eventsService;
        private readonly SessionService _sessionService;

        public ObservableCollection<Event> pastEventsCollection { get; }

        /// <summary>
        /// Past Events View Model constructor
        /// </summary>
        /// <param name="eventsService"> events service </param>
        /// <param name="sessionService"> session service - the logged in user </param>
        public PastEventsViewModel(IEventsService eventsService, SessionService sessionService)
        {
            _eventsService = eventsService;
            _sessionService = sessionService;

            pastEventsCollection = _eventsService.GetPastEvents(_sessionService.loggedInUser.CompanyId);
        }
    }
}