using OurApp.Core.Models;
using OurApp.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OurApp.Tests.Helpers
{
    internal class FakeCollaboratorsRepo : ICollaboratorsRepo
    {
        public Event ReceivedEvent = null;
        public Company ReceivedCompany = null;
        public int ReceivedLoggedInUserId = -1;
        public List<Company> CollaboratorsToReturn = new List<Company>();

        public void AddCollaboratorToRepo(Event eventOfCollaboration, Company collaboratorToBeAdded, int loggedInUserID)
        {
            ReceivedEvent = eventOfCollaboration;
            ReceivedCompany = collaboratorToBeAdded;
            ReceivedLoggedInUserId = loggedInUserID;
        }

        public List<Company> GetAllCollaborators(int loggedInCompanyId)
        {
            return CollaboratorsToReturn;
        }
    }
}
