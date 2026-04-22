using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using OurApp.Core.Models;
using OurApp.Core.Database;

namespace OurApp.Core.Repositories
{
    public class ApplicantRepository : IApplicantRepository
    {
        public Applicant GetApplicantById(int applicantId)
        {
            Applicant applicant = null;
            using (var conn = DbConnectionHelper.GetConnection())
            {
                conn.Open();
                string sql = @"
                    SELECT a.applicant_id, a.app_test_grade, a.cv_grade,
                           a.company_test_grade, a.interview_grade, a.application_status, a.applied_at,
                           a.job_id, a.recommended_from_company_id, a.user_id,
                           u.name, u.email, u.cv_xml
                    FROM applicants a
                    LEFT JOIN users u ON a.user_id = u.user_id
                    WHERE a.applicant_id = @id";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", applicantId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            applicant = MapReaderToApplicant(reader);
                        }
                    }
                }
            }
            return applicant;
        }

        public IEnumerable<Applicant> GetApplicantsByCompany(int companyId)
        {
            var list = new List<Applicant>();

            using (var conn = DbConnectionHelper.GetConnection())
            {
                conn.Open();

                string sql = @"
            SELECT a.applicant_id, a.app_test_grade, a.cv_grade,
                   a.company_test_grade, a.interview_grade, a.application_status, a.applied_at,
                   a.job_id, a.recommended_from_company_id, a.user_id,
                   u.name, u.email, u.cv_xml
            FROM applicants a
            INNER JOIN jobs j ON a.job_id = j.job_id
            LEFT JOIN users u ON a.user_id = u.user_id
            WHERE j.company_id = @companyId";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@companyId", companyId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var applicant = MapReaderToApplicant(reader);

                            // Attach job (minimal info, same as your pattern)
                            if (!reader.IsDBNull(reader.GetOrdinal("job_id")))
                            {
                                applicant.Job = new JobPosting
                                {
                                    JobId = reader.GetInt32(reader.GetOrdinal("job_id"))
                                };
                            }

                            list.Add(applicant);
                        }
                    }
                }
            }

            return list;
        }

        public IEnumerable<Applicant> GetApplicantsByJob(JobPosting job)
        {
            var list = new List<Applicant>();
            if (job == null) return list;

            using (var conn = DbConnectionHelper.GetConnection())
            {
                conn.Open();
                string sql = @"
                    SELECT a.applicant_id, a.app_test_grade, a.cv_grade,
                           a.company_test_grade, a.interview_grade, a.application_status, a.applied_at,
                           a.job_id, a.recommended_from_company_id, a.user_id,
                           u.name, u.email, u.cv_xml
                    FROM applicants a
                    LEFT JOIN users u ON a.user_id = u.user_id
                    WHERE a.job_id = @jobId";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@jobId", job.JobId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var applicant = MapReaderToApplicant(reader);
                            applicant.Job = job;
                            list.Add(applicant);
                        }
                    }
                }
            }
            return list;
        }

        public void AddApplicant(Applicant applicant)
        {
            using (var conn = DbConnectionHelper.GetConnection())
            {
                conn.Open();
                
                // If the user is new, ideally we would insert into `users` first.
                // Assuming `user_id` exists for this demonstration or is handled.
                
                string sql = @"
                    INSERT INTO applicants (applicant_id, job_id, app_test_grade, cv_grade,
                                          company_test_grade, interview_grade, application_status,
                                          recommended_from_company_id, applied_at, user_id)
                    VALUES (@appId, @jobId, @appGrade, @cvGrade, @compGrade, @intGrade, @status, @recId, @applied, @userId)";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@appId", applicant.ApplicantId);
                    cmd.Parameters.AddWithValue("@jobId", applicant.Job?.JobId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@appGrade", applicant.AppTestGrade ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@cvGrade", applicant.CvGrade ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@compGrade", applicant.CompanyTestGrade ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@intGrade", applicant.InterviewGrade ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@status", string.IsNullOrEmpty(applicant.ApplicationStatus) ? DBNull.Value : applicant.ApplicationStatus);
                    cmd.Parameters.AddWithValue("@recId", applicant.RecommendedFromCompany?.CompanyId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@applied", applicant.AppliedAt);
                    cmd.Parameters.AddWithValue("@userId", applicant.User.Id);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateApplicant(Applicant applicant)
        {
            using (var conn = DbConnectionHelper.GetConnection())
            {
                conn.Open();
                string sql = @"
                    UPDATE applicants
                    SET app_test_grade = @appGrade,
                        cv_grade = @cvGrade,
                        company_test_grade = @compGrade,
                        interview_grade = @intGrade,
                        application_status = @status
                    WHERE applicant_id = @id";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@appGrade", applicant.AppTestGrade ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@cvGrade", applicant.CvGrade ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@compGrade", applicant.CompanyTestGrade ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@intGrade", applicant.InterviewGrade ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@status", string.IsNullOrEmpty(applicant.ApplicationStatus) ? DBNull.Value : applicant.ApplicationStatus);
                    cmd.Parameters.AddWithValue("@id", applicant.ApplicantId);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void RemoveApplicant(int applicantId)
        {
            using (var conn = DbConnectionHelper.GetConnection())
            {
                conn.Open();
                string sql = "DELETE FROM applicants WHERE applicant_id = @id";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", applicantId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private Applicant MapReaderToApplicant(SqlDataReader reader)
        {
            var applicant = new Applicant
            {
                ApplicantId = reader.GetInt32(reader.GetOrdinal("applicant_id")),
                AppTestGrade = reader.IsDBNull(reader.GetOrdinal("app_test_grade")) ? null : reader.GetDecimal(reader.GetOrdinal("app_test_grade")),
                CvGrade = reader.IsDBNull(reader.GetOrdinal("cv_grade")) ? null : reader.GetDecimal(reader.GetOrdinal("cv_grade")),
                CompanyTestGrade = reader.IsDBNull(reader.GetOrdinal("company_test_grade")) ? null : reader.GetDecimal(reader.GetOrdinal("company_test_grade")),
                InterviewGrade = reader.IsDBNull(reader.GetOrdinal("interview_grade")) ? null : reader.GetDecimal(reader.GetOrdinal("interview_grade")),
                ApplicationStatus = reader.IsDBNull(reader.GetOrdinal("application_status")) ? null : reader.GetString(reader.GetOrdinal("application_status")),
                AppliedAt = reader.GetDateTime(reader.GetOrdinal("applied_at"))
            };

            // Reconstruct nested optional Job
            if (!reader.IsDBNull(reader.GetOrdinal("job_id")))
            {
                applicant.Job = new JobPosting { JobId = reader.GetInt32(reader.GetOrdinal("job_id")) };
            }

            // Reconstruct nested User
            if (!reader.IsDBNull(reader.GetOrdinal("user_id")))
            {
                var cvXmlOrdinal = reader.GetOrdinal("cv_xml");
                applicant.User = new User(
                    reader.GetInt32(reader.GetOrdinal("user_id")),
                    reader.IsDBNull(reader.GetOrdinal("name")) ? "Unknown" : reader.GetString(reader.GetOrdinal("name")),
                    reader.IsDBNull(reader.GetOrdinal("email")) ? "" : reader.GetString(reader.GetOrdinal("email")),
                    reader.IsDBNull(cvXmlOrdinal) ? null : reader.GetString(cvXmlOrdinal)
                );
            }

            if (!reader.IsDBNull(reader.GetOrdinal("recommended_from_company_id")))
            {
                applicant.RecommendedFromCompany = new Company { CompanyId = reader.GetInt32(reader.GetOrdinal("recommended_from_company_id")) };
            }

            return applicant;
        }
    }
}
