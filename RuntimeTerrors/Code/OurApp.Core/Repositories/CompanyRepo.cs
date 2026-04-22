using Microsoft.Data.SqlClient;
using OurApp.Core.Database;
using OurApp.Core.Models;
using OurApp.Core.Repositories;
using System;
using System.Collections.ObjectModel;

namespace OurApp.Core.Repositories
{
    public class CompanyRepo : ICompanyRepo
    {
        private ObservableCollection<Company> _companies;
        private Company? _currentCompany;

        public CompanyRepo()
        {
            _companies = new ObservableCollection<Company>();
        }

        private void ValidateRequiredFields(Company company)
        {
            if (company is null) throw new ArgumentNullException(nameof(company));
            if (string.IsNullOrWhiteSpace(company.Name)) throw new ArgumentException("Company name is required.", nameof(company));
            if (string.IsNullOrWhiteSpace(company.CompanyLogoPath)) throw new ArgumentException("Company logo url/path is required.", nameof(company));
        }

        private static object GetDatabaseValue(string? value) => value is null ? DBNull.Value : value;
        private static object GetDatabaseValue(int? value) => value.HasValue ? value.Value : DBNull.Value;

        private Company MapCompany(SqlDataReader dataReader)
        {
            var company = new Company(
                name: dataReader["company_name"]?.ToString() ?? "",
                aboutus: dataReader["about_us"] is DBNull ? "" : dataReader["about_us"]?.ToString() ?? "",
                pfpUrl: dataReader["profile_picture_url"] is DBNull ? "" : dataReader["profile_picture_url"]?.ToString() ?? "",
                logoUrl: dataReader["logo_picture_url"]?.ToString() ?? "",
                location: dataReader["location"] is DBNull ? "" : dataReader["location"]?.ToString() ?? "",
                email: dataReader["email"] is DBNull ? "" : dataReader["email"]?.ToString() ?? "",
                companyId: Convert.ToInt32(dataReader["company_id"]),
                postedJobsCount: dataReader["posted_jobs_count"] is DBNull ? 0 : Convert.ToInt32(dataReader["posted_jobs_count"]),
                collaboratorsCount: dataReader["collaborators_count"] is DBNull ? 0 : Convert.ToInt32(dataReader["collaborators_count"])
            );

            company.Game = MapGame(dataReader);

            return company;
        }

        public Game? GetGame()
        {
            if (_currentCompany == null)
                return null;

            return _currentCompany.Game;
        }

        public void SaveGame(Game game)
        {
            if (_currentCompany == null)
                throw new InvalidOperationException("Nu exista o companie curenta selectata.");

            _currentCompany.Game = game;

            using var databaseConnection = DbConnectionHelper.GetConnection();
            databaseConnection.Open();

            var sqlCommand = new SqlCommand(
                @"UPDATE companies
                  SET buddy_name        = @BuddyName,
                      buddy_description = @BuddyDescription,
                      avatar_id         = @AvatarId,
                      final_quote       = @FinalQuote,
                      scen_1_text       = @Scenario1Text,
                      scen1_answer1     = @Scenario1Answer1,
                      scen1_answer2     = @Scenario1Answer2,
                      scen1_answer3     = @Scenario1Answer3,
                      scen1_reaction1   = @Scenario1Reaction1,
                      scen1_reaction2   = @Scenario1Reaction2,
                      scen1_reaction3   = @Scenario1Reaction3,
                      scen2_text        = @Scenario2Text,
                      scen2_answer1     = @Scenario2Answer1,
                      scen2_answer2     = @Scenario2Answer2,
                      scen2_answer3     = @Scenario2Answer3,
                      scen2_reaction1   = @Scenario2Reaction1,
                      scen2_reaction2   = @Scenario2Reaction2,
                      scen2_reaction3   = @Scenario2Reaction3
                  WHERE company_id = @CompanyId",
                databaseConnection);

            sqlCommand.Parameters.AddWithValue("@CompanyId", _currentCompany.CompanyId);
            sqlCommand.Parameters.AddWithValue("@BuddyName", GetDatabaseValue(game.Buddy.Name));
            sqlCommand.Parameters.AddWithValue("@BuddyDescription", GetDatabaseValue(game.Buddy.Introduction));
            sqlCommand.Parameters.AddWithValue("@AvatarId", GetDatabaseValue(game.Buddy.Id));
            sqlCommand.Parameters.AddWithValue("@FinalQuote", GetDatabaseValue(game.Conclusion));
            sqlCommand.Parameters.AddWithValue("@Scenario1Text", GetDatabaseValue(game.GetScenario(0).Description));
            sqlCommand.Parameters.AddWithValue("@Scenario1Answer1", GetDatabaseValue(game.GetScenario(0).GetAdviceTexts()[0]));
            sqlCommand.Parameters.AddWithValue("@Scenario1Answer2", GetDatabaseValue(game.GetScenario(0).GetAdviceTexts()[1]));
            sqlCommand.Parameters.AddWithValue("@Scenario1Answer3", GetDatabaseValue(game.GetScenario(0).GetAdviceTexts()[2]));
            sqlCommand.Parameters.AddWithValue("@Scenario1Reaction1", GetDatabaseValue(game.GetScenario(0).GetAdviceReactions()[0]));
            sqlCommand.Parameters.AddWithValue("@Scenario1Reaction2", GetDatabaseValue(game.GetScenario(0).GetAdviceReactions()[1]));
            sqlCommand.Parameters.AddWithValue("@Scenario1Reaction3", GetDatabaseValue(game.GetScenario(0).GetAdviceReactions()[2]));
            sqlCommand.Parameters.AddWithValue("@Scenario2Text", GetDatabaseValue(game.GetScenario(1).Description));
            sqlCommand.Parameters.AddWithValue("@Scenario2Answer1", GetDatabaseValue(game.GetScenario(1).GetAdviceTexts()[0]));
            sqlCommand.Parameters.AddWithValue("@Scenario2Answer2", GetDatabaseValue(game.GetScenario(1).GetAdviceTexts()[1]));
            sqlCommand.Parameters.AddWithValue("@Scenario2Answer3", GetDatabaseValue(game.GetScenario(1).GetAdviceTexts()[2]));
            sqlCommand.Parameters.AddWithValue("@Scenario2Reaction1", GetDatabaseValue(game.GetScenario(1).GetAdviceReactions()[0]));
            sqlCommand.Parameters.AddWithValue("@Scenario2Reaction2", GetDatabaseValue(game.GetScenario(1).GetAdviceReactions()[1]));
            sqlCommand.Parameters.AddWithValue("@Scenario2Reaction3", GetDatabaseValue(game.GetScenario(1).GetAdviceReactions()[2]));

            sqlCommand.ExecuteNonQuery();
        }

        public Game MapGame(SqlDataReader dataReader)
        {
            var buddy = new Buddy(
                dataReader["avatar_id"] is DBNull ? 0 : Convert.ToInt32(dataReader["avatar_id"]),
                dataReader["buddy_name"]?.ToString() ?? "",
                dataReader["buddy_description"] is DBNull ? "" : dataReader["buddy_description"]?.ToString() ?? ""
            );

            var scenarios = new List<Scenario>();

            if (!(dataReader["scen_1_text"] is DBNull))
            {
                var scenario1 = new Scenario(dataReader["scen_1_text"].ToString());

                scenario1.AddChoice(new AdviceChoice(
                    dataReader["scen1_answer1"]?.ToString() ?? "",
                    dataReader["scen1_reaction1"]?.ToString() ?? ""
                ));

                scenario1.AddChoice(new AdviceChoice(
                    dataReader["scen1_answer2"]?.ToString() ?? "",
                    dataReader["scen1_reaction2"]?.ToString() ?? ""
                ));

                scenario1.AddChoice(new AdviceChoice(
                    dataReader["scen1_answer3"]?.ToString() ?? "",
                    dataReader["scen1_reaction3"]?.ToString() ?? ""
                ));

                scenarios.Add(scenario1);
            }

            if (!(dataReader["scen2_text"] is DBNull))
            {
                var scenario2 = new Scenario(dataReader["scen2_text"].ToString());

                scenario2.AddChoice(new AdviceChoice(
                    dataReader["scen2_answer1"]?.ToString() ?? "",
                    dataReader["scen2_reaction1"]?.ToString() ?? ""
                ));

                scenario2.AddChoice(new AdviceChoice(
                    dataReader["scen2_answer2"]?.ToString() ?? "",
                    dataReader["scen2_reaction2"]?.ToString() ?? ""
                ));

                scenario2.AddChoice(new AdviceChoice(
                    dataReader["scen2_answer3"]?.ToString() ?? "",
                    dataReader["scen2_reaction3"]?.ToString() ?? ""
                ));

                scenarios.Add(scenario2);
            }

            return new Game(
                buddy,
                scenarios,
                dataReader["final_quote"]?.ToString() ?? "",
                true
            );
        }

        private void RefreshCompaniesFromDatabase()
        {
            var companyList = new ObservableCollection<Company>();

            using var databaseConnection = DbConnectionHelper.GetConnection();
            databaseConnection.Open();

            var sqlCommand = new SqlCommand(
                @"SELECT 
                    c.company_id, c.company_name, c.about_us, c.profile_picture_url, 
                    c.logo_picture_url, c.location, c.email,
                    c.buddy_name, c.buddy_description, c.avatar_id, c.final_quote,
                    c.scen_1_text, c.scen1_answer1, c.scen1_answer2, c.scen1_answer3,
                    c.scen1_reaction1, c.scen1_reaction2, c.scen1_reaction3,
                    c.scen2_text, c.scen2_answer1, c.scen2_answer2,
                    c.scen2_answer3, c.scen2_reaction1, c.scen2_reaction2, c.scen2_reaction3,

                    (SELECT COUNT(*) FROM jobs j WHERE j.company_id = c.company_id) AS posted_jobs_count,

                    (SELECT COUNT(DISTINCT ec.company_id)
                     FROM collaborators ec) AS collaborators_count

                FROM companies c;",
                databaseConnection);

            using var dataReader = sqlCommand.ExecuteReader();
            while (dataReader.Read())
            {
                companyList.Add(MapCompany(dataReader));
            }

            _companies = companyList;
        }

        public void PrintAll()
        {
            RefreshCompaniesFromDatabase();
            foreach (var company in _companies)
                System.Diagnostics.Debug.WriteLine($"{company} ");
        }

        ObservableCollection<Company> ICompanyRepo.GetAll()
        {
            RefreshCompaniesFromDatabase();
            return _companies;
        }

        Company? ICompanyRepo.GetById(int companyId)
        {
            using var databaseConnection = DbConnectionHelper.GetConnection();
            databaseConnection.Open();

            var sqlCommand = new SqlCommand(
                @"SELECT c.company_id, c.company_name, c.about_us, c.profile_picture_url,
                    c.logo_picture_url, c.location, c.email,
                    c.buddy_name, c.buddy_description, c.avatar_id, c.final_quote,
                    c.scen_1_text, c.scen1_answer1, c.scen1_answer2, c.scen1_answer3,
                    c.scen1_reaction1, c.scen1_reaction2, c.scen1_reaction3,
                    c.scen2_text, c.scen2_answer1, c.scen2_answer2, c.scen2_answer3,
                    c.scen2_reaction1, c.scen2_reaction2, c.scen2_reaction3,
                    (SELECT COUNT(*) FROM jobs j WHERE j.company_id = c.company_id) AS posted_jobs_count,
                    (SELECT COUNT(DISTINCT ec.company_id)
                    FROM collaborators ec) AS collaborators_count
                  FROM companies c
                  WHERE c.company_id = @CompanyId;",
                databaseConnection);
            sqlCommand.Parameters.AddWithValue("@CompanyId", companyId);
            using var dataReader = sqlCommand.ExecuteReader();
            if (!dataReader.Read())
                return null;

            _currentCompany = MapCompany(dataReader);
            return _currentCompany;
        }

        void ICompanyRepo.Add(Company company)
        {
            ValidateRequiredFields(company);

            using var databaseConnection = DbConnectionHelper.GetConnection();
            databaseConnection.Open();
            using var sqlTransaction = databaseConnection.BeginTransaction();
            var nextIdCommand = new SqlCommand(
                @"SELECT COALESCE(MAX(company_id), 0) + 1
                  FROM companies WITH (UPDLOCK, HOLDLOCK);",
                databaseConnection,
                sqlTransaction);
            int nextId = (int)nextIdCommand.ExecuteScalar();

            var insertCommand = new SqlCommand(
                @"INSERT INTO companies
                (company_id, company_name, about_us, profile_picture_url, logo_picture_url, location, email,
                 buddy_name, buddy_description, avatar_id, final_quote,
                 scen_1_text, scen1_answer1, scen1_answer2, scen1_answer3,
                 scen1_reaction1, scen1_reaction2, scen1_reaction3,
                 scen2_text, scen2_answer1, scen2_answer2,
                 scen2_answer3, scen2_reaction1, scen2_reaction2, scen2_reaction3)
                VALUES
                (@CompanyId, @Name, @AboutUs, @ProfilePictureUrl, @LogoPictureUrl, @Location, @Email,
                 @BuddyName, @BuddyDescription, @AvatarId, @FinalQuote,
                 @Scenario1Text, @Scenario1Answer1, @Scenario1Answer2, @Scenario1Answer3,
                 @Scenario1Reaction1, @Scenario1Reaction2, @Scenario1Reaction3,
                 @Scenario2Text, @Scenario2Answer1, @Scenario2Answer2,
                 @Scenario2Answer3, @Scenario2Reaction1, @Scenario2Reaction2, @Scenario2Reaction3)",
                databaseConnection,
                sqlTransaction);

            insertCommand.Parameters.AddWithValue("@CompanyId", nextId);
            insertCommand.Parameters.AddWithValue("@Name", company.Name);
            insertCommand.Parameters.AddWithValue("@AboutUs", GetDatabaseValue(company.AboutUs));
            insertCommand.Parameters.AddWithValue("@ProfilePictureUrl", GetDatabaseValue(company.ProfilePicturePath));
            insertCommand.Parameters.AddWithValue("@LogoPictureUrl", company.CompanyLogoPath);
            insertCommand.Parameters.AddWithValue("@Location", GetDatabaseValue(company.Location));
            insertCommand.Parameters.AddWithValue("@Email", GetDatabaseValue(company.Email));
            insertCommand.Parameters.AddWithValue("@BuddyName", GetDatabaseValue(company.Game.Buddy.Name));
            insertCommand.Parameters.AddWithValue("@BuddyDescription", GetDatabaseValue(company.Game.Buddy.Introduction));
            insertCommand.Parameters.AddWithValue("@AvatarId", GetDatabaseValue(company.Game.Buddy.Id));
            insertCommand.Parameters.AddWithValue("@FinalQuote", GetDatabaseValue(company.Game.Conclusion));
            insertCommand.Parameters.AddWithValue("@Scenario1Text", GetDatabaseValue(company.Game.GetScenario(0).Description));
            insertCommand.Parameters.AddWithValue("@Scenario1Answer1", GetDatabaseValue(company.Game.GetScenario(0).GetAdviceTexts()[0]));
            insertCommand.Parameters.AddWithValue("@Scenario1Answer2", GetDatabaseValue(company.Game.GetScenario(0).GetAdviceTexts()[1]));
            insertCommand.Parameters.AddWithValue("@Scenario1Answer3", GetDatabaseValue(company.Game.GetScenario(0).GetAdviceTexts()[2]));
            insertCommand.Parameters.AddWithValue("@Scenario1Reaction1", GetDatabaseValue(company.Game.GetScenario(0).GetAdviceReactions()[0]));
            insertCommand.Parameters.AddWithValue("@Scenario1Reaction2", GetDatabaseValue(company.Game.GetScenario(0).GetAdviceReactions()[1]));
            insertCommand.Parameters.AddWithValue("@Scenario1Reaction3", GetDatabaseValue(company.Game.GetScenario(0).GetAdviceReactions()[2]));
            insertCommand.Parameters.AddWithValue("@Scenario2Text", GetDatabaseValue(company.Game.GetScenario(1).Description));
            insertCommand.Parameters.AddWithValue("@Scenario2Answer1", GetDatabaseValue(company.Game.GetScenario(1).GetAdviceTexts()[0]));
            insertCommand.Parameters.AddWithValue("@Scenario2Answer2", GetDatabaseValue(company.Game.GetScenario(1).GetAdviceTexts()[1]));
            insertCommand.Parameters.AddWithValue("@Scenario2Answer3", GetDatabaseValue(company.Game.GetScenario(1).GetAdviceTexts()[2]));
            insertCommand.Parameters.AddWithValue("@Scenario2Reaction1", GetDatabaseValue(company.Game.GetScenario(1).GetAdviceReactions()[0]));
            insertCommand.Parameters.AddWithValue("@Scenario2Reaction2", GetDatabaseValue(company.Game.GetScenario(1).GetAdviceReactions()[1]));
            insertCommand.Parameters.AddWithValue("@Scenario2Reaction3", GetDatabaseValue(company.Game.GetScenario(1).GetAdviceReactions()[2]));

            insertCommand.ExecuteNonQuery();
            company.CompanyId = nextId;

            sqlTransaction.Commit();
            RefreshCompaniesFromDatabase();
        }

        void ICompanyRepo.Remove(int companyId)
        {
            using var databaseConnection = DbConnectionHelper.GetConnection();
            databaseConnection.Open();

            var deleteCommand = new SqlCommand(
                @"DELETE FROM companies
                  WHERE company_id = @CompanyId;",
                databaseConnection);
            deleteCommand.Parameters.AddWithValue("@CompanyId", companyId);
            deleteCommand.ExecuteNonQuery();

            RefreshCompaniesFromDatabase();
        }

        void ICompanyRepo.Update(Company company)
        {
            ValidateRequiredFields(company);

            using var databaseConnection = DbConnectionHelper.GetConnection();
            databaseConnection.Open();

            var sqlCommand = new SqlCommand(
                @"UPDATE companies
                  SET company_name      = @Name,
                      about_us          = @AboutUs,
                      profile_picture_url = @ProfilePictureUrl,
                      logo_picture_url  = @LogoPictureUrl,
                      location          = @Location,
                      email             = @Email,
                      buddy_name        = @BuddyName,
                      buddy_description = @BuddyDescription,
                      avatar_id         = @AvatarId,
                      final_quote       = @FinalQuote,
                      scen_1_text       = @Scenario1Text,
                      scen1_answer1     = @Scenario1Answer1,
                      scen1_answer2     = @Scenario1Answer2,
                      scen1_answer3     = @Scenario1Answer3,
                      scen1_reaction1   = @Scenario1Reaction1,
                      scen1_reaction2   = @Scenario1Reaction2,
                      scen1_reaction3   = @Scenario1Reaction3,
                      scen2_text        = @Scenario2Text,
                      scen2_answer1     = @Scenario2Answer1,
                      scen2_answer2     = @Scenario2Answer2,
                      scen2_answer3     = @Scenario2Answer3,
                      scen2_reaction1   = @Scenario2Reaction1,
                      scen2_reaction2   = @Scenario2Reaction2,
                      scen2_reaction3   = @Scenario2Reaction3
                      
                  WHERE company_id = @CompanyId;",
                databaseConnection);

            sqlCommand.Parameters.AddWithValue("@CompanyId", company.CompanyId);
            sqlCommand.Parameters.AddWithValue("@Name", company.Name);
            sqlCommand.Parameters.AddWithValue("@AboutUs", GetDatabaseValue(company.AboutUs));
            sqlCommand.Parameters.AddWithValue("@ProfilePictureUrl", GetDatabaseValue(company.ProfilePicturePath));
            sqlCommand.Parameters.AddWithValue("@LogoPictureUrl", company.CompanyLogoPath);
            sqlCommand.Parameters.AddWithValue("@Location", GetDatabaseValue(company.Location));
            sqlCommand.Parameters.AddWithValue("@Email", GetDatabaseValue(company.Email));
            sqlCommand.Parameters.AddWithValue("@BuddyName", GetDatabaseValue(company.Game.Buddy.Name));
            sqlCommand.Parameters.AddWithValue("@BuddyDescription", GetDatabaseValue(company.Game.Buddy.Introduction));
            sqlCommand.Parameters.AddWithValue("@AvatarId", GetDatabaseValue(company.Game.Buddy.Id));
            sqlCommand.Parameters.AddWithValue("@FinalQuote", GetDatabaseValue(company.Game.Conclusion));
            sqlCommand.Parameters.AddWithValue("@Scenario1Text", GetDatabaseValue(company.Game.GetScenario(0).Description));
            sqlCommand.Parameters.AddWithValue("@Scenario1Answer1", GetDatabaseValue(company.Game.GetScenario(0).GetAdviceTexts()[0]));
            sqlCommand.Parameters.AddWithValue("@Scenario1Answer2", GetDatabaseValue(company.Game.GetScenario(0).GetAdviceTexts()[1]));
            sqlCommand.Parameters.AddWithValue("@Scenario1Answer3", GetDatabaseValue(company.Game.GetScenario(0).GetAdviceTexts()[2]));
            sqlCommand.Parameters.AddWithValue("@Scenario1Reaction1", GetDatabaseValue(company.Game.GetScenario(0).GetAdviceReactions()[0]));
            sqlCommand.Parameters.AddWithValue("@Scenario1Reaction2", GetDatabaseValue(company.Game.GetScenario(0).GetAdviceReactions()[1]));
            sqlCommand.Parameters.AddWithValue("@Scenario1Reaction3", GetDatabaseValue(company.Game.GetScenario(0).GetAdviceReactions()[2]));
            sqlCommand.Parameters.AddWithValue("@Scenario2Text", GetDatabaseValue(company.Game.GetScenario(1).Description));
            sqlCommand.Parameters.AddWithValue("@Scenario2Answer1", GetDatabaseValue(company.Game.GetScenario(1).GetAdviceTexts()[0]));
            sqlCommand.Parameters.AddWithValue("@Scenario2Answer2", GetDatabaseValue(company.Game.GetScenario(1).GetAdviceTexts()[1]));
            sqlCommand.Parameters.AddWithValue("@Scenario2Answer3", GetDatabaseValue(company.Game.GetScenario(1).GetAdviceTexts()[2]));
            sqlCommand.Parameters.AddWithValue("@Scenario2Reaction1", GetDatabaseValue(company.Game.GetScenario(1).GetAdviceReactions()[0]));
            sqlCommand.Parameters.AddWithValue("@Scenario2Reaction2", GetDatabaseValue(company.Game.GetScenario(1).GetAdviceReactions()[1]));
            sqlCommand.Parameters.AddWithValue("@Scenario2Reaction3", GetDatabaseValue(company.Game.GetScenario(1).GetAdviceReactions()[2]));

            int affectedRows = sqlCommand.ExecuteNonQuery();
            if (affectedRows == 0)
                throw new InvalidOperationException($"No company found with id '{company.CompanyId}' to update.");

            RefreshCompaniesFromDatabase();
        }

        public Company? GetCompanyByName(string companyName)
        {
            if (string.IsNullOrWhiteSpace(companyName))
                return null;

            using var sqlConnection = DbConnectionHelper.GetConnection();
            sqlConnection.Open();

            string sqlQuery = "SELECT * FROM companies WHERE company_name = @Name";

            using var sqlCommand = new SqlCommand(sqlQuery, sqlConnection);
            sqlCommand.Parameters.AddWithValue("@Name", companyName);

            using var dataReader = sqlCommand.ExecuteReader();

            if (dataReader.Read())
            {
                return MapCompany(dataReader);
            }

            return null;
        }
    }
}