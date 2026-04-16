use iss_project

CREATE TABLE companies (
    company_id int PRIMARY KEY,
    company_name nvarchar(255) not null,
    about_us text,
    profile_picture_url nvarchar(max),
    logo_picture_url nvarchar(max) not null,
    location nvarchar(300),
    email nvarchar(100),
    buddy_name nvarchar(255),
    avatar_id int,
    final_quote text,
    scen_1_text text,
    scen1_answer1 text,
    scen1_answer2 text,
    scen1_answer3 text,
    scen1_reaction1 text,
    scen1_reaction2 text,
    scen1_reaction3 text,
    scen2_text text,
    scen2_answer1 text,
    scen2_answer2 text,
    scen2_answer3 text,
    scen2_reaction1 text,
    scen2_reaction2 text,
    scen2_reaction3 text,
    buddy_description nvarchar(255),
    posted_jobs_count int,
    collaborators_count int    
    );

CREATE TABLE jobs (
    job_id int PRIMARY KEY,
    company_id int,
    photo nvarchar(255),
    job_title nvarchar(255) NOT NULL,
    industry_field nvarchar(255) NOT NULL,
    job_type nvarchar(255) NOT NULL,
    experience_level nvarchar(255) NOT NULL,
    start_date date,
    end_date date,
    job_description text NOT NULL,
    job_location nvarchar(255) NOT NULL,
    available_positions int NOT NULL,
    posted_at datetime, 
    salary int,
    amount_payed int,
    deadline date,
    FOREIGN KEY (company_id) REFERENCES companies(company_id) ON DELETE CASCADE
);

CREATE TABLE skills (
    skill_id int PRIMARY KEY IDENTITY,
    skill_name nvarchar(255) NOT NULL
);


CREATE TABLE job_skills (
    skill_id int,
    job_id int,
    PRIMARY KEY (skill_id , job_id),
    required_percentage int NOT NULL,
    FOREIGN KEY (job_id) REFERENCES jobs(job_id) ON DELETE CASCADE,
    FOREIGN KEY (skill_id) REFERENCES skills(skill_id) ON DELETE CASCADE,
);


CREATE TABLE applicants (
    applicant_id int PRIMARY KEY,
    job_id int,
    cv_file_url nvarchar(500),
    app_test_grade decimal(5,2),
    cv_grade decimal(5,2),
    company_test_grade decimal(5,2),
    interview_grade decimal(5,2),
    application_status nvarchar(50),
    recommended_from_company_id int,
    applied_at datetime,
    user_id int,
    FOREIGN KEY (job_id) REFERENCES jobs(job_id) ON DELETE CASCADE,
    FOREIGN KEY (recommended_from_company_id) REFERENCES companies(company_id)
);

CREATE TABLE events (
    event_id int PRIMARY KEY,
    host_company_id int NOT NULL,
    photo varchar(max),
title varchar(200) not null,
	description text,
	start_date date not null,
	end_date date not null,
	location varchar(300) not null,
	posted_at datetime,
    FOREIGN KEY (host_company_id) REFERENCES companies(company_id) ON DELETE CASCADE
);


CREATE TABLE collaborators (
    event_id int NOT NULL,
    company_id int NOT NULL,
    PRIMARY KEY (event_id, company_id),
    FOREIGN KEY (event_id) REFERENCES events(event_id) ON DELETE CASCADE,
    FOREIGN KEY (company_id) REFERENCES companies(company_id)
);

ALTER TABLE applicants
ALTER COLUMN user_id INT NOT NULL;

ALTER TABLE applicants
ADD CONSTRAINT UQ_applicants_user_id UNIQUE (user_id);

--from other team
CREATE TABLE users (
	user_id int PRIMARY KEY,
	name nvarchar(255) not null,
	email nvarchar(255) not null,
	cv_xml nvarchar(max),
FOREIGN KEY (user_id) REFERENCES applicants(user_id) ON DELETE CASCADE
);

