using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OurApp.Core.Models;

namespace OurApp.Core.Repositories
{
    public interface ICollaboratorsRepo
    {
        void AddCollaboratorToRepo(Event eventOfCollaboration, Company collaboratorToBeAdded, int loggedInUserID);
        List<Company> GetAllCollaborators(int loggedInCompanyId);
    }
}
