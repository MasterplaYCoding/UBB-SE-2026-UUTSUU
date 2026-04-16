using OurApp.Core.Models;

namespace OurApp.Core.Services
{
    public interface ICollaboratorsService
    {
        void AddCollaborator(Event eventToBeCollaboratedOn, Company companyInvitedToCollaborate, int loggedInUserID);
        List<Company> GetAllCollaborators(int loggedInCompanyId);
    }
}