USE MyDb;
GO



-- Clear existing data (in correct order to respect foreign keys)
DELETE FROM job_skills;
DELETE FROM skills;
DELETE FROM applicants;
DELETE FROM users;
DELETE FROM jobs;
DELETE FROM companies;
GO

INSERT INTO companies
                  (company_id, company_name, about_us, profile_picture_url, logo_picture_url, location, email,
                   buddy_name, buddy_description, avatar_id, final_quote,
                   scen_1_text, scen1_answer1, scen1_answer2, scen1_answer3,
                   scen1_reaction1, scen1_reaction2, scen1_reaction3,
                   scen2_text, scen2_answer1, scen2_answer2,
                   scen2_answer3, scen2_reaction1, scen2_reaction2, scen2_reaction3)
                  VALUES
                  (1, 'sdfghj', 'asdfgh', ' ', ' ',' ', 'dfgh@gmail.com',
                   ' ', ' ', ' ', ' ',
                   ' ', ' ', ' ', ' ',
                   ' ', ' ', ' ',
                   ' ', ' ', ' ',
                   ' ', ' ', ' ', ' ');

-- Seed Jobs
INSERT INTO jobs (job_id, company_id, job_title, industry_field, job_type, experience_level, job_description, job_location, available_positions)
VALUES (1, 1, 'Software Engineer', 'IT', 'Full Time', 'Junior', 'Write code', 'Local', 5);

-- Seed Skills
SET IDENTITY_INSERT skills ON;
INSERT INTO skills (skill_id, skill_name) VALUES (1, 'c#');
INSERT INTO skills (skill_id, skill_name) VALUES (2, 'sql');
INSERT INTO skills (skill_id, skill_name) VALUES (3, '.net');
SET IDENTITY_INSERT skills OFF;

-- Seed Job Skills (link jobs to skills)
INSERT INTO job_skills (job_id, skill_id, required_percentage) VALUES (1, 1, 80);
INSERT INTO job_skills (job_id, skill_id, required_percentage) VALUES (1, 2, 70);
INSERT INTO job_skills (job_id, skill_id, required_percentage) VALUES (1, 3, 60);

-- Seed Users (CV XML stored in users.cv_xml)
SET IDENTITY_INSERT users ON;
INSERT INTO users (user_id, name, email, cv_xml) VALUES (
    1,
    N'Cipicao ',
    N'florea.ciprianm@gmail.com',
    N'<?xml version="1.0" encoding="utf-8"?><Cv><Name>Cipicao</Name><Email>florea.ciprianm@gmail.com</Email><Phone>+40 721 234 567</Phone><Summary>Software developer focused on cloud services with agile teams shipping react and javascript features.</Summary><Projects>Built dotnet REST APIs with csharp and SQL. React SPA with javascript tests, docker on Azure, agile sprints with daily standups.</Projects><Skills>javascript</Skills><Interests>open source azure docker</Interests></Cv>'
);
INSERT INTO users (user_id, name, email, cv_xml) VALUES (
    2,
    N'Bobby',
    N'bobbyb@test.com',
    N'<?xml version="1.0" encoding="utf-8"?><Cv><Name>Bobby</Name><Email>bobbyb@test.com</Email><ContactNumber>+1 555 010 2030</ContactNumber><Summary>Backend engineer building csharp services, SQL data layers, dotnet APIs, and pragmatic agile delivery with python tooling.</Summary><Projects>Microservices in csharp and SQL. Python data jobs, react UI, javascript automation, dotnet core on Azure with agile delivery.</Projects><Skills>csharp sql dotnet python</Skills><Interests>java react agile</Interests></Cv>'
);
SET IDENTITY_INSERT users OFF;

-- Seed Applicants (no CV column; CV is on the linked user)
INSERT INTO applicants (applicant_id, job_id, application_status, applied_at, user_id)
VALUES (101, 1, 'Pending', GETDATE(), 1);

INSERT INTO applicants (applicant_id, job_id, application_status, applied_at, user_id, app_test_grade, cv_grade)
VALUES (102, 1, 'On Hold', GETDATE(), 2, 7.5, 8.0);
