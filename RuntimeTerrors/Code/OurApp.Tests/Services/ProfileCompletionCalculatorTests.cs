using Microsoft.VisualStudio.TestTools.UnitTesting;
using OurApp.Core.Models;
using OurApp.Core.Services;
using OurApp.Tests.Helpers;
using System;
using System.Collections.Generic;

namespace OurApp.Tests.Services
{
    [TestClass]
    public class ProfileCompletionCalculatorTests
    {
        private FakeJobsRepository jobsRepo;
        private FakeApplicantRepository applicantRepo;
        private ProfileCompletionCalculator calculator;

        [TestInitialize]
        public void Setup()
        {
            jobsRepo = new FakeJobsRepository();
            applicantRepo = new FakeApplicantRepository();
            calculator = new ProfileCompletionCalculator(jobsRepo, applicantRepo);
        }

        private Company CreateCompany()
        {
            var company = new Company("TestCompany", "", "", "logo.png", "", "");
            company.CompanyId = 1;
            company.PostedJobsCount = 0;
            company.CollaboratorsCount = 0;
            company.game = new Game();
            return company;
        }


        [TestMethod]
        public void Calculate_EmptyCompany_ReturnsZero()
        {
            var company = CreateCompany();
            var result = calculator.Calculate(company);
            Assert.AreEqual(0, result.percentage);
            Assert.IsTrue(result.remainingTasks.Count > 0);
        }

        [TestMethod]
        public void Calculate_AllTasksCompleted_Returns100()
        {
            var company = CreateCompany();
            company.ProfilePicturePath = "pfp.png";
            company.AboutUs = "Description";
            company.PostedJobsCount = 5;
            company.CollaboratorsCount = 2;
            company.game.Publish();
            var result = calculator.Calculate(company);
            Assert.AreEqual(100, result.percentage);
            Assert.AreEqual(0, result.remainingTasks.Count);
        }

        
        //get skills top 3
        [TestMethod]
        public void GetSkillsTop3_NoJobs_ReturnsEmptyLists()
        {
            var result = calculator.GetSkillsTop3(1);
            Assert.AreEqual(0, result.skillNames.Count);
            Assert.AreEqual(0, result.percents.Count);
        }

        [TestMethod]
        public void GetSkillsTop3_ReturnsTopSkills()
        {
            var company = CreateCompany();

            var job = new JobPosting
            {
                Company = company,
                JobSkills = new List<JobSkill>
                {
                    new JobSkill { Skill = new Skill { SkillName = "C#" }, RequiredPercentage = 50 },
                    new JobSkill { Skill = new Skill { SkillName = "SQL" }, RequiredPercentage = 30 },
                    new JobSkill { Skill = new Skill { SkillName = "React" }, RequiredPercentage = 20 }
                }
            };

            jobsRepo.Jobs.Add(job);

            var result = calculator.GetSkillsTop3(company.CompanyId);
            Assert.AreEqual(3, result.skillNames.Count);
        }

        
        //applicants message
        [TestMethod]
        public void ApplicantsMessage_NoApplicants_ReturnsStartMessage()
        {
            var message = calculator.applicantsMessage(1);
            Assert.AreEqual("No applicants yet. Start posting jobs!", message);
        }

        [TestMethod]
        public void ApplicantsMessage_FirstApplicants_ReturnsGreatStart()
        {
            var company = CreateCompany();
            var job = new JobPosting { Company = company };

            applicantRepo.AddApplicant(new Applicant
            {
                Job = job,
                AppliedAt = DateTime.Now
            });

            var message = calculator.applicantsMessage(company.CompanyId);
            Assert.IsTrue(message.Contains("Great start"));
        }

        [TestMethod]
        public void ApplicantsMessage_MoreApplicants_ReturnsCongratsMessage()
        {
            var company = CreateCompany();
            var job = new JobPosting { Company = company };

            applicantRepo.AddApplicant(new Applicant { Job = job, AppliedAt = DateTime.Now });
            applicantRepo.AddApplicant(new Applicant { Job = job, AppliedAt = DateTime.Now });

            applicantRepo.AddApplicant(new Applicant { Job = job, AppliedAt = DateTime.Now.AddDays(-10) });

            var message = calculator.applicantsMessage(company.CompanyId);
            Assert.IsTrue(message.Contains("Congrats"));
        }

        [TestMethod]
        public void ApplicantsMessage_FewerApplicants_ReturnsDecreaseMessage()
        {
            var company = CreateCompany();
            var job = new JobPosting { Company = company };

            applicantRepo.AddApplicant(new Applicant { Job = job, AppliedAt = DateTime.Now.AddDays(-10) });
            applicantRepo.AddApplicant(new Applicant { Job = job, AppliedAt = DateTime.Now.AddDays(-10) });
            applicantRepo.AddApplicant(new Applicant { Job = job, AppliedAt = DateTime.Now.AddDays(-10) });

            applicantRepo.AddApplicant(new Applicant { Job = job, AppliedAt = DateTime.Now.AddDays(-8) });

            var message = calculator.applicantsMessage(company.CompanyId);

            Assert.IsTrue(message.Contains("fewer"));
        }
    }
}