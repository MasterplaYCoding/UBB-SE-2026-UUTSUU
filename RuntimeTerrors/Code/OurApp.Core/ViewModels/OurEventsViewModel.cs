using CommunityToolkit.Mvvm.ComponentModel;
using OurApp.Core.Models;
using OurApp.Core.Services;
using System.Collections.ObjectModel;

namespace OurApp.Core.ViewModels
{
    public partial class OurEventsViewModel : ObservableObject
    {
        private readonly IEventsService _eventsService;
        private readonly SessionService _sessionService;

        public ObservableCollection<Event> currentEventsCollection { get; }

        /// <summary>
        /// Our Events View Model constructor
        /// </summary>
        /// <param name="eventsService"> events service </param>
        /// <param name="sessionService"> session service - the logged in user </param>
        public OurEventsViewModel(IEventsService eventsService, SessionService sessionService)
        {
            _eventsService = eventsService;
            _sessionService = sessionService;

            currentEventsCollection = _eventsService.GetCurrentEvents(_sessionService.loggedInUser.CompanyId);
        }
    }
}