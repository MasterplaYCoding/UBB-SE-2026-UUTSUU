using Microsoft.Data.SqlClient;
using OurApp.Core.Database;

namespace OurApp.Tests.Helpers
{
    internal static class TestDbSeeder
    {
        public const int CompanyId = 90001;
        public const int CollaboratorCompanyId = 90002;
        public const int JobId = 90101;
        public const int PlaceholderApplicantId = 90300;
        public const int PlaceholderUserId = 90200;
        public const int ApplicantId = 90301;
        public const int UserId = 90201;

        public static void Seed()
        {
            using (var conn = DbConnectionHelper.GetConnection())
            {
                conn.Open();

                Execute(conn, @"
                    IF NOT EXISTS (SELECT 1 FROM companies WHERE company_id = 90001)
                    INSERT INTO companies (company_id, company_name, logo_picture_url, collaborators_count, posted_jobs_count)
                    VALUES (90001, 'TestCompany', 'logo.png', 0, 0)");

                Execute(conn, @"
                    IF NOT EXISTS (SELECT 1 FROM companies WHERE company_id = 90002)
                    INSERT INTO companies (company_id, company_name, logo_picture_url, collaborators_count, posted_jobs_count)
                    VALUES (90002, 'TestCollaboratorCompany', 'logo2.png', 0, 0)");

                Execute(conn, @"
                    IF NOT EXISTS (SELECT 1 FROM jobs WHERE job_id = 90101)
                    INSERT INTO jobs
                        (job_id, company_id, job_title, industry_field, job_type,
                         experience_level, job_description, job_location, available_positions)
                    VALUES
                        (90101, 90001, 'Test Job', 'IT', 'Full-time',
                         'Entry Level', 'Test job description', 'Remote', 1)");

                Execute(conn, @"
                    IF NOT EXISTS (SELECT 1 FROM applicants WHERE applicant_id = 90300)
                    INSERT INTO applicants
                        (applicant_id, job_id, application_status, applied_at, user_id)
                    VALUES (90300, 90101, 'OnHold', GETUTCDATE(), 90200)");

                Execute(conn, @"
                    IF NOT EXISTS (SELECT 1 FROM users WHERE user_id = 90200)
                    INSERT INTO users (user_id, name, email, cv_xml)
                    VALUES (90200, 'Placeholder User', 'placeholder@test.com', '<CV><Name>Test</Name></CV>')");

                Execute(conn, @"
                    IF NOT EXISTS (SELECT 1 FROM applicants WHERE applicant_id = 90301)
                    INSERT INTO applicants
                        (applicant_id, job_id, application_status, applied_at, user_id)
                    VALUES (90301, 90101, 'OnHold', GETUTCDATE(), 90201)");

                Execute(conn, @"
                    IF NOT EXISTS (SELECT 1 FROM users WHERE user_id = 90201)
                    INSERT INTO users (user_id, name, email)
                    VALUES (90201, 'Test User', 'test@test.com')");

                Execute(conn, @"
                    IF NOT EXISTS (SELECT 1 FROM events WHERE event_id = 90401)
                    INSERT INTO events
                        (event_id, title, description, start_date, end_date,
                         location, host_company_id, posted_at)
                    VALUES
                        (90401, 'Test Event', 'Test event description',
                         '2027-01-01', '2027-01-02',
                         'Cluj-Napoca', 90001, GETUTCDATE())");
            }
        }

        public static void Clean()
        {
            using (var conn = DbConnectionHelper.GetConnection())
            {
                conn.Open();

                Execute(conn, "DELETE FROM collaborators WHERE event_id  = 90401");
                Execute(conn, "DELETE FROM events        WHERE event_id  = 90401");
                Execute(conn, "DELETE FROM applicants    WHERE applicant_id >= 90300");
                Execute(conn, "DELETE FROM users         WHERE user_id      >= 90200");
                Execute(conn, "DELETE FROM jobs          WHERE job_id     = 90101");
                Execute(conn, "DELETE FROM companies     WHERE company_id IN (90001, 90002)");
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