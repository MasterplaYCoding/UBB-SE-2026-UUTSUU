using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OurApp.Core.Models;
using OurApp.Core.Repositories;
using OurApp.Core.Validators;

namespace OurApp.Core.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepo companyRepo;
        private readonly ICompanyValidator companyValidator;

        public CompanyService(ICompanyRepo repo)
        {
            companyRepo = repo;
            companyValidator = new CompanyValidator();
        }

        private void ValidateCompany(Company company)
        {
            companyValidator.ValidateName(company.Name);
            companyValidator.ValidateAboutUs(company.AboutUs);
            companyValidator.ValidateProfilePicture(company.ProfilePicturePath);
            companyValidator.ValidateLogo(company.CompanyLogoPath);
            companyValidator.ValidateLocation(company.Location);
            companyValidator.ValidateEmail(company.Email);
        }

        public void AddCompany(string companyName, string aboutUs, string pfpUrl, string logoUrl, string location, string email)
        {
            Company companyToBeAdded = new Company(companyName, aboutUs, pfpUrl, logoUrl, location, email);
            ValidateCompany(companyToBeAdded);
            companyRepo.Add(companyToBeAdded);
        }

        public Company? GetCompanyById(int companyId)
        {
            return companyRepo.GetById(companyId);
        }

        public void UpdateCompany(Company company)
        {
            ValidateCompany(company);
            companyRepo.Update(company);
        }

        public void RemoveCompany(int companyId)
        {
            companyRepo.Remove(companyId);
        }

        public void PrintAll()
        {
            this.companyRepo.PrintAll();
        }

        /// <summary>
        /// Function that searches a company by name and returns it
        /// </summary>
        /// <param name="companyName"> the name of the company being searched </param>
        /// <returns> the company if found, else null </returns>
        public Company? GetCompanyByName(string companyName)
        {
            return this.companyRepo.GetCompanyByName(companyName);
        }
    }
}
