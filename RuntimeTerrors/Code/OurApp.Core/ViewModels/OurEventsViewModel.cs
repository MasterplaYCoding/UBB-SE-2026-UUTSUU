using CommunityToolkit.Mvvm.ComponentModel;
using OurApp.Core.Models;
using OurApp.Core.Repositories;
using OurApp.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OurApp.Core.ViewModels
{
    public partial class OurEventsViewModel : ObservableObject
    {
        IEventsService eventsService;
        SessionService sessionService;
        public ObservableCollection<Event> currentEventsCollection { get; }

        /// <summary>
        /// Our Events View Model constructor
        /// </summary>
        /// <param name="eventsService"> events service </param>
        /// <param name="sessionService"> session service - the logged in user </param>
        public OurEventsViewModel(IEventsService eventsService, SessionService sessionService)
        {
            this.eventsService = eventsService;
            this.sessionService = sessionService;

            currentEventsCollection = eventsService.GetCurrentEvents(sessionService.loggedInUser.CompanyId);
        }
    }
}
