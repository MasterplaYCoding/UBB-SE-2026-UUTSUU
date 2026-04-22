// File: OurApp.Tests/Helpers/TestDbSeeder.cs

using Microsoft.Data.SqlClient;
using OurApp.Core.Database;

namespace OurApp.Tests.Helpers
{
    internal static class TestDbSeeder
    {
        public const int CompanyId = 90001;
        public const int JobId = 90101;

        // placeholder - only exists to satisfy circular FK, tests never touch these
        public const int PlaceholderApplicantId = 90300;
        public const int PlaceholderUserId = 90200;

        // test applicant and user - used by tests
        public const int ApplicantId = 90301;
        public const int UserId = 90201;

        public static void Seed()
        {
            using (var conn = DbConnectionHelper.GetConnection())
            {
                conn.Open();

                Execute(conn, @"
                    IF NOT EXISTS (SELECT 1 FROM companies WHERE company_id = 90001)
                    INSERT INTO companies (company_id, company_name, logo_picture_url)
                    VALUES (90001, 'TestCompany', 'logo.png')");

                Execute(conn, @"
                    IF NOT EXISTS (SELECT 1 FROM jobs WHERE job_id = 90101)
                    INSERT INTO jobs
                        (job_id, company_id, job_title, industry_field, job_type,
                         experience_level, job_description, job_location, available_positions)
                    VALUES
                        (90101, 90001, 'Test Job', 'IT', 'Full-time',
                         'Entry Level', 'Test job description', 'Remote', 1)");

                // placeholder applicant with PlaceholderUserId
                Execute(conn, @"
                    IF NOT EXISTS (SELECT 1 FROM applicants WHERE applicant_id = 90300)
                    INSERT INTO applicants
                        (applicant_id, job_id, application_status, applied_at, user_id)
                    VALUES (90300, 90101, 'OnHold', GETUTCDATE(), 90200)");

                // placeholder user referencing placeholder applicant
                Execute(conn, @"
                    IF NOT EXISTS (SELECT 1 FROM users WHERE user_id = 90200)
                    INSERT INTO users (user_id, name, email)
                    VALUES (90200, 'Placeholder User', 'placeholder@test.com')");

                // test applicant with TestUserId
                Execute(conn, @"
                    IF NOT EXISTS (SELECT 1 FROM applicants WHERE applicant_id = 90301)
                    INSERT INTO applicants
                        (applicant_id, job_id, application_status, applied_at, user_id)
                    VALUES (90301, 90101, 'OnHold', GETUTCDATE(), 90201)");

                // test user referencing test applicant
                Execute(conn, @"
                    IF NOT EXISTS (SELECT 1 FROM users WHERE user_id = 90201)
                    INSERT INTO users (user_id, name, email)
                    VALUES (90201, 'Test User', 'test@test.com')");
            }
        }

        public static void Clean()
        {
            using (var conn = DbConnectionHelper.GetConnection())
            {
                conn.Open();

                // delete applicants first to break circular FK
                Execute(conn, "DELETE FROM applicants WHERE applicant_id >= 90300");
                Execute(conn, "DELETE FROM users      WHERE user_id      >= 90200");
                Execute(conn, "DELETE FROM jobs        WHERE job_id       = 90101");
                Execute(conn, "DELETE FROM companies   WHERE company_id   = 90001");
            }
        }

        private static void Execute(SqlConnection conn, string sql)
        {
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }
}