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

        /// <summary>
        /// Function that returns the event id with the maximum value out of the database
        /// </summary>
        /// <returns></returns>
        public int GetMaxEventId()
        {
            using (SqlConnection sqlConnection = DbConnectionHelper.GetConnection())
            {
                sqlConnection.Open();

                string query = "SELECT MAX(event_id) FROM events";

                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);

                object result = sqlCommand.ExecuteScalar();

                if (result == DBNull.Value || result == null)
                    return 0;

                return Convert.ToInt32(result);
            }
        }

        public void AddEventToRepo(Event eventToBeAdded)
        {
            using var conn = DbConnectionHelper.GetConnection();
            conn.Open();

            using var tx = conn.BeginTransaction();

            try
            {
                int nextId;
                using (var idCmd = new SqlCommand(
                    "SELECT COALESCE(MAX(event_id), 0) + 1 FROM events WITH (UPDLOCK, HOLDLOCK)",
                    conn, tx))
                {
                    nextId = (int)idCmd.ExecuteScalar();
                }

                using (var insertEventCmd = new SqlCommand(@"
                    INSERT INTO events 
                    (event_id, photo, title, description, start_date, end_date, location, host_company_id, posted_at)
                    VALUES (@Id, @Photo, @Title, @Description, @StartDate, @EndDate, @Location, @Host, @Now)",
                    conn, tx))
                {
                    insertEventCmd.Parameters.AddWithValue("@Id", nextId);
                    insertEventCmd.Parameters.AddWithValue("@Photo", (object?)eventToBeAdded.Photo ?? DBNull.Value);
                    insertEventCmd.Parameters.AddWithValue("@Title", eventToBeAdded.Title);
                    insertEventCmd.Parameters.AddWithValue("@Description", (object?)eventToBeAdded.Description ?? DBNull.Value);
                    insertEventCmd.Parameters.AddWithValue("@StartDate", eventToBeAdded.StartDate);
                    insertEventCmd.Parameters.AddWithValue("@EndDate", eventToBeAdded.EndDate);
                    insertEventCmd.Parameters.AddWithValue("@Location", eventToBeAdded.Location);
                    insertEventCmd.Parameters.AddWithValue("@Host", eventToBeAdded.HostID);
                    insertEventCmd.Parameters.AddWithValue("@Now", DateTime.Now);

                    insertEventCmd.ExecuteNonQuery();
                }

                eventToBeAdded.Id = nextId;

                if (eventToBeAdded.Collaborators != null)
                {
                    foreach (var collaborator in eventToBeAdded.Collaborators)
                    {
                        using var checkCmd = new SqlCommand(@"
                            SELECT COUNT(*) 
                            FROM event_collaborators 
                            WHERE company_id = @CompanyId",
                            conn, tx);

                        checkCmd.Parameters.AddWithValue("@CompanyId", collaborator.CompanyId);
                        int existingCount = (int)checkCmd.ExecuteScalar();

                        using var insertCollabCmd = new SqlCommand(@"
                            INSERT INTO event_collaborators (event_id, company_id)
                            VALUES (@EventId, @CompanyId)",
                            conn, tx);

                        insertCollabCmd.Parameters.AddWithValue("@EventId", nextId);
                        insertCollabCmd.Parameters.AddWithValue("@CompanyId", collaborator.CompanyId);
                        insertCollabCmd.ExecuteNonQuery();

                        if (existingCount == 0)
                        {
                            using var updateCmd = new SqlCommand(@"
                                UPDATE companies
                                SET collaborators_count = collaborators_count + 1
                                WHERE company_id = @CompanyId",
                                conn, tx);

                            updateCmd.Parameters.AddWithValue("@CompanyId", collaborator.CompanyId);
                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Function that removes an event from the database
        /// </summary>
        /// <param name="eventToBeRemoved"> the event selected to be removed </param>
        public void RemoveEventFromRepo(Event eventToBeRemoved)
        {
            using (SqlConnection conn = DbConnectionHelper.GetConnection())
            {
                conn.Open();

                string query = "DELETE FROM events WHERE event_id = @Id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", eventToBeRemoved.Id);

                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Function that returns a collection of all the current events, 
        /// whose ending date has not exceeded the current date
        /// </summary>
        /// <returns> ObservableCollection of current events </returns>
        public ObservableCollection<Event> getCurrentEventsFromRepo(int loggedInUser)
        {
            var currentEvents = new ObservableCollection<Event>();

            try
            {
                using (SqlConnection sqlConnection = DbConnectionHelper.GetConnection())
                {
                    sqlConnection.Open();

                    string queryToBeRun = "SELECT * FROM events WHERE host_company_id = @HostID and end_date >= @TodaysDate";

                    SqlCommand sqlCommand = new SqlCommand(queryToBeRun, sqlConnection);

                    sqlCommand.Parameters.AddWithValue("@HostID", loggedInUser);
                    sqlCommand.Parameters.AddWithValue("@TodaysDate", DateTime.Now.Date);

                    SqlDataReader reader = sqlCommand.ExecuteReader();

                    while (reader.Read())
                    {
                        currentEvents.Add(new Event(
                            reader["photo"].ToString(),
                            reader["title"].ToString(),
                            reader["description"].ToString(),
                            (DateTime)reader["start_date"],
                            (DateTime)reader["end_date"],
                            reader["location"].ToString(),
                            1,
                            new List<Company>()
                        )
                        {
                            Id = (int)reader["event_id"]
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                throw;
            }

            return currentEvents;
        }

        /// <summary>
        /// Function that returns a collection of all the past events, 
        /// whose ending date has exceeded the current date
        /// </summary>
        /// <returns> ObservableCollection of past events </returns>
        public ObservableCollection<Event> getPastEventsFromRepo(int loggedInUser)
        {
            var pastEvents = new ObservableCollection<Event>();

            using (SqlConnection sqlConnection = DbConnectionHelper.GetConnection())
            {
                sqlConnection.Open();

                string queryToBeRun = "SELECT * FROM events WHERE host_company_id = @HostID and end_date < @TodaysDate";

                SqlCommand sqlCommand = new SqlCommand(queryToBeRun, sqlConnection);


                sqlCommand.Parameters.AddWithValue("@HostID", loggedInUser);
                sqlCommand.Parameters.AddWithValue("@TodaysDate", DateTime.Now.Date);

                SqlDataReader reader = sqlCommand.ExecuteReader();

                while (reader.Read())
                {
                    pastEvents.Add(new Event(
                        reader["photo"].ToString(),
                        reader["title"].ToString(),
                        reader["description"].ToString(),
                        (DateTime)reader["start_date"],
                        (DateTime)reader["end_date"],
                        reader["location"].ToString(),
                        1, 
                        new List<Company>()
                    )
                    {
                        Id = (int)reader["event_id"]
                    });
                }
            }

            return pastEvents;
        
        }

        /// <summary>
        /// Function that updates the contents of an event.
        /// </summary>
        /// <param name="eventIdToBeUpdated"> id of the event that is updated </param>
        /// <param name="newEventPhoto"> the updated photo url </param>
        /// <param name="newEventTitle"> the updated title of the event </param>
        /// <param name="newEventDescription"> the updated description of the event </param>
        /// <param name="newEventStartDate"> the updated starting date of the event </param>
        /// <param name="newEventEndDate"> the updated ending date of the event </param>
        /// <param name="newEventLocation"> the updated location of the event </param>
        public void UpdateEventToRepo(int eventIdToBeUpdated, string newEventPhoto, string newEventTitle, string newEventDescription, DateTime newEventStartDate, DateTime newEventEndDate, string newEventLocation)
        {
            using (SqlConnection sqlConnection = DbConnectionHelper.GetConnection())
            {
                sqlConnection.Open();

                string queryToBeRun = @"UPDATE events SET 
                                photo=@Photo,
                                title=@Title,
                                description=@Description,
                                start_date=@StartDate,
                                end_date=@EndDate,
                                location=@Location,
                                posted_at=@PostedAt
                                WHERE event_id=@Id";

                SqlCommand sqlCommand = new SqlCommand(queryToBeRun, sqlConnection);

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
