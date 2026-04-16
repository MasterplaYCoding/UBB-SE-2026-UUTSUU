using OurApp.Core.Models;
using OurApp.Core.Repositories;
using OurApp.Core.Validators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OurApp.Core.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepo CompanyRepo;
        private readonly CompanyValidator _companyValidator;
        private readonly GameValidator _gameValidator;

        public CompanyService(ICompanyRepo repo) 
        {
            CompanyRepo = repo;
            _companyValidator = new CompanyValidator();
            _gameValidator = new GameValidator();
        }

        private void ValidateCompany(Company company)
        {
            _companyValidator.NameValidator(company.Name);
            _companyValidator.AboutUsValidator(company.AboutUs);
            _companyValidator.PfpValidator(company.ProfilePicturePath);
            _companyValidator.LogoValidator(company.CompanyLogoPath);
            _companyValidator.LocationValidator(company.Location);
            _companyValidator.EmailValidator(company.Email);
        }

        public void AddCompany(string companyName, string aboutUs, string pfpUrl, string logoUrl, string location, string email)
        {
            Company companyToBeAdded = new Company(companyName, aboutUs, pfpUrl, logoUrl, location, email);
            ValidateCompany(companyToBeAdded);
            CompanyRepo.Add(companyToBeAdded);
        }

        public Company? GetCompanyById(int companyId)
        {
            return CompanyRepo.GetById(companyId);
        }

        public void UpdateCompany(Company company)
        {
            ValidateCompany(company);
            CompanyRepo.Update(company);
        }

        public void RemoveCompany(int companyId)
        {
            CompanyRepo.Remove(companyId);
        }

        public void PrintAll()
        {
            this.CompanyRepo.PrintAll();
        }

        /// <summary>
        /// Function that searches a company by name and returns it
        /// </summary>
        /// <param name="companyName"> the name of the company being searched </param>
        /// <returns> the company if found, else null </returns>
        public Company? GetCompanyByName(string companyName)
        {
            return this.CompanyRepo.GetCompanyByName(companyName);
        }
       
    }
}
