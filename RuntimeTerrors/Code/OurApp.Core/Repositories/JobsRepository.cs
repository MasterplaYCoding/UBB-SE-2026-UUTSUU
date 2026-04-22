using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using OurApp.Core.Models;
using OurApp.Core.Database;

namespace OurApp.Core.Repositories
{
    public class JobsRepository : IJobsRepository
    {
        private const string DefaultCompanyDescription = "Company description";
        private const string DefaultProfilePicture = "pfp.png";
        private const string DefaultLogoPicture = "logo.png";
        private const string DefaultLocation = "location here";
        private const string DefaultEmail = "company@gmail.com";
        private const int DefaultAmountPayed = 0;
        private const int MinimumSkillPercentage = 1;
        private const int MaximumSkillPercentage = 100;
        private const int FirstPostedJobCount = 1;

        public IEnumerable<JobPosting> GetAllJobs()
        {
            var jobPostingList = new List<JobPosting>();
            using (var databaseConnection = DbConnectionHelper.GetConnection())
            {
                databaseConnection.Open();
                string sqlQuery = @"
                    SELECT j.job_id, j.job_title, j.industry_field, j.job_type, j.experience_level, 
                           j.start_date, j.end_date, j.job_description, j.job_location, j.available_positions,
                           j.posted_at, j.salary, j.amount_payed, j.deadline, j.photo,
                           c.company_id, c.company_name
                    FROM jobs j
                    LEFT JOIN companies c ON j.company_id = c.company_id";

                using (var sqlCommand = new SqlCommand(sqlQuery, databaseConnection))
                using (var dataReader = sqlCommand.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        var jobPosting = new JobPosting
                        {
                            JobId = dataReader.GetInt32(dataReader.GetOrdinal("job_id")),
                            JobTitle = dataReader.GetString(dataReader.GetOrdinal("job_title")),
                            IndustryField = dataReader.GetString(dataReader.GetOrdinal("industry_field")),
                            JobType = dataReader.GetString(dataReader.GetOrdinal("job_type")),
                            ExperienceLevel = dataReader.GetString(dataReader.GetOrdinal("experience_level"))
                        };

                        if (!dataReader.IsDBNull(dataReader.GetOrdinal("company_id")))
                        {
                            jobPosting.Company = new Company(
                                dataReader.GetString(dataReader.GetOrdinal("company_name")),
                                DefaultCompanyDescription,
                                DefaultProfilePicture,
                                DefaultLogoPicture,
                                DefaultLocation,
                                DefaultEmail,
                                dataReader.GetInt32(dataReader.GetOrdinal("company_id"))
                            );
                        }

                        jobPostingList.Add(jobPosting);
                    }
                }
            }

            using (var databaseConnection = DbConnectionHelper.GetConnection())
            {
                databaseConnection.Open();
                string sqlQuery = @"
                    SELECT js.job_id, js.skill_id, js.required_percentage, s.skill_name
                    FROM job_skills js
                    JOIN skills s ON js.skill_id = s.skill_id";

                using (var sqlCommand = new SqlCommand(sqlQuery, databaseConnection))
                using (var dataReader = sqlCommand.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        int jobId = dataReader.GetInt32(dataReader.GetOrdinal("job_id"));
                        var targetJob = jobPostingList.Find(job => job.JobId == jobId);

                        if (targetJob != null)
                        {
                            var jobSkill = new JobSkill
                            {
                                Job = targetJob,
                                RequiredPercentage = dataReader.GetInt32(dataReader.GetOrdinal("required_percentage")),
                                Skill = new Skill
                                {
                                    SkillId = dataReader.GetInt32(dataReader.GetOrdinal("skill_id")),
                                    SkillName = dataReader.GetString(dataReader.GetOrdinal("skill_name"))
                                }
                            };
                            targetJob.JobSkills.Add(jobSkill);
                        }
                    }
                }
            }

            return jobPostingList;
        }

        public IReadOnlyList<Skill> GetAllSkills()
        {
            var skillList = new List<Skill>();
            using var databaseConnection = DbConnectionHelper.GetConnection();
            databaseConnection.Open();
            using var sqlCommand = new SqlCommand(
                "SELECT skill_id, skill_name FROM skills",
                databaseConnection);
            using var dataReader = sqlCommand.ExecuteReader();

            while (dataReader.Read())
            {
                skillList.Add(new Skill
                {
                    SkillId = dataReader.GetInt32(dataReader.GetOrdinal("skill_id")),
                    SkillName = dataReader.GetString(dataReader.GetOrdinal("skill_name"))
                });
            }

            return skillList;
        }

        public int AddJob(JobPosting jobPosting, int companyId, IReadOnlyList<(int SkillId, int RequiredPercentage)> skillLinks)
        {
            if (jobPosting == null)
            {
                throw new ArgumentNullException(nameof(jobPosting));
            }

            using var databaseConnection = DbConnectionHelper.GetConnection();
            databaseConnection.Open();
            using var sqlTransaction = databaseConnection.BeginTransaction();

            using (var nextIdCommand = new SqlCommand(
                       "SELECT COALESCE(MAX(job_id), 0) + 1 FROM jobs WITH (UPDLOCK, HOLDLOCK)",
                       databaseConnection,
                       sqlTransaction))
            {
                var nextIdObject = nextIdCommand.ExecuteScalar();
                int nextId = Convert.ToInt32(nextIdObject);

                using var insertCommand = new SqlCommand(
                    @"INSERT INTO jobs (
                        job_id, company_id, photo, job_title, industry_field, job_type, experience_level,
                        start_date, end_date, job_description, job_location, available_positions,
                        posted_at, salary, amount_payed, deadline)
                      VALUES (
                        @JobId, @CompanyId, @Photo, @JobTitle, @IndustryField, @JobType, @ExperienceLevel,
                        @StartDate, @EndDate, @JobDescription, @JobLocation, @AvailablePositions,
                        @PostedAt, @Salary, @AmountPayed, @Deadline)",
                    databaseConnection,
                    sqlTransaction);

                insertCommand.Parameters.AddWithValue("@JobId", nextId);
                insertCommand.Parameters.AddWithValue("@CompanyId", companyId);
                insertCommand.Parameters.AddWithValue("@Photo", string.IsNullOrWhiteSpace(jobPosting.Photo) ? (object)DBNull.Value : jobPosting.Photo);
                insertCommand.Parameters.AddWithValue("@JobTitle", jobPosting.JobTitle);
                insertCommand.Parameters.AddWithValue("@IndustryField", jobPosting.IndustryField);
                insertCommand.Parameters.AddWithValue("@JobType", jobPosting.JobType);
                insertCommand.Parameters.AddWithValue("@ExperienceLevel", jobPosting.ExperienceLevel);
                insertCommand.Parameters.AddWithValue("@StartDate", jobPosting.StartDate.HasValue ? (object)jobPosting.StartDate.Value.Date : DBNull.Value);
                insertCommand.Parameters.AddWithValue("@EndDate", jobPosting.EndDate.HasValue ? (object)jobPosting.EndDate.Value.Date : DBNull.Value);
                insertCommand.Parameters.AddWithValue("@JobDescription", jobPosting.JobDescription);
                insertCommand.Parameters.AddWithValue("@JobLocation", jobPosting.JobLocation);
                insertCommand.Parameters.AddWithValue("@AvailablePositions", jobPosting.AvailablePositions);
                insertCommand.Parameters.AddWithValue("@PostedAt", jobPosting.PostedAt.HasValue ? (object)jobPosting.PostedAt.Value : (object)DateTime.Now);
                insertCommand.Parameters.AddWithValue("@Salary", jobPosting.Salary.HasValue ? (object)jobPosting.Salary.Value : DBNull.Value);
                insertCommand.Parameters.AddWithValue("@AmountPayed", jobPosting.AmountPayed.HasValue ? (object)jobPosting.AmountPayed.Value : (object)DefaultAmountPayed);
                insertCommand.Parameters.AddWithValue("@Deadline", jobPosting.Deadline.HasValue ? (object)jobPosting.Deadline.Value.Date : DBNull.Value);

                insertCommand.ExecuteNonQuery();

                if (skillLinks != null)
                {
                    foreach (var (skillId, percentage) in skillLinks)
                    {
                        if (percentage < MinimumSkillPercentage || percentage > MaximumSkillPercentage)
                        {
                            continue;
                        }

                        using var insertJobSkillCommand = new SqlCommand(
                            @"INSERT INTO job_skills (job_id, skill_id, required_percentage)
                              VALUES (@JobId, @SkillId, @Percentage)",
                            databaseConnection,
                            sqlTransaction);

                        insertJobSkillCommand.Parameters.AddWithValue("@JobId", nextId);
                        insertJobSkillCommand.Parameters.AddWithValue("@SkillId", skillId);
                        insertJobSkillCommand.Parameters.AddWithValue("@Percentage", percentage);
                        insertJobSkillCommand.ExecuteNonQuery();
                    }
                }

                using var checkExistingJobsCommand = new SqlCommand(@"
                    SELECT COUNT(*) 
                    FROM jobs 
                    WHERE company_id = @CompanyId",
                    databaseConnection, sqlTransaction);

                checkExistingJobsCommand.Parameters.AddWithValue("@CompanyId", companyId);
                int existingJobsCount = (int)checkExistingJobsCommand.ExecuteScalar();

                if (existingJobsCount == FirstPostedJobCount)
                {
                    using var updateCompanyCommand = new SqlCommand(@"
                        UPDATE companies
                        SET posted_jobs_count = 1
                        WHERE company_id = @CompanyId",
                        databaseConnection, sqlTransaction);

                    updateCompanyCommand.Parameters.AddWithValue("@CompanyId", companyId);
                    updateCompanyCommand.ExecuteNonQuery();
                }
                else
                {
                    using var updateCompanyCommand = new SqlCommand(@"
                        UPDATE companies
                        SET posted_jobs_count = posted_jobs_count + 1
                        WHERE company_id = @CompanyId",
                        databaseConnection, sqlTransaction);

                    updateCompanyCommand.Parameters.AddWithValue("@CompanyId", companyId);
                    updateCompanyCommand.ExecuteNonQuery();
                }

                sqlTransaction.Commit();
                return nextId;
            }
        }
    }
}