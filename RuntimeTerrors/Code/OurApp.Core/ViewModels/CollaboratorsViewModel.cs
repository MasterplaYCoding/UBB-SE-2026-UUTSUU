using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using OurApp.Core.Models;
using OurApp.Core.Services;

namespace OurApp.Core.ViewModels
{
    public partial class CollaboratorsViewModel : ObservableObject
    {
        public List<Company> AllCollaborators { get; }
        private readonly ICollaboratorsService collaboratorsService;
        private readonly SessionService sessionService;

        /// <summary>
        /// Collaborators view model constructor that populates the list of all the collaborators
        /// </summary>
        /// <param name="collaboratorsService"></param>
        /// <param name="sessionService"></param>
        public CollaboratorsViewModel(ICollaboratorsService collaboratorsService, SessionService sessionService)
        {
            this.collaboratorsService = collaboratorsService;
            this.sessionService = sessionService;

            this.AllCollaborators = collaboratorsService.GetAllCollaborators(sessionService.LoggedInUser.CompanyId);
        }
    }
}
