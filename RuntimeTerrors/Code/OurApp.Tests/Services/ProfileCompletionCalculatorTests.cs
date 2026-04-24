using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OurApp.Core.Models;
using OurApp.Core.Services;
using OurApp.Tests.Helpers;

namespace OurApp.Tests.Services
{
    [TestClass]
    public class ProfileCompletionCalculatorTests
    {
        private const string TestCompanyName = "TestCompany";
        private const string CompanyLogo = "logo.png";
        private const string CompanyPfp = "pfp.png";
        private const string CompanyDesc = "Description";
        private const string EmptyValue = "";

        private const string SkillCSharp = "C#";
        private const string SkillSql = "SQL";
        private const string SkillReact = "React";

        private const string MsgNoApplicants = "No applicants yet. Start posting jobs!";
        private const string MsgStartKeyword = "Great start";
        private const string MsgCongratsKeyword = "Congrats";
        private const string MsgDecreaseKeyword = "fewer";

        private const int CompanyIdValue = 1;
        private const int InitialJobs = 0;
        private const int InitialCollabs = 0;
        private const int CompletedJobs = 5;
        private const int CompletedCollabs = 2;

        private const int ZeroPercent = 0;
        private const int FullPercent = 100;
        private const int EmptyCount = 0;
        private const int ExpectedTopSkillsCount = 3;

        private const int SkillValHigh = 50;
        private const int SkillValMed = 30;
        private const int SkillValLow = 20;

        private const int OffsetPastDaysFar = -10;
        private const int OffsetPastDaysNear = -8;

        private FakeJobsRepository jobsRepo = null!;
        private FakeApplicantRepository applicantRepo = null!;
        private ProfileCompletionCalculator calculator = null!;

        [TestInitialize]
        public void Setup()
        {
            jobsRepo = new FakeJobsRepository();
            applicantRepo = new FakeApplicantRepository();
            calculator = new ProfileCompletionCalculator(jobsRepo, applicantRepo);
        }

        private Company CreateCompany()
        {
            var company = new Company(TestCompanyName, EmptyValue, EmptyValue, CompanyLogo, EmptyValue, EmptyValue);
            company.CompanyId = CompanyIdValue;
            company.PostedJobsCount = InitialJobs;
            company.CollaboratorsCount = InitialCollabs;
            company.Game = new Game();
            return company;
        }

        [TestMethod]
        public void Calculate_EmptyCompany_ReturnsZero()
        {
            var company = CreateCompany();
            var result = calculator.Calculate(company);
            Assert.AreEqual(ZeroPercent, result.percentage);
            Assert.IsTrue(result.remainingTasks.Count > EmptyCount);
        }

        [TestMethod]
        public void Calculate_AllTasksCompleted_Returns100()
        {
            var company = CreateCompany();
            company.ProfilePicturePath = CompanyPfp;
            company.AboutUs = CompanyDesc;
            company.PostedJobsCount = CompletedJobs;
            company.CollaboratorsCount = CompletedCollabs;
            company.Game.Publish();

            var result = calculator.Calculate(company);

            Assert.AreEqual(FullPercent, result.percentage);
            Assert.AreEqual(EmptyCount, result.remainingTasks.Count);
        }

        [TestMethod]
        public void GetSkillsTop3_NoJobs_ReturnsEmptyLists()
        {
            var result = calculator.GetSkillsTop3(CompanyIdValue);
            Assert.AreEqual(EmptyCount, result.skillNames.Count);
            Assert.AreEqual(EmptyCount, result.percents.Count);
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
                    new JobSkill { Skill = new Skill { SkillName = SkillCSharp }, RequiredPercentage = SkillValHigh },
                    new JobSkill { Skill = new Skill { SkillName = SkillSql }, RequiredPercentage = SkillValMed },
                    new JobSkill { Skill = new Skill { SkillName = SkillReact }, RequiredPercentage = SkillValLow }
                }
            };

            jobsRepo.Jobs.Add(job);

            var result = calculator.GetSkillsTop3(company.CompanyId);
            Assert.AreEqual(ExpectedTopSkillsCount, result.skillNames.Count);
        }

        [TestMethod]
        public void ApplicantsMessage_NoApplicants_ReturnsStartMessage()
        {
            var message = calculator.ApplicantsMessage(CompanyIdValue);
            Assert.AreEqual(MsgNoApplicants, message);
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

            var message = calculator.ApplicantsMessage(company.CompanyId);
            Assert.IsTrue(message.Contains(MsgStartKeyword));
        }

        [TestMethod]
        public void ApplicantsMessage_MoreApplicants_ReturnsCongratsMessage()
        {
            var company = CreateCompany();
            var job = new JobPosting { Company = company };

            applicantRepo.AddApplicant(new Applicant { Job = job, AppliedAt = DateTime.Now });
            applicantRepo.AddApplicant(new Applicant { Job = job, AppliedAt = DateTime.Now });

            applicantRepo.AddApplicant(new Applicant { Job = job, AppliedAt = DateTime.Now.AddDays(OffsetPastDaysFar) });

            var message = calculator.ApplicantsMessage(company.CompanyId);
            Assert.IsTrue(message.Contains(MsgCongratsKeyword));
        }

        [TestMethod]
        public void ApplicantsMessage_FewerApplicants_ReturnsDecreaseMessage()
        {
            var company = CreateCompany();
            var job = new JobPosting { Company = company };

            applicantRepo.AddApplicant(new Applicant { Job = job, AppliedAt = DateTime.Now.AddDays(OffsetPastDaysFar) });
            applicantRepo.AddApplicant(new Applicant { Job = job, AppliedAt = DateTime.Now.AddDays(OffsetPastDaysFar) });
            applicantRepo.AddApplicant(new Applicant { Job = job, AppliedAt = DateTime.Now.AddDays(OffsetPastDaysFar) });

            applicantRepo.AddApplicant(new Applicant { Job = job, AppliedAt = DateTime.Now.AddDays(OffsetPastDaysNear) });

            var message = calculator.ApplicantsMessage(company.CompanyId);

            Assert.IsTrue(message.Contains(MsgDecreaseKeyword));
        }
    }
}