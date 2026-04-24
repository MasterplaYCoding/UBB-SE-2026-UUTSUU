using System.Collections.Generic;
using System.Collections.ObjectModel;
using OurApp.Core.Models;
using OurApp.Core.Repositories;

namespace OurApp.Tests.Helpers
{
    public class FakeCompanyRepository : ICompanyRepo
    {
        public Company LastAdded;
        public Company LastUpdated;
        public int LastRemovedId;
        public bool PrintAllCalled;
        public Company StoredCompany;

        public void Add(Company company)
        {
            LastAdded = company;
        }

        public Company GetById(int id)
        {
            return StoredCompany;
        }

        public void Update(Company company)
        {
            LastUpdated = company;
        }

        public void Remove(int id)
        {
            LastRemovedId = id;
        }

        public void PrintAll()
        {
            PrintAllCalled = true;
        }

        public Company GetCompanyByName(string name)
        {
            return StoredCompany;
        }

        public ObservableCollection<Company> GetAll()
        {
            return new ObservableCollection<Company>();
        }

        public Game GetGame()
        {
            return null;
        }

        public void SaveGame(Game game)
        {
        }
    }
}