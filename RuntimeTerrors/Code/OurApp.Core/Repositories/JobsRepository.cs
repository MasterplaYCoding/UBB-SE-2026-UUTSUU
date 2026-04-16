using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using OurApp.Core.Models;
using OurApp.Core.Database;

namespace OurApp.Core.Repositories
{
    public class JobsRepository : IJobsRepository
    {
        public IEnumerable<JobPosting> GetAllJobs()
        {
            var list = new List<JobPosting>();
            using (var conn = DbConnectionHelper.GetConnection())
            {
                conn.Open();
                string sql = @"
                    SELECT j.job_id, j.job_title, j.industry_field, j.job_type, j.experience_level, 
                           j.start_date, j.end_date, j.job_description, j.job_location, j.available_positions,
                           j.posted_at, j.salary, j.amount_payed, j.deadline, j.photo,
                           c.company_id, c.company_name
                    FROM jobs j
                    LEFT JOIN companies c ON j.company_id = c.company_id";

                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var job = new JobPosting
                        {
                            JobId = reader.GetInt32(reader.GetOrdinal("job_id")),
                            JobTitle = reader.GetString(reader.GetOrdinal("job_title")),
                            IndustryField = reader.GetString(reader.GetOrdinal("industry_field")),
                            JobType = reader.GetString(reader.GetOrdinal("job_type")),
                            ExperienceLevel = reader.GetString(reader.GetOrdinal("experience_level"))
                        };

                        if (!reader.IsDBNull(reader.GetOrdinal("company_id")))
                        {
                            job.Company = new Company(
                                reader.GetString(reader.GetOrdinal("company_name")), // name
                                "Company description",                               // aboutus
                                "pfp.png",                                           // pfpUrl
                                "logo.png",                                          // logoUrl
                                "location here",                                     // location
                                "company@gmail.com",                                 // email
                                reader.GetInt32(reader.GetOrdinal("company_id"))     // companyId (optional param)
);
                        }

                        list.Add(job);
                    }
                }
            }

            using (var conn = DbConnectionHelper.GetConnection())
            {
                conn.Open();
                string sql = @"
                    SELECT js.job_id, js.skill_id, js.required_percentage, s.skill_name
                    FROM job_skills js
                    JOIN skills s ON js.skill_id = s.skill_id";

                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int jobId = reader.GetInt32(reader.GetOrdinal("job_id"));
                        var targetJob = list.Find(j => j.JobId == jobId);
                        if (targetJob != null)
                        {
                            var js = new JobSkill
                            {
                                Job = targetJob,
                                RequiredPercentage = reader.GetInt32(reader.GetOrdinal("required_percentage")),
                                Skill = new Skill
                                {
                                    SkillId = reader.GetInt32(reader.GetOrdinal("skill_id")),
                                    SkillName = reader.GetString(reader.GetOrdinal("skill_name"))
                                }
                            };
                            targetJob.JobSkills.Add(js);
                        }
                    }
                }
            }

            return list;
        }

        public IReadOnlyList<Skill> GetAllSkills()
        {
            var list = new List<Skill>();
            using var conn = DbConnectionHelper.GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(
                "SELECT skill_id, skill_name FROM skills",
                conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Skill
                {
                    SkillId = reader.GetInt32(reader.GetOrdinal("skill_id")),
                    SkillName = reader.GetString(reader.GetOrdinal("skill_name"))
                });
            }

            return list;
        }

        public int AddJob(JobPosting job, int companyId, IReadOnlyList<(int SkillId, int RequiredPercentage)> skillLinks)
        {
            if (job == null)
            {
                throw new ArgumentNullException(nameof(job));
            }

            using var conn = DbConnectionHelper.GetConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();

            using (var nextCmd = new SqlCommand(
                       "SELECT COALESCE(MAX(job_id), 0) + 1 FROM jobs WITH (UPDLOCK, HOLDLOCK)",
                       conn,
                       tx))
            {
                var nextIdObj = nextCmd.ExecuteScalar();
                int nextId = Convert.ToInt32(nextIdObj);

                using var insert = new SqlCommand(
                    @"INSERT INTO jobs (
                        job_id, company_id, photo, job_title, industry_field, job_type, experience_level,
                        start_date, end_date, job_description, job_location, available_positions,
                        posted_at, salary, amount_payed, deadline)
                      VALUES (
                        @jobId, @companyId, @photo, @jobTitle, @industryField, @jobType, @experienceLevel,
                        @startDate, @endDate, @jobDescription, @jobLocation, @availablePositions,
                        @postedAt, @salary, @amountPayed, @deadline)",
                    conn,
                    tx);

                insert.Parameters.AddWithValue("@jobId", nextId);
                insert.Parameters.AddWithValue("@companyId", companyId);
                insert.Parameters.AddWithValue("@photo", string.IsNullOrWhiteSpace(job.Photo) ? (object)DBNull.Value : job.Photo);
                insert.Parameters.AddWithValue("@jobTitle", job.JobTitle);
                insert.Parameters.AddWithValue("@industryField", job.IndustryField);
                insert.Parameters.AddWithValue("@jobType", job.JobType);
                insert.Parameters.AddWithValue("@experienceLevel", job.ExperienceLevel);
                insert.Parameters.AddWithValue("@startDate", job.StartDate.HasValue ? (object)job.StartDate.Value.Date : DBNull.Value);
                insert.Parameters.AddWithValue("@endDate", job.EndDate.HasValue ? (object)job.EndDate.Value.Date : DBNull.Value);
                insert.Parameters.AddWithValue("@jobDescription", job.JobDescription);
                insert.Parameters.AddWithValue("@jobLocation", job.JobLocation);
                insert.Parameters.AddWithValue("@availablePositions", job.AvailablePositions);
                insert.Parameters.AddWithValue("@postedAt", job.PostedAt.HasValue ? (object)job.PostedAt.Value : (object)DateTime.Now);
                insert.Parameters.AddWithValue("@salary", job.Salary.HasValue ? (object)job.Salary.Value : DBNull.Value);
                insert.Parameters.AddWithValue("@amountPayed", job.AmountPayed.HasValue ? (object)job.AmountPayed.Value : (object)0);
                insert.Parameters.AddWithValue("@deadline", job.Deadline.HasValue ? (object)job.Deadline.Value.Date : DBNull.Value);

                insert.ExecuteNonQuery();

                if (skillLinks != null)
                {
                    foreach (var (skillId, pct) in skillLinks)
                    {
                        if (pct < 1 || pct > 100)
                        {
                            continue;
                        }

                        using var jsCmd = new SqlCommand(
                            @"INSERT INTO job_skills (job_id, skill_id, required_percentage)
                              VALUES (@jobId, @skillId, @pct)",
                            conn,
                            tx);
                        jsCmd.Parameters.AddWithValue("@jobId", nextId);
                        jsCmd.Parameters.AddWithValue("@skillId", skillId);
                        jsCmd.Parameters.AddWithValue("@pct", pct);
                        jsCmd.ExecuteNonQuery();
                    }
                }

                using var checkCmd = new SqlCommand(@"
                    SELECT COUNT(*) 
                    FROM jobs 
                    WHERE company_id = @CompanyId",
                    conn, tx);

                checkCmd.Parameters.AddWithValue("@CompanyId", companyId);
                int existingJobs = (int)checkCmd.ExecuteScalar();

                if (existingJobs == 1) 
                {
                    using var updateCmd = new SqlCommand(@"
                        UPDATE companies
                        SET posted_jobs_count = 1
                        WHERE company_id = @CompanyId",
                        conn, tx);

                    updateCmd.Parameters.AddWithValue("@CompanyId", companyId);
                    updateCmd.ExecuteNonQuery();
                }
                else
                {
                    using var updateCmd = new SqlCommand(@"
                        UPDATE companies
                        SET posted_jobs_count = posted_jobs_count + 1
                        WHERE company_id = @CompanyId",
                        conn, tx);

                    updateCmd.Parameters.AddWithValue("@CompanyId", companyId);
                    updateCmd.ExecuteNonQuery();
                }

                tx.Commit();
                return nextId;
            }
        }
    }
}