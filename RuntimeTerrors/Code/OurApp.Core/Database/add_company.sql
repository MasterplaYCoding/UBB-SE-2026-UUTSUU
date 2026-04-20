USE iss_project;

DELETE FROM job_skills;
DELETE FROM skills;
DBCC CHECKIDENT ('skills', RESEED, 0);

DELETE FROM applicants;
DELETE FROM users;
DELETE FROM jobs;
DELETE FROM collaborators;
DELETE FROM events;
DELETE FROM companies;
GO

INSERT INTO skills (skill_name) VALUES
('C#'),
('SQL'),
('React'),
('Python'),
('Azure');

INSERT INTO companies (
    company_id, company_name, about_us, profile_picture_url, logo_picture_url, 
    location, email, buddy_name, avatar_id, final_quote, 
    scen_1_text, scen1_answer1, scen1_answer2, scen1_answer3, 
    scen1_reaction1, scen1_reaction2, scen1_reaction3, 
    scen2_text, scen2_answer1, scen2_answer2, scen2_answer3, 
    scen2_reaction1, scen2_reaction2, scen2_reaction3, 
    buddy_description, posted_jobs_count, collaborators_count
) VALUES
(
    1, 'TechNova', 'We build scalable web applications.', 'technova_profile.jpg', 'technova_logo.png', 
    'San Francisco, CA', 'hr@technova.com', 'NovaBot', 1, 'Keep innovating!', 
    'You found a critical bug in production.', 'Ignore it', 'Fix it immediately', 'Report to QA', 
    'Poor choice.', 'Excellent initiative.', 'Good protocol.', 
    'A client is unhappy with a feature.', 'Apologize', 'Blame the dev team', 'Schedule a review call', 
    'Polite but passive.', 'Unprofessional.', 'Great problem-solving approach.', 
    'Friendly technical assistant', 1, 1
),
(
    2, 'DataFlow Inc', 'Pioneering data analytics.', 'dataflow_profile.jpg', 'dataflow_logo.png', 
    'New York, NY', 'careers@dataflow.com', 'DataDan', 2, 'Trust the data.', 
    'A dataset is missing values.', 'Delete the rows', 'Impute the data', 'Leave them blank', 
    'Destructive approach.', 'Standard industry practice.', 'Will cause errors later.', 
    'The database server crashed.', 'Wait for it to fix itself', 'Reboot server', 'Alert DevOps', 
    'Too passive.', 'Could cause data loss.', 'Safest and most professional action.', 
    'Analytical companion', 1, 1
),
(
    3, 'EcoCode', 'Building sustainable and green software solutions.', 'ecocode_profile.jpg', 'ecocode_logo.png', 
    'Seattle, WA', 'hello@ecocode.com', 'Leafy', 3, 'Code for the planet!', 
    'You notice a memory leak in the background process.', 'Ignore it', 'Log it for next sprint', 'Fix it immediately', 
    'Irresponsible.', 'Acceptable, but risky.', 'Excellent initiative.', 
    'The client wants a feature delivered faster than possible.', 'Agree to rush it', 'Explain the tradeoffs', 'Work the weekend', 
    'Will lead to bugs.', 'Perfect communication.', 'Not sustainable long-term.', 
    'Eco-friendly assistant', 0, 2
),
(
    4, 'FinEdge', 'High-security financial trading platforms.', 'finedge_profile.jpg', 'finedge_logo.png', 
    'London, UK', 'hr@finedge.com', 'VaultBot', 4, 'Security is not a feature, it is the foundation.', 
    'A minor security vulnerability is found in an old module.', 'Hide it', 'Patch it next week', 'Patch it immediately', 
    'Illegal and unethical.', 'Too slow for finance.', 'The only correct answer.', 
    'The development team is falling behind schedule.', 'Yell at them', 'Reassess the sprint goals', 'Ignore the delay', 
    'Toxic leadership.', 'Excellent agile management.', 'Negligent.', 
    'Strict professional companion', 0, 1
);

INSERT INTO jobs (
    job_id, company_id, photo, job_title, industry_field, job_type, 
    experience_level, start_date, end_date, job_description, 
    job_location, available_positions, posted_at, salary, amount_payed, deadline
) VALUES
(
    101, 1, 'backend_job.jpg', 'Backend C# Developer', 'IT', 'Full-time', 
    'Mid-Level', '2026-06-01', NULL, 'Develop robust REST APIs using .NET Core.', 
    'Remote', 3, '2026-04-15 09:00:00', 95000, 0, '2026-05-15'
),
(
    102, 2, 'data_job.jpg', 'Data Engineer', 'Data Science', 'Contract', 
    'Senior', '2026-07-01', '2027-07-01', 'Maintain cloud data pipelines and warehouses.', 
    'New York, NY', 1, '2026-04-18 10:30:00', 120000, 0, '2026-06-01'
);

INSERT INTO job_skills (skill_id, job_id, required_percentage) VALUES
(1, 101, 90),
(2, 101, 60),
(5, 101, 40),
(2, 102, 95),
(4, 102, 85);

INSERT INTO applicants (
    applicant_id, job_id, cv_file_url, app_test_grade, cv_grade, 
    company_test_grade, interview_grade, application_status, 
    recommended_from_company_id, applied_at, user_id
) VALUES
(
    501, 101, 'alice_smith_cv.pdf', 8.50, 9.20, 8.00, 9.50, 
    'Accepted', NULL, '2026-04-19 14:00:00', 1001
),
(
    502, 102, 'bob_jones_cv.pdf', 7.00, 8.00, 7.50, 6.50, 
    'Rejected', 1, '2026-04-20 09:15:00', 1002
);

INSERT INTO users (user_id, name, email, cv_xml) VALUES
(
    1001, 'Alice Smith', 'alice.smith@email.com', 
    '<cv><summary>Experienced C# Developer</summary><skills><skill>C#</skill><skill>SQL</skill></skills></cv>'
),
(
    1002, 'Bob Jones', 'bob.jones@email.com', 
    '<cv><summary>Data Enthusiast</summary><skills><skill>Python</skill><skill>SQL</skill></skills></cv>'
);

INSERT INTO events (
    event_id, host_company_id, photo, title, description, 
    start_date, end_date, location, posted_at
) VALUES
(
    201, 1, 'hackathon.jpg', 'TechNova Spring Hackathon', 
    'Join us for 48 hours of intense coding and problem solving.', 
    '2026-05-10', '2026-05-12', 'San Francisco HQ', '2026-04-10 08:00:00'
),
(
    202, 2, 'summit.jpg', 'Data Summit 2026', 
    'Exploring the future of big data, AI, and machine learning.', 
    '2026-08-20', '2026-08-21', 'New York Convention Center', '2026-04-12 11:00:00'
),
(
    203, 3, 'winter_summit_2026.jpg', 'Winter Web Summit 2026', 
    'Our annual kickoff exploring sustainable tech. Thank you to everyone who attended!', 
    '2026-01-15', '2026-01-17', 'Seattle Convention Center', '2025-11-01 10:00:00'
),
(
    204, 4, 'fintech_panel.jpg', 'The Future of FinTech Security', 
    'A live panel discussion on securing high-frequency trading platforms against modern threats.', 
    '2026-05-05', '2026-05-05', 'Virtual', '2026-04-01 09:00:00'
),
(
    205, 1, 'cloud_workshop_2025.jpg', 'TechNova Cloud Architecture Workshop', 
    'A hands-on deep dive into building scalable cloud solutions using .NET and Azure.', 
    '2025-10-10', '2025-10-11', 'TechNova HQ, San Francisco', '2025-08-01 09:00:00'
),
(
    206, 1, 'opensource_summit_2026.jpg', 'TechNova Open Source Summit 2026', 
    'Join our core developers as we contribute to major open-source projects for the weekend.', 
    '2026-09-05', '2026-09-06', 'Virtual', '2026-04-10 12:00:00'
);

INSERT INTO collaborators (event_id, company_id) VALUES
(201, 2),
(202, 1),
(203, 1),
(203, 4),
(204, 2),
(205, 2),
(206, 3);