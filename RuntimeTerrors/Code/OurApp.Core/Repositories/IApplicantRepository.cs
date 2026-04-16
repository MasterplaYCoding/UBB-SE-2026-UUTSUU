using System.Collections.Generic;
using OurApp.Core.Models;

namespace OurApp.Core.Repositories
{
    public interface IApplicantRepository
    {
        Applicant GetApplicantById(int applicantId);
        public IEnumerable<Applicant> GetApplicantsByCompany(int companyId);
        IEnumerable<Applicant> GetApplicantsByJob(JobPosting job);
        void AddApplicant(Applicant applicant);
        void UpdateApplicant(Applicant applicant);
        void RemoveApplicant(int applicantId);
    }
}
