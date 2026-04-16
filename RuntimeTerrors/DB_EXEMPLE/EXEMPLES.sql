USE iss_project;
GO

-- 1. Insert 3 Companies
INSERT INTO companies (company_id, company_name, logo_picture_url)
VALUES 
(1, 'TechNova Solutions', 'technova_logo.png'),
(2, 'Global Innovations Inc.', 'global_logo.png'),
(3, 'Startup Hub', 'startup_logo.png');

-- 2. Insert 5 Jobs
INSERT INTO jobs (
    job_id, company_id, job_title, industry_field, job_type, 
    experience_level, job_description, job_location, available_positions, amount_payed
)
VALUES 
(1, 1, 'Senior Backend Engineer', 'IT', 'Full-Time', 'Senior', 'Writing C# and SQL.', 'New York', 2, 500),

(2, 2, 'Lead Data Scientist', 'IT', 'Full-Time', 'Senior', 'Analyzing data.', 'Remote', 1, 1200),

(3, 1, 'Junior Web Developer', 'IT', 'Full-Time', 'Junior', 'Building WinUI apps.', 'New York', 3, 0),

(4, 3, 'Marketing Intern', 'Marketing', 'Internship', 'Junior', 'Social media campaigns.', 'London', 5, 0),

(5, 2, 'Part-Time UI Designer', 'Design', 'Part-Time', 'Mid-Level', 'Creating Figma mockups.', 'Remote', 1, 250);