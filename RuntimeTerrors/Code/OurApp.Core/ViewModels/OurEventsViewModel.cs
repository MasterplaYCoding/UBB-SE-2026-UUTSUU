using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using OurApp.Core.Models;
using OurApp.Core.Services;

namespace OurApp.Core.ViewModels
{
    public partial class OurEventsViewModel : ObservableObject
    {
        private readonly IEventsService eventsService;
        private readonly SessionService sessionService;

        public ObservableCollection<Event> CurrentEventsCollection { get; }

        /// <summary>
        /// Our Events View Model constructor
        /// </summary>
        /// <param name="eventsService"> events service </param>
        /// <param name="sessionService"> session service - the logged in user </param>
        public OurEventsViewModel(IEventsService eventsService, SessionService sessionService)
        {
            this.eventsService = eventsService;
            this.sessionService = sessionService;

            CurrentEventsCollection = this.eventsService.GetCurrentEvents(this.sessionService.LoggedInUser.CompanyId);
        }
    }
}