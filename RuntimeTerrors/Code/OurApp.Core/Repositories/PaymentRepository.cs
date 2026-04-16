using Microsoft.Data.SqlClient;
using OurApp.Core.Database;
using OurApp.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OurApp.Core.Repositories
{
    public class PaymentRepository
    {
        //change it to your own 
        // private readonly string _connectionString =
        //     "Data Source=Aron\\SQLEXPRESS;Initial Catalog=iss_project;Integrated Security=True;Trust Server Certificate=True";

        public void UpdateJobPayment(int jobId, int paymentAmount)
        {
            string query = "UPDATE jobs SET amount_payed = @amount WHERE job_id = @jobId";

            using (SqlConnection connection = DbConnectionHelper.GetConnection())
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@amount", paymentAmount);
                    command.Parameters.AddWithValue("@jobId", jobId);
                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        throw new Exception("Job ID not found. Payment not applied to database.");
                    }
                }
            }
        }
        public List<JobPaymentInfo> GetPaidJobs(string jobType, string experienceLevel)
        {
            var results = new List<JobPaymentInfo>();

            string query = @"
                SELECT c.company_name, j.job_title, j.amount_payed 
                FROM jobs j
                INNER JOIN companies c ON j.company_id = c.company_id
                WHERE j.job_type = @jobType AND j.experience_level = @expLevel";

            using (SqlConnection connection = DbConnectionHelper.GetConnection())
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@jobType", jobType);
                    command.Parameters.AddWithValue("@expLevel", experienceLevel);

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            results.Add(new JobPaymentInfo
                            {
                                CompanyName = reader.GetString(0),
                                JobTitle = reader.GetString(1),
                                AmountPayed = reader.IsDBNull(2) ? 0 : reader.GetInt32(2)
                            });
                        }
                    }
                }
            }
            return results;
        }
        public List<string> GetCompaniesToNotify(int currentJobId, int newPaymentAmount)
        {
            var emails = new List<string>();

            string query = @"
                SELECT DISTINCT c.email 
                FROM companies c
                INNER JOIN jobs j ON c.company_id = j.company_id
                WHERE c.email IS NOT NULL 
                  AND c.email != ''
                  AND j.job_id != @jobId
                  AND j.job_type = (SELECT job_type FROM jobs WHERE job_id = @jobId)
                  AND j.experience_level = (SELECT experience_level FROM jobs WHERE job_id = @jobId)
                  AND (j.amount_payed IS NULL OR j.amount_payed < @amount)";

            using (SqlConnection connection = DbConnectionHelper.GetConnection())
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@jobId", currentJobId);
                    command.Parameters.AddWithValue("@amount", newPaymentAmount);

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            emails.Add(reader.GetString(0));
                        }
                    }
                }
            }
            return emails;
        }
    }
}
