using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using OurApp.Core.Database;
using OurApp.Core.Models;

namespace OurApp.Core.Repositories
{
    public class CollaboratorsRepo : ICollaboratorsRepo
    {
        /// <summary>
        /// Function that adds a collaborator to the collaborators table
        /// </summary>
        /// <param name="eventOfCollaboration"> the event that the invited company is collaborating on </param>
        /// <param name="collaboratorToBeAdded"> the company that has been invited to collaborate </param>
        /// <param name="loggedInUserID"></param>
        public void AddCollaboratorToRepo(Event eventOfCollaboration, Company collaboratorToBeAdded, int loggedInUserID)
        {
            using (SqlConnection sqlConnection = DbConnectionHelper.GetConnection())
            {
                sqlConnection.Open();
                const int NewCollaboratorIndicator = 1;

                using (SqlTransaction transaction = sqlConnection.BeginTransaction())
                {
                    try
                    {
                        string insertQuery = @"
                            INSERT INTO collaborators (event_id, company_id)
                            VALUES (@EventId, @CompanyId)";

                        SqlCommand insertCommand = new SqlCommand(insertQuery, sqlConnection, transaction);
                        insertCommand.Parameters.AddWithValue("@EventId", eventOfCollaboration.Id);
                        insertCommand.Parameters.AddWithValue("@CompanyId", collaboratorToBeAdded.CompanyId);

                        insertCommand.ExecuteNonQuery();

                        string checkQuery = @"
                            SELECT COUNT(*)
                            FROM collaborators c
                            INNER JOIN events e ON e.event_id = c.event_id
                            WHERE e.host_company_id = @HostID
                            AND c.company_id = @CollaboratorId";

                        SqlCommand checkCommand = new SqlCommand(checkQuery, sqlConnection, transaction);
                        checkCommand.Parameters.AddWithValue("@HostID", loggedInUserID);
                        checkCommand.Parameters.AddWithValue("@CollaboratorId", collaboratorToBeAdded.CompanyId);

                        int existingCount = (int)checkCommand.ExecuteScalar();
                        bool isNewCollaborator = existingCount == NewCollaboratorIndicator;

                        if (isNewCollaborator)
                        {
                            string updateQuery = @"
                            UPDATE companies
                            SET collaborators_count = collaborators_count + 1
                            WHERE company_id = @CompanyId";

                            SqlCommand updateCommand = new SqlCommand(updateQuery, sqlConnection, transaction);
                            updateCommand.Parameters.AddWithValue("@CompanyId", loggedInUserID);

                            updateCommand.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Function that returns a list of all the collaborators of the user company
        /// </summary>
        /// <param name="loggedInCompanyId">  </param>
        /// <returns></returns>
        public List<Company> GetAllCollaborators(int loggedInCompanyId)
        {
            var usersCollaborators = new List<Company>();

            using (SqlConnection sqlConnection = DbConnectionHelper.GetConnection())
            {
                sqlConnection.Open();

                string queryToBeRun = @"
                    SELECT *
                    FROM companies
                    WHERE company_id IN (
                        SELECT c2.company_id
                        FROM collaborators c2
                        INNER JOIN events e ON e.event_id = c2.event_id
                        WHERE e.host_company_id = @HostID
                    )
                    ";

                SqlCommand sqlCommand = new SqlCommand(queryToBeRun, sqlConnection);
                sqlCommand.Parameters.AddWithValue("@HostID", loggedInCompanyId);

                SqlDataReader reader = sqlCommand.ExecuteReader();
                string defaultDatabaseStringValue = string.Empty;

                while (reader.Read())
                {
                    usersCollaborators.Add(new Company(reader["company_name"].ToString(),
                        defaultDatabaseStringValue,
                        defaultDatabaseStringValue,
                        reader["logo_picture_url"].ToString(),
                        defaultDatabaseStringValue,
                        defaultDatabaseStringValue));
                }
            }

            return usersCollaborators;
        }
    }
}
