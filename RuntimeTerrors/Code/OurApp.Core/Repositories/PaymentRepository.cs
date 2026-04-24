using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using OurApp.Core.Database;
using OurApp.Core.Models;

namespace OurApp.Core.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        // change it to your own
        // private readonly string _connectionString =
        // "Data Source=Aron\\SQLEXPRESS;Initial Catalog=iss_project;Integrated Security=True;Trust Server Certificate=True";
        private const string UpdateJobPaymentSqlQuery = "UPDATE jobs SET amount_payed = @amount WHERE job_id = @jobId";
        private const string GetPaidJobsSqlQuery = @"
                SELECT c.company_name, j.job_title, j.amount_payed 
                FROM jobs j
                INNER JOIN companies c ON j.company_id = c.company_id
                WHERE j.job_type = @jobType AND j.experience_level = @expLevel";
        private const string GetCompaniesToNotifySqlQuery = @"
                SELECT DISTINCT c.email 
                FROM companies c
                INNER JOIN jobs j ON c.company_id = j.company_id
                WHERE c.email IS NOT NULL 
                  AND c.email != ''
                  AND j.job_id != @jobId
                  AND j.job_type = (SELECT job_type FROM jobs WHERE job_id = @jobId)
                  AND j.experience_level = (SELECT experience_level FROM jobs WHERE job_id = @jobId)
                  AND (j.amount_payed IS NULL OR j.amount_payed < @amount)";

        private const string AmountParameterName = "@amount";
        private const string JobIdParameterName = "@jobId";
        private const string JobTypeParameterName = "@jobType";
        private const string ExperienceLevelParameterName = "@expLevel";

        private const int EmptyRowsAffectedCount = 0;
        private const string JobNotFoundErrorMessage = "Job ID not found. Payment not applied to database.";

        private const int CompanyNameColumnIndex = 0;
        private const int JobTitleColumnIndex = 1;
        private const int AmountPayedColumnIndex = 2;
        private const int DefaultAmountPayedValue = 0;
        private const int EmailColumnIndex = 0;

        public void UpdateJobPayment(int jobId, int paymentAmount)
        {
            using (SqlConnection databaseConnection = DbConnectionHelper.GetConnection())
            {
                using (SqlCommand sqlCommand = new SqlCommand(UpdateJobPaymentSqlQuery, databaseConnection))
                {
                    sqlCommand.Parameters.AddWithValue(AmountParameterName, paymentAmount);
                    sqlCommand.Parameters.AddWithValue(JobIdParameterName, jobId);
                    databaseConnection.Open();
                    int affectedRowsCount = sqlCommand.ExecuteNonQuery();
                    if (affectedRowsCount == EmptyRowsAffectedCount)
                    {
                        throw new Exception(JobNotFoundErrorMessage);
                    }
                }
            }
        }

        public List<JobPaymentInfo> GetPaidJobs(string jobType, string experienceLevel)
        {
            var paidJobsList = new List<JobPaymentInfo>();

            using (SqlConnection databaseConnection = DbConnectionHelper.GetConnection())
            {
                using (SqlCommand sqlCommand = new SqlCommand(GetPaidJobsSqlQuery, databaseConnection))
                {
                    sqlCommand.Parameters.AddWithValue(JobTypeParameterName, jobType);
                    sqlCommand.Parameters.AddWithValue(ExperienceLevelParameterName, experienceLevel);

                    databaseConnection.Open();
                    using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
                    {
                        while (sqlDataReader.Read())
                        {
                            paidJobsList.Add(new JobPaymentInfo
                            {
                                CompanyName = sqlDataReader.GetString(CompanyNameColumnIndex),
                                JobTitle = sqlDataReader.GetString(JobTitleColumnIndex),
                                AmountPayed = sqlDataReader.IsDBNull(AmountPayedColumnIndex) ? DefaultAmountPayedValue : sqlDataReader.GetInt32(AmountPayedColumnIndex)
                            });
                        }
                    }
                }
            }
            return paidJobsList;
        }

        public List<string> GetCompaniesToNotify(int currentJobId, int newPaymentAmount)
        {
            var emailsToNotifyList = new List<string>();

            using (SqlConnection databaseConnection = DbConnectionHelper.GetConnection())
            {
                using (SqlCommand sqlCommand = new SqlCommand(GetCompaniesToNotifySqlQuery, databaseConnection))
                {
                    sqlCommand.Parameters.AddWithValue(JobIdParameterName, currentJobId);
                    sqlCommand.Parameters.AddWithValue(AmountParameterName, newPaymentAmount);

                    databaseConnection.Open();
                    using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
                    {
                        while (sqlDataReader.Read())
                        {
                            emailsToNotifyList.Add(sqlDataReader.GetString(EmailColumnIndex));
                        }
                    }
                }
            }
            return emailsToNotifyList;
        }
    }
}