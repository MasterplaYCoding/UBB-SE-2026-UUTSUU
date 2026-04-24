using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using OurApp.Core.Models;
using OurApp.Core.Database;

namespace OurApp.Core.Repositories
{
    public class ApplicantRepository : IApplicantRepository
    {
        private const string DefaultUserName = "Unknown";
        private const string DefaultUserEmail = "";

        public Applicant GetApplicantById(int applicantId)
        {
            Applicant applicant = null;
            using (var databaseConnection = DbConnectionHelper.GetConnection())
            {
                databaseConnection.Open();
                string sqlQuery = @"
                    SELECT a.applicant_id, a.app_test_grade, a.cv_grade,
                           a.company_test_grade, a.interview_grade, a.application_status, a.applied_at,
                           a.job_id, a.recommended_from_company_id, a.user_id,
                           u.name, u.email, u.cv_xml
                    FROM applicants a
                    LEFT JOIN users u ON a.user_id = u.user_id
                    WHERE a.applicant_id = @ApplicantId";

                using (var sqlCommand = new SqlCommand(sqlQuery, databaseConnection))
                {
                    sqlCommand.Parameters.AddWithValue("@ApplicantId", applicantId);
                    using (var dataReader = sqlCommand.ExecuteReader())
                    {
                        if (dataReader.Read())
                        {
                            applicant = MapReaderToApplicant(dataReader);
                        }
                    }
                }
            }
            return applicant;
        }

        public IEnumerable<Applicant> GetApplicantsByCompany(int companyId)
        {
            var applicantList = new List<Applicant>();

            using (var databaseConnection = DbConnectionHelper.GetConnection())
            {
                databaseConnection.Open();

                string sqlQuery = @"
                    SELECT a.applicant_id, a.app_test_grade, a.cv_grade,
                           a.company_test_grade, a.interview_grade, a.application_status, a.applied_at,
                           a.job_id, a.recommended_from_company_id, a.user_id,
                           u.name, u.email, u.cv_xml
                    FROM applicants a
                    INNER JOIN jobs j ON a.job_id = j.job_id
                    LEFT JOIN users u ON a.user_id = u.user_id
                    WHERE j.company_id = @CompanyId";

                using (var sqlCommand = new SqlCommand(sqlQuery, databaseConnection))
                {
                    sqlCommand.Parameters.AddWithValue("@CompanyId", companyId);

                    using (var dataReader = sqlCommand.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            var applicant = MapReaderToApplicant(dataReader);

                            if (!dataReader.IsDBNull(dataReader.GetOrdinal("job_id")))
                            {
                                applicant.Job = new JobPosting
                                {
                                    JobId = dataReader.GetInt32(dataReader.GetOrdinal("job_id"))
                                };
                            }

                            applicantList.Add(applicant);
                        }
                    }
                }
            }

            return applicantList;
        }

        public IEnumerable<Applicant> GetApplicantsByJob(JobPosting jobPosting)
        {
            var applicantList = new List<Applicant>();
            if (jobPosting == null)
            {
                return applicantList;
            }

            using (var databaseConnection = DbConnectionHelper.GetConnection())
            {
                databaseConnection.Open();
                string sqlQuery = @"
                    SELECT a.applicant_id, a.app_test_grade, a.cv_grade,
                           a.company_test_grade, a.interview_grade, a.application_status, a.applied_at,
                           a.job_id, a.recommended_from_company_id, a.user_id,
                           u.name, u.email, u.cv_xml
                    FROM applicants a
                    LEFT JOIN users u ON a.user_id = u.user_id
                    WHERE a.job_id = @JobId";

                using (var sqlCommand = new SqlCommand(sqlQuery, databaseConnection))
                {
                    sqlCommand.Parameters.AddWithValue("@JobId", jobPosting.JobId);
                    using (var dataReader = sqlCommand.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            var applicant = MapReaderToApplicant(dataReader);
                            applicant.Job = jobPosting;
                            applicantList.Add(applicant);
                        }
                    }
                }
            }
            return applicantList;
        }

        public void AddApplicant(Applicant applicant)
        {
            using (var databaseConnection = DbConnectionHelper.GetConnection())
            {
                databaseConnection.Open();

                string sqlQuery = @"
                    INSERT INTO applicants (applicant_id, job_id, app_test_grade, cv_grade,
                                          company_test_grade, interview_grade, application_status,
                                          recommended_from_company_id, applied_at, user_id)
                    VALUES (@ApplicantId, @JobId, @ApplicationTestGrade, @CurriculumVitaeGrade, @CompanyTestGrade, @InterviewGrade, @ApplicationStatus, @RecommendedFromCompanyId, @AppliedAt, @UserId)";

                using (var sqlCommand = new SqlCommand(sqlQuery, databaseConnection))
                {
                    sqlCommand.Parameters.AddWithValue("@ApplicantId", applicant.ApplicantId);
                    sqlCommand.Parameters.AddWithValue("@JobId", applicant.Job?.JobId ?? (object)DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@ApplicationTestGrade", applicant.AppTestGrade ?? (object)DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@CurriculumVitaeGrade", applicant.CvGrade ?? (object)DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@CompanyTestGrade", applicant.CompanyTestGrade ?? (object)DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@InterviewGrade", applicant.InterviewGrade ?? (object)DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@ApplicationStatus", string.IsNullOrEmpty(applicant.ApplicationStatus) ? DBNull.Value : applicant.ApplicationStatus);
                    sqlCommand.Parameters.AddWithValue("@RecommendedFromCompanyId", applicant.RecommendedFromCompany?.CompanyId ?? (object)DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@AppliedAt", applicant.AppliedAt);
                    sqlCommand.Parameters.AddWithValue("@UserId", applicant.User.Id);

                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        public void UpdateApplicant(Applicant applicant)
        {
            using (var databaseConnection = DbConnectionHelper.GetConnection())
            {
                databaseConnection.Open();
                string sqlQuery = @"
                    UPDATE applicants
                    SET app_test_grade = @ApplicationTestGrade,
                        cv_grade = @CurriculumVitaeGrade,
                        company_test_grade = @CompanyTestGrade,
                        interview_grade = @InterviewGrade,
                        application_status = @ApplicationStatus
                    WHERE applicant_id = @ApplicantId";

                using (var sqlCommand = new SqlCommand(sqlQuery, databaseConnection))
                {
                    sqlCommand.Parameters.AddWithValue("@ApplicationTestGrade", applicant.AppTestGrade ?? (object)DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@CurriculumVitaeGrade", applicant.CvGrade ?? (object)DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@CompanyTestGrade", applicant.CompanyTestGrade ?? (object)DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@InterviewGrade", applicant.InterviewGrade ?? (object)DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@ApplicationStatus", string.IsNullOrEmpty(applicant.ApplicationStatus) ? DBNull.Value : applicant.ApplicationStatus);
                    sqlCommand.Parameters.AddWithValue("@ApplicantId", applicant.ApplicantId);

                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        public void RemoveApplicant(int applicantId)
        {
            using (var databaseConnection = DbConnectionHelper.GetConnection())
            {
                databaseConnection.Open();
                string sqlQuery = "DELETE FROM applicants WHERE applicant_id = @ApplicantId";
                using (var sqlCommand = new SqlCommand(sqlQuery, databaseConnection))
                {
                    sqlCommand.Parameters.AddWithValue("@ApplicantId", applicantId);
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        private Applicant MapReaderToApplicant(SqlDataReader dataReader)
        {
            var applicant = new Applicant
            {
                ApplicantId = dataReader.GetInt32(dataReader.GetOrdinal("applicant_id")),
                AppTestGrade = dataReader.IsDBNull(dataReader.GetOrdinal("app_test_grade")) ? null : dataReader.GetDecimal(dataReader.GetOrdinal("app_test_grade")),
                CvGrade = dataReader.IsDBNull(dataReader.GetOrdinal("cv_grade")) ? null : dataReader.GetDecimal(dataReader.GetOrdinal("cv_grade")),
                CompanyTestGrade = dataReader.IsDBNull(dataReader.GetOrdinal("company_test_grade")) ? null : dataReader.GetDecimal(dataReader.GetOrdinal("company_test_grade")),
                InterviewGrade = dataReader.IsDBNull(dataReader.GetOrdinal("interview_grade")) ? null : dataReader.GetDecimal(dataReader.GetOrdinal("interview_grade")),
                ApplicationStatus = dataReader.IsDBNull(dataReader.GetOrdinal("application_status")) ? null : dataReader.GetString(dataReader.GetOrdinal("application_status")),
                AppliedAt = dataReader.GetDateTime(dataReader.GetOrdinal("applied_at"))
            };

            if (!dataReader.IsDBNull(dataReader.GetOrdinal("job_id")))
            {
                applicant.Job = new JobPosting { JobId = dataReader.GetInt32(dataReader.GetOrdinal("job_id")) };
            }

            if (!dataReader.IsDBNull(dataReader.GetOrdinal("user_id")))
            {
                var curriculumVitaeXmlOrdinal = dataReader.GetOrdinal("cv_xml");
                applicant.User = new User(
                    dataReader.GetInt32(dataReader.GetOrdinal("user_id")),
                    dataReader.IsDBNull(dataReader.GetOrdinal("name")) ? DefaultUserName : dataReader.GetString(dataReader.GetOrdinal("name")),
                    dataReader.IsDBNull(dataReader.GetOrdinal("email")) ? DefaultUserEmail : dataReader.GetString(dataReader.GetOrdinal("email")),
                    dataReader.IsDBNull(curriculumVitaeXmlOrdinal) ? null : dataReader.GetString(curriculumVitaeXmlOrdinal));
            }

            if (!dataReader.IsDBNull(dataReader.GetOrdinal("recommended_from_company_id")))
            {
                applicant.RecommendedFromCompany = new Company { CompanyId = dataReader.GetInt32(dataReader.GetOrdinal("recommended_from_company_id")) };
            }

            return applicant;
        }
    }
}