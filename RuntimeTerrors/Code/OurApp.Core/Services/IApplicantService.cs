using System.Collections.Generic;
using OurApp.Core.Models;

namespace OurApp.Core.Services
{
    public interface IApplicantService
    {
        IEnumerable<Applicant> GetApplicantsForJob(JobPosting job);
        Applicant GetApplicant(int applicantId);
        
        void ProcessCv(int applicantId);
        void UpdateAppTestGrade(int applicantId, decimal grade);
        void UpdateCompanyTestGrade(int applicantId, decimal grade);
        void UpdateInterviewGrade(int applicantId, decimal grade);
        void UpdateApplicant(Applicant applicant);
        void RemoveApplicant(int applicantId);
        
        decimal? ScanCvXml(Applicant applicant);
    }
}
