using OurApp.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OurApp.Core.Models;

namespace OurApp.Core.Services
{
    public class CollaboratorsService : ICollaboratorsService
    {
        ICollaboratorsRepo collaboratorsRepository;

        /// <summary>
        /// Collaborators service constructor
        /// </summary>
        /// <param name="collaboratorsRepo"></param>
        public CollaboratorsService(ICollaboratorsRepo collaboratorsRepo)
        {
            this.collaboratorsRepository = collaboratorsRepo;
        }

        /// <summary>
        /// Function that adds a collaborator to a company's collaborator list
        /// </summary>
        /// <param name="eventToBeCollaboratedOn"> the event the company is invited to collaborate on </param>
        /// <param name="companyInvitedToCollaborate"> the company to be added to the list </param>
        /// <param name="loggedInUserID"></param>
        public void AddCollaborator(Event eventToBeCollaboratedOn, Company companyInvitedToCollaborate, int loggedInUserID)
        {
            this.collaboratorsRepository.AddCollaboratorToRepo(eventToBeCollaboratedOn, companyInvitedToCollaborate, loggedInUserID);
        }


        /// <summary>
        /// Function that returns a list of all the collaborators of the user company
        /// </summary>
        /// <param name="loggedInCompanyId"> the ID of the user company that is currently logged in </param>
        /// <returns> a list of all its collaborators </returns>
        public List<Company> GetAllCollaborators(int loggedInCompanyId)
        {
            return this.collaboratorsRepository.GetAllCollaborators(loggedInCompanyId);
        }
    }
}
