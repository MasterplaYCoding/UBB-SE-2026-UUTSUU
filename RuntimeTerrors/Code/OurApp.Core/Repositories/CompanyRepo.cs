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
        ObservableCollection<Company> companies;
        private Company? _currentCompany;

        public CompanyRepo()
        {
            companies = new ObservableCollection<Company>();
        }

        private void ValidateRequiredFields(Company c)
        {
            if (c is null) throw new ArgumentNullException(nameof(c));
            if (string.IsNullOrWhiteSpace(c.Name)) throw new ArgumentException("Company name is required.", nameof(c));
            if (string.IsNullOrWhiteSpace(c.CompanyLogoPath)) throw new ArgumentException("Company logo url/path is required.", nameof(c));
        }

        private static object DbValue(string? value) => value is null ? DBNull.Value : value;
        private static object DbValue(int? value) => value.HasValue ? value.Value : DBNull.Value;

        private Company MapCompany(SqlDataReader reader)
        {
            var company = new Company(
                name: reader["company_name"]?.ToString() ?? "",
                aboutus: reader["about_us"] is DBNull ? "" : reader["about_us"]?.ToString() ?? "",
                pfpUrl: reader["profile_picture_url"] is DBNull ? "" : reader["profile_picture_url"]?.ToString() ?? "",
                logoUrl: reader["logo_picture_url"]?.ToString() ?? "",
                location: reader["location"] is DBNull ? "" : reader["location"]?.ToString() ?? "",
                email: reader["email"] is DBNull ? "" : reader["email"]?.ToString() ?? "",
                companyId: Convert.ToInt32(reader["company_id"]),
                postedJobsCount: reader["posted_jobs_count"] is DBNull ? 0 : Convert.ToInt32(reader["posted_jobs_count"]),
                collaboratorsCount: reader["collaborators_count"] is DBNull ? 0 : Convert.ToInt32(reader["collaborators_count"])
            );

            company.game = MapGame(reader);

            return company;
        }

        public Game? GetGame()
        {
            if (_currentCompany == null)
                return null;

            return _currentCompany.game;
        }

        public void SaveGame(Game game)
        {
            if (_currentCompany == null)
                throw new InvalidOperationException("Nu exista o companie curenta selectata.");

            _currentCompany.game = game;

            using var conn = DbConnectionHelper.GetConnection();
            conn.Open();

            var cmd = new SqlCommand(
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
                conn);

            cmd.Parameters.AddWithValue("@CompanyId", _currentCompany.CompanyId);
            cmd.Parameters.AddWithValue("@BuddyName", DbValue(game.Buddy.Name));
            cmd.Parameters.AddWithValue("@BuddyDescription", DbValue(game.Buddy.Introduction));
            cmd.Parameters.AddWithValue("@AvatarId", DbValue(game.Buddy.Id));
            cmd.Parameters.AddWithValue("@FinalQuote", DbValue(game.Conclusion));
            cmd.Parameters.AddWithValue("@Scenario1Text", DbValue(game.GetScenario(0).Description));
            cmd.Parameters.AddWithValue("@Scenario1Answer1", DbValue(game.GetScenario(0).GetAdviceTexts()[0]));
            cmd.Parameters.AddWithValue("@Scenario1Answer2", DbValue(game.GetScenario(0).GetAdviceTexts()[1]));
            cmd.Parameters.AddWithValue("@Scenario1Answer3", DbValue(game.GetScenario(0).GetAdviceTexts()[2]));
            cmd.Parameters.AddWithValue("@Scenario1Reaction1", DbValue(game.GetScenario(0).GetAdviceReactions()[0]));
            cmd.Parameters.AddWithValue("@Scenario1Reaction2", DbValue(game.GetScenario(0).GetAdviceReactions()[1]));
            cmd.Parameters.AddWithValue("@Scenario1Reaction3", DbValue(game.GetScenario(0).GetAdviceReactions()[2]));
            cmd.Parameters.AddWithValue("@Scenario2Text", DbValue(game.GetScenario(1).Description));
            cmd.Parameters.AddWithValue("@Scenario2Answer1", DbValue(game.GetScenario(1).GetAdviceTexts()[0]));
            cmd.Parameters.AddWithValue("@Scenario2Answer2", DbValue(game.GetScenario(1).GetAdviceTexts()[1]));
            cmd.Parameters.AddWithValue("@Scenario2Answer3", DbValue(game.GetScenario(1).GetAdviceTexts()[2]));
            cmd.Parameters.AddWithValue("@Scenario2Reaction1", DbValue(game.GetScenario(1).GetAdviceReactions()[0]));
            cmd.Parameters.AddWithValue("@Scenario2Reaction2", DbValue(game.GetScenario(1).GetAdviceReactions()[1]));
            cmd.Parameters.AddWithValue("@Scenario2Reaction3", DbValue(game.GetScenario(1).GetAdviceReactions()[2]));

            cmd.ExecuteNonQuery();
        }

        public Game MapGame(SqlDataReader reader)
        {
            var buddy = new Buddy(
                reader["avatar_id"] is DBNull ? 0 : Convert.ToInt32(reader["avatar_id"]),
                reader["buddy_name"]?.ToString() ?? "",
                reader["buddy_description"] is DBNull ? "" : reader["buddy_description"]?.ToString() ?? ""
            );

            var scenarios = new List<Scenario>();

            if (!(reader["scen_1_text"] is DBNull))
            {
                var scen1 = new Scenario(reader["scen_1_text"].ToString());

                scen1.AddChoice(new AdviceChoice(
                    reader["scen1_answer1"]?.ToString() ?? "",
                    reader["scen1_reaction1"]?.ToString() ?? ""
                ));

                scen1.AddChoice(new AdviceChoice(
                    reader["scen1_answer2"]?.ToString() ?? "",
                    reader["scen1_reaction2"]?.ToString() ?? ""
                ));

                scen1.AddChoice(new AdviceChoice(
                    reader["scen1_answer3"]?.ToString() ?? "",
                    reader["scen1_reaction3"]?.ToString() ?? ""
                ));

                scenarios.Add(scen1);
            }

            if (!(reader["scen2_text"] is DBNull))
            {
                var scen2 = new Scenario(reader["scen2_text"].ToString());

                scen2.AddChoice(new AdviceChoice(
                    reader["scen2_answer1"]?.ToString() ?? "",
                    reader["scen2_reaction1"]?.ToString() ?? ""
                ));

                scen2.AddChoice(new AdviceChoice(
                    reader["scen2_answer2"]?.ToString() ?? "",
                    reader["scen2_reaction2"]?.ToString() ?? ""
                ));

                scen2.AddChoice(new AdviceChoice(
                    reader["scen2_answer3"]?.ToString() ?? "",
                    reader["scen2_reaction3"]?.ToString() ?? ""
                ));

                scenarios.Add(scen2);
            }

            return new Game(
                buddy,
                scenarios,
                reader["final_quote"]?.ToString() ?? "",
                true
            );
        }

        private void RefreshCompaniesFromDatabase()
        {
            var list = new ObservableCollection<Company>();

            using var conn = DbConnectionHelper.GetConnection();
            conn.Open();

            var cmd = new SqlCommand(
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
                conn);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(MapCompany(reader));
            }

            companies = list;
        }

        public void PrintAll()
        {
            RefreshCompaniesFromDatabase();
            foreach (var c in companies)
                System.Diagnostics.Debug.WriteLine($"{c} ");
        }

        ObservableCollection<Company> ICompanyRepo.GetAll()
        {
            RefreshCompaniesFromDatabase();
            return companies;
        }

        Company? ICompanyRepo.GetById(int companyId)
        {
            using var conn = DbConnectionHelper.GetConnection();
            conn.Open();

            var cmd = new SqlCommand(
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
                conn);
            cmd.Parameters.AddWithValue("@CompanyId", companyId);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return null;

            _currentCompany = MapCompany(reader);
            return _currentCompany;
        }

        void ICompanyRepo.Add(Company c)
        {
            ValidateRequiredFields(c);

            using var conn = DbConnectionHelper.GetConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();
            var nextIdCmd = new SqlCommand(
                @"SELECT COALESCE(MAX(company_id), 0) + 1
                  FROM companies WITH (UPDLOCK, HOLDLOCK);",
                conn,
                tx);
            int nextId = (int)nextIdCmd.ExecuteScalar();

            var insertCmd = new SqlCommand(
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
                conn,
                tx);

            insertCmd.Parameters.AddWithValue("@CompanyId", nextId);
            insertCmd.Parameters.AddWithValue("@Name", c.Name);
            insertCmd.Parameters.AddWithValue("@AboutUs", DbValue(c.AboutUs));
            insertCmd.Parameters.AddWithValue("@ProfilePictureUrl", DbValue(c.ProfilePicturePath));
            insertCmd.Parameters.AddWithValue("@LogoPictureUrl", c.CompanyLogoPath);
            insertCmd.Parameters.AddWithValue("@Location", DbValue(c.Location));
            insertCmd.Parameters.AddWithValue("@Email", DbValue(c.Email));
            insertCmd.Parameters.AddWithValue("@BuddyName", DbValue(c.game.Buddy.Name));
            insertCmd.Parameters.AddWithValue("@BuddyDescription", DbValue(c.game.Buddy.Introduction));
            insertCmd.Parameters.AddWithValue("@AvatarId", DbValue(c.game.Buddy.Id));
            insertCmd.Parameters.AddWithValue("@FinalQuote", DbValue(c.game.Conclusion));
            insertCmd.Parameters.AddWithValue("@Scenario1Text", DbValue(c.game.GetScenario(0).Description));
            insertCmd.Parameters.AddWithValue("@Scenario1Answer1", DbValue(c.game.GetScenario(0).GetAdviceTexts()[0]));
            insertCmd.Parameters.AddWithValue("@Scenario1Answer2", DbValue(c.game.GetScenario(0).GetAdviceTexts()[1]));
            insertCmd.Parameters.AddWithValue("@Scenario1Answer3", DbValue(c.game.GetScenario(0).GetAdviceTexts()[2]));
            insertCmd.Parameters.AddWithValue("@Scenario1Reaction1", DbValue(c.game.GetScenario(0).GetAdviceReactions()[0]));
            insertCmd.Parameters.AddWithValue("@Scenario1Reaction2", DbValue(c.game.GetScenario(0).GetAdviceReactions()[1]));
            insertCmd.Parameters.AddWithValue("@Scenario1Reaction3", DbValue(c.game.GetScenario(0).GetAdviceReactions()[2]));
            insertCmd.Parameters.AddWithValue("@Scenario2Text", DbValue(c.game.GetScenario(1).Description));
            insertCmd.Parameters.AddWithValue("@Scenario2Answer1", DbValue(c.game.GetScenario(1).GetAdviceTexts()[0]));
            insertCmd.Parameters.AddWithValue("@Scenario2Answer2", DbValue(c.game.GetScenario(1).GetAdviceTexts()[1]));
            insertCmd.Parameters.AddWithValue("@Scenario2Answer3", DbValue(c.game.GetScenario(1).GetAdviceTexts()[2]));
            insertCmd.Parameters.AddWithValue("@Scenario2Reaction1", DbValue(c.game.GetScenario(1).GetAdviceReactions()[0]));
            insertCmd.Parameters.AddWithValue("@Scenario2Reaction2", DbValue(c.game.GetScenario(1).GetAdviceReactions()[1]));
            insertCmd.Parameters.AddWithValue("@Scenario2Reaction3", DbValue(c.game.GetScenario(1).GetAdviceReactions()[2]));

            insertCmd.ExecuteNonQuery();
            c.CompanyId = nextId;

            tx.Commit();
            RefreshCompaniesFromDatabase();
        }

        void ICompanyRepo.Remove(int companyId)
        {
            using var conn = DbConnectionHelper.GetConnection();
            conn.Open();

            var deleteCmd = new SqlCommand(
                @"DELETE FROM companies
                  WHERE company_id = @CompanyId;",
                conn);
            deleteCmd.Parameters.AddWithValue("@CompanyId", companyId);
            deleteCmd.ExecuteNonQuery();

            RefreshCompaniesFromDatabase();
        }

        void ICompanyRepo.Update(Company c)
        {
            ValidateRequiredFields(c);

            using var conn = DbConnectionHelper.GetConnection();
            conn.Open();

            var cmd = new SqlCommand(
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
                conn);

            cmd.Parameters.AddWithValue("@CompanyId", c.CompanyId);
            cmd.Parameters.AddWithValue("@Name", c.Name);
            cmd.Parameters.AddWithValue("@AboutUs", DbValue(c.AboutUs));
            cmd.Parameters.AddWithValue("@ProfilePictureUrl", DbValue(c.ProfilePicturePath));
            cmd.Parameters.AddWithValue("@LogoPictureUrl", c.CompanyLogoPath);
            cmd.Parameters.AddWithValue("@Location", DbValue(c.Location));
            cmd.Parameters.AddWithValue("@Email", DbValue(c.Email));
            cmd.Parameters.AddWithValue("@BuddyName", DbValue(c.game.Buddy.Name));
            cmd.Parameters.AddWithValue("@BuddyDescription", DbValue(c.game.Buddy.Introduction));
            cmd.Parameters.AddWithValue("@AvatarId", DbValue(c.game.Buddy.Id));
            cmd.Parameters.AddWithValue("@FinalQuote", DbValue(c.game.Conclusion));
            cmd.Parameters.AddWithValue("@Scenario1Text", DbValue(c.game.GetScenario(0).Description));
            cmd.Parameters.AddWithValue("@Scenario1Answer1", DbValue(c.game.GetScenario(0).GetAdviceTexts()[0]));
            cmd.Parameters.AddWithValue("@Scenario1Answer2", DbValue(c.game.GetScenario(0).GetAdviceTexts()[1]));
            cmd.Parameters.AddWithValue("@Scenario1Answer3", DbValue(c.game.GetScenario(0).GetAdviceTexts()[2]));
            cmd.Parameters.AddWithValue("@Scenario1Reaction1", DbValue(c.game.GetScenario(0).GetAdviceReactions()[0]));
            cmd.Parameters.AddWithValue("@Scenario1Reaction2", DbValue(c.game.GetScenario(0).GetAdviceReactions()[1]));
            cmd.Parameters.AddWithValue("@Scenario1Reaction3", DbValue(c.game.GetScenario(0).GetAdviceReactions()[2]));
            cmd.Parameters.AddWithValue("@Scenario2Text", DbValue(c.game.GetScenario(1).Description));
            cmd.Parameters.AddWithValue("@Scenario2Answer1", DbValue(c.game.GetScenario(1).GetAdviceTexts()[0]));
            cmd.Parameters.AddWithValue("@Scenario2Answer2", DbValue(c.game.GetScenario(1).GetAdviceTexts()[1]));
            cmd.Parameters.AddWithValue("@Scenario2Answer3", DbValue(c.game.GetScenario(1).GetAdviceTexts()[2]));
            cmd.Parameters.AddWithValue("@Scenario2Reaction1", DbValue(c.game.GetScenario(1).GetAdviceReactions()[0]));
            cmd.Parameters.AddWithValue("@Scenario2Reaction2", DbValue(c.game.GetScenario(1).GetAdviceReactions()[1]));
            cmd.Parameters.AddWithValue("@Scenario2Reaction3", DbValue(c.game.GetScenario(1).GetAdviceReactions()[2]));

            int affected = cmd.ExecuteNonQuery();
            if (affected == 0)
                throw new InvalidOperationException($"No company found with id '{c.CompanyId}' to update.");

            RefreshCompaniesFromDatabase();
        }

        public Company? GetCompanyByName(string companyName)
        {
            if (string.IsNullOrWhiteSpace(companyName))
                return null;

            using var sqlConnection = DbConnectionHelper.GetConnection();
            sqlConnection.Open();

            string query = "SELECT * FROM companies WHERE company_name = @Name";

            using var sqlCommand = new SqlCommand(query, sqlConnection);
            sqlCommand.Parameters.AddWithValue("@Name", companyName);

            using var reader = sqlCommand.ExecuteReader();

            if (reader.Read())
            {
                return MapCompany(reader);
            }

            return null;
        }
    }
}