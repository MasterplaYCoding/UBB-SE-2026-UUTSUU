using Microsoft.Data.SqlClient;
using OurApp.Core.Database;
using OurApp.Core.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OurApp.Core.Repositories
{
    public class EventsRepo : IEventsRepo
    {
        private const int DefaultHostCompanyId = 1;
        private const int EmptyEventIdFallback = 0;

        public int GetMaxEventId()
        {
            using (SqlConnection databaseConnection = DbConnectionHelper.GetConnection())
            {
                databaseConnection.Open();

                string sqlQuery = "SELECT MAX(event_id) FROM events";

                SqlCommand sqlCommand = new SqlCommand(sqlQuery, databaseConnection);

                object queryResult = sqlCommand.ExecuteScalar();

                if (queryResult == DBNull.Value)
                {
                    return EmptyEventIdFallback;
                }

                return Convert.ToInt32(queryResult);
            }
        }

        public void AddEventToRepo(Event eventToBeAdded)
        {
            using var databaseConnection = DbConnectionHelper.GetConnection();
            databaseConnection.Open();

            using var sqlTransaction = databaseConnection.BeginTransaction();

            try
            {
                int nextId;
                using (var nextIdCommand = new SqlCommand(
                    "SELECT COALESCE(MAX(event_id), 0) + 1 FROM events WITH (UPDLOCK, HOLDLOCK)",
                    databaseConnection, sqlTransaction))
                {
                    nextId = (int)nextIdCommand.ExecuteScalar();
                }

                using (var insertEventCommand = new SqlCommand(@"
                    INSERT INTO events 
                    (event_id, photo, title, description, start_date, end_date, location, host_company_id, posted_at)
                    VALUES (@Id, @Photo, @Title, @Description, @StartDate, @EndDate, @Location, @Host, @Now)",
                    databaseConnection, sqlTransaction))
                {
                    insertEventCommand.Parameters.AddWithValue("@Id", nextId);
                    insertEventCommand.Parameters.AddWithValue("@Photo", (object?)eventToBeAdded.Photo ?? DBNull.Value);
                    insertEventCommand.Parameters.AddWithValue("@Title", eventToBeAdded.Title);
                    insertEventCommand.Parameters.AddWithValue("@Description", (object?)eventToBeAdded.Description ?? DBNull.Value);
                    insertEventCommand.Parameters.AddWithValue("@StartDate", eventToBeAdded.StartDate);
                    insertEventCommand.Parameters.AddWithValue("@EndDate", eventToBeAdded.EndDate);
                    insertEventCommand.Parameters.AddWithValue("@Location", eventToBeAdded.Location);
                    insertEventCommand.Parameters.AddWithValue("@Host", eventToBeAdded.HostID);
                    insertEventCommand.Parameters.AddWithValue("@Now", DateTime.Now);

                    insertEventCommand.ExecuteNonQuery();
                }

                eventToBeAdded.Id = nextId;

                if (eventToBeAdded.Collaborators != null)
                {
                    foreach (var collaborator in eventToBeAdded.Collaborators)
                    {
                        using var checkCollaboratorCommand = new SqlCommand(@"
                            SELECT COUNT(*) 
                            FROM collaborators 
                            WHERE company_id = @CompanyId",
                            databaseConnection, sqlTransaction);

                        checkCollaboratorCommand.Parameters.AddWithValue("@CompanyId", collaborator.CompanyId);
                        int existingCount = (int)checkCollaboratorCommand.ExecuteScalar();

                        using var insertCollaboratorCommand = new SqlCommand(@"
                            INSERT INTO collaborators (event_id, company_id)
                            VALUES (@EventId, @CompanyId)",
                            databaseConnection, sqlTransaction);

                        insertCollaboratorCommand.Parameters.AddWithValue("@EventId", nextId);
                        insertCollaboratorCommand.Parameters.AddWithValue("@CompanyId", collaborator.CompanyId);
                        insertCollaboratorCommand.ExecuteNonQuery();

                        if (existingCount == 0)
                        {
                            using var updateCompanyCommand = new SqlCommand(@"
                                UPDATE companies
                                SET collaborators_count = collaborators_count + 1
                                WHERE company_id = @CompanyId",
                                databaseConnection, sqlTransaction);

                            updateCompanyCommand.Parameters.AddWithValue("@CompanyId", collaborator.CompanyId);
                            updateCompanyCommand.ExecuteNonQuery();
                        }
                    }
                }

                sqlTransaction.Commit();
            }
            catch
            {
                sqlTransaction.Rollback();
                throw;
            }
        }

        public void RemoveEventFromRepo(Event eventToBeRemoved)
        {
            using (SqlConnection databaseConnection = DbConnectionHelper.GetConnection())
            {
                databaseConnection.Open();

                string sqlQuery = "DELETE FROM events WHERE event_id = @Id";

                SqlCommand sqlCommand = new SqlCommand(sqlQuery, databaseConnection);
                sqlCommand.Parameters.AddWithValue("@Id", eventToBeRemoved.Id);

                sqlCommand.ExecuteNonQuery();
            }
        }

        public ObservableCollection<Event> getCurrentEventsFromRepo(int loggedInUser)
        {
            var currentEvents = new ObservableCollection<Event>();

            try
            {
                using (SqlConnection databaseConnection = DbConnectionHelper.GetConnection())
                {
                    databaseConnection.Open();

                    string sqlQuery = "SELECT * FROM events WHERE host_company_id = @HostId and end_date >= @TodaysDate";

                    SqlCommand sqlCommand = new SqlCommand(sqlQuery, databaseConnection);

                    sqlCommand.Parameters.AddWithValue("@HostId", loggedInUser);
                    sqlCommand.Parameters.AddWithValue("@TodaysDate", DateTime.Now.Date);

                    SqlDataReader dataReader = sqlCommand.ExecuteReader();

                    while (dataReader.Read())
                    {
                        currentEvents.Add(new Event(
                            dataReader["photo"].ToString(),
                            dataReader["title"].ToString(),
                            dataReader["description"].ToString(),
                            (DateTime)dataReader["start_date"],
                            (DateTime)dataReader["end_date"],
                            dataReader["location"].ToString(),
                            DefaultHostCompanyId,
                            new List<Company>()
                        )
                        {
                            Id = (int)dataReader["event_id"]
                        });
                    }
                }
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception.Message);
                throw;
            }

            return currentEvents;
        }

        public ObservableCollection<Event> getPastEventsFromRepo(int loggedInUser)
        {
            var pastEvents = new ObservableCollection<Event>();

            using (SqlConnection databaseConnection = DbConnectionHelper.GetConnection())
            {
                databaseConnection.Open();

                string sqlQuery = "SELECT * FROM events WHERE host_company_id = @HostId and end_date < @TodaysDate";

                SqlCommand sqlCommand = new SqlCommand(sqlQuery, databaseConnection);

                sqlCommand.Parameters.AddWithValue("@HostId", loggedInUser);
                sqlCommand.Parameters.AddWithValue("@TodaysDate", DateTime.Now.Date);

                SqlDataReader dataReader = sqlCommand.ExecuteReader();

                while (dataReader.Read())
                {
                    pastEvents.Add(new Event(
                        dataReader["photo"].ToString(),
                        dataReader["title"].ToString(),
                        dataReader["description"].ToString(),
                        (DateTime)dataReader["start_date"],
                        (DateTime)dataReader["end_date"],
                        dataReader["location"].ToString(),
                        DefaultHostCompanyId,
                        new List<Company>()
                    )
                    {
                        Id = (int)dataReader["event_id"]
                    });
                }
            }

            return pastEvents;
        }

        public void UpdateEventToRepo(int eventIdToBeUpdated, string newEventPhoto, string newEventTitle, string newEventDescription, DateTime newEventStartDate, DateTime newEventEndDate, string newEventLocation)
        {
            using (SqlConnection databaseConnection = DbConnectionHelper.GetConnection())
            {
                databaseConnection.Open();

                string sqlQuery = @"UPDATE events SET 
                                photo=@Photo,
                                title=@Title,
                                description=@Description,
                                start_date=@StartDate,
                                end_date=@EndDate,
                                location=@Location,
                                posted_at=@PostedAt
                                WHERE event_id=@Id";

                SqlCommand sqlCommand = new SqlCommand(sqlQuery, databaseConnection);

                sqlCommand.Parameters.AddWithValue("@Photo", newEventPhoto ?? (object)DBNull.Value);
                sqlCommand.Parameters.AddWithValue("@Title", newEventTitle);
                sqlCommand.Parameters.AddWithValue("@Description", newEventDescription ?? (object)DBNull.Value);
                sqlCommand.Parameters.AddWithValue("@StartDate", newEventStartDate);
                sqlCommand.Parameters.AddWithValue("@EndDate", newEventEndDate);
                sqlCommand.Parameters.AddWithValue("@Location", newEventLocation);
                sqlCommand.Parameters.AddWithValue("@PostedAt", DateTime.Now);
                sqlCommand.Parameters.AddWithValue("@Id", eventIdToBeUpdated);

                sqlCommand.ExecuteNonQuery();
            }
        }
    }
}