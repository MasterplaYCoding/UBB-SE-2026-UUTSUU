using Microsoft.Data.SqlClient;
using OurApp.Core.Database;

namespace OurApp.Tests.Helpers
{
    internal static class TestDbSeeder
    {
        public const int CompanyId = 90001;
        public const int JobId = 90101;
        public const int UserId = 90201;
        public const int ApplicantId = 90301;

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

                Execute(conn, @"
                    IF NOT EXISTS (SELECT 1 FROM applicants WHERE applicant_id = 90301)
                    INSERT INTO applicants
                        (applicant_id, job_id, application_status, applied_at, user_id)
                    VALUES (90301, 90101, 'OnHold', GETUTCDATE(), 90201)");

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

                Execute(conn, "DELETE FROM users      WHERE user_id    = 90201");
                Execute(conn, "DELETE FROM applicants WHERE job_id     = 90101");
                Execute(conn, "DELETE FROM jobs        WHERE job_id    = 90101");
                Execute(conn, "DELETE FROM companies   WHERE company_id = 90001");
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
