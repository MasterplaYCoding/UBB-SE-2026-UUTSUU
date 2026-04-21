using OurApp.Tests.Helpers;
using OurApp.Core.Services;
using OurApp.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OurApp.Tests.Services
{
    [TestClass]
    public class ApplicantServiceTests
    {
        private FakeApplicantRepository? fakeRepo;
        private ApplicantService? sut;

        [TestInitialize]
        public void Setup()
        {
            fakeRepo = new FakeApplicantRepository();
            sut = new ApplicantService(fakeRepo);
        }

        private static Applicant MakeApplicant(int id = 1)
        {
            return new Applicant
            {
                ApplicantId = id,
                Job = new JobPosting { JobId = 10 },
                User = new User(id * 100, "Test User", "test@test.com")
            };
        }

        private static string MakeValidCvXml()
        {
            return @"<CV>
                     <Name>Alice</Name>
                     <Email>alice@example.com</Email>
                     <Phone>0466428888</Phone>
                     <Skills>c# sql python</Skills>
                     <Interests>coding</Interests>
                     <Summary>Passionate software developer with many years of experience</Summary>
                     <Projects>Built enterprise applications using c# and sql</Projects>
            </CV>";
        }

        [TestMethod]
        public void GetApplicant_ExistingId_ReturnsApplicant()
        {
            fakeRepo.AddApplicant(MakeApplicant(id: 1));
            var result = sut.GetApplicant(1);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void GetApplicant_NonExistingId_ReturnsNull()
        {
            var result = sut.GetApplicant(999);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetApplicantsForJob_NullJob_ReturnsEmptyList()
        {
            var result = sut.GetApplicantsForJob(null);
            Assert.AreEqual(0, new List<Applicant>(result).Count);
        }

        [TestMethod]
        public void GetApplicantsForJob_TwoApplicantsSameJob_ReturnsBoth()
        {
            var job = new JobPosting { JobId = 10 };
            fakeRepo.AddApplicant(MakeApplicant(id: 1));
            fakeRepo.AddApplicant(MakeApplicant(id: 2));

            var result = sut.GetApplicantsForJob(job);
            Assert.AreEqual(2, new List<Applicant>(result).Count);
        }

        [TestMethod]
        public void UpdateAppTestGrade_ExistingApplicant_StoresGrade()
        {
            fakeRepo.AddApplicant(MakeApplicant(id: 1));
            sut.UpdateAppTestGrade(1, 7.0m);
            Assert.AreEqual(7.0m, fakeRepo.LastUpdated.AppTestGrade);
        }

        [TestMethod]
        public void UpdateAppTestGrade_ExistingApplicant_CallsRepositoryUpdate()
        {
            fakeRepo.AddApplicant(MakeApplicant(id: 1));
            sut.UpdateAppTestGrade(1, 7.0m);
            Assert.IsNotNull(fakeRepo.LastUpdated);
        }

        [TestMethod]
        public void UpdateAppTestGrade_NonExistingApplicant_DoesNotCallRepositoryUpdate()
        {
            sut.UpdateAppTestGrade(999, 7.0m);
            Assert.IsNull(fakeRepo.LastUpdated);
        }

        [TestMethod]
        public void UpdateCompanyTestGrade_ExistingApplicant_StoresGrade()
        {
            fakeRepo.AddApplicant(MakeApplicant(id: 1));
            sut.UpdateCompanyTestGrade(1, 8.0m);
            Assert.AreEqual(8.0m, fakeRepo.LastUpdated.CompanyTestGrade);
        }

        [TestMethod]
        public void UpdateCompanyTestGrade_NonExistingApplicant_DoesNotCallRepositoryUpdate()
        {
            sut.UpdateCompanyTestGrade(999, 8.0m);
            Assert.IsNull(fakeRepo.LastUpdated);
        }

        [TestMethod]
        public void UpdateInterviewGrade_ExistingApplicant_StoresGrade()
        {
            fakeRepo.AddApplicant(MakeApplicant(id: 1));
            sut.UpdateInterviewGrade(1, 9.0m);
            Assert.AreEqual(9.0m, fakeRepo.LastUpdated.InterviewGrade);
        }

        [TestMethod]
        public void UpdateInterviewGrade_NonExistingApplicant_DoesNotCallRepositoryUpdate()
        {
            sut.UpdateInterviewGrade(999, 9.0m);
            Assert.IsNull(fakeRepo.LastUpdated);
        }

        [TestMethod]
        public void UpdateAppTestGrade_GradeBelowIndividualThreshold_SetsStatusRejected()
        {
            fakeRepo.AddApplicant(MakeApplicant(id: 1));
            sut.UpdateAppTestGrade(1, 5.4m);
            Assert.AreEqual("Rejected", fakeRepo.LastUpdated.ApplicationStatus);
        }

        [TestMethod]
        public void UpdateAppTestGrade_GradeExactlyAtIndividualThreshold_SetsStatusRejected()
        {
            fakeRepo.AddApplicant(MakeApplicant(id: 1));
            sut.UpdateAppTestGrade(1, 5.5m);
            Assert.AreEqual("Rejected", fakeRepo.LastUpdated.ApplicationStatus);
        }

        [TestMethod]
        public void UpdateAppTestGrade_GradeAboveIndividualThreshold_DoesNotReject()
        {
            fakeRepo.AddApplicant(MakeApplicant(id: 1));
            sut.UpdateAppTestGrade(1, 8.0m);
            Assert.AreNotEqual("Rejected", fakeRepo.LastUpdated.ApplicationStatus);
        }

        [TestMethod]
        public void UpdateCompanyTestGrade_AverageBelowCollectiveThreshold_SetsStatusRejected()
        {
            var applicant = MakeApplicant(id: 1);
            applicant.AppTestGrade = 6.0m;
            fakeRepo.AddApplicant(applicant);

            sut.UpdateCompanyTestGrade(1, 5.5m);
            Assert.AreEqual("Rejected", fakeRepo.LastUpdated.ApplicationStatus);
        }

        [TestMethod]
        public void UpdateCompanyTestGrade_AverageExactlyAtCollectiveThresholdDoesNotReject()
        {
            var applicant = MakeApplicant(id: 1);
            applicant.AppTestGrade = 7.0m;
            fakeRepo.AddApplicant(applicant);

            sut.UpdateCompanyTestGrade(1, 7.0m);
            Assert.AreNotEqual("Rejected", fakeRepo.LastUpdated.ApplicationStatus);
        }

        [TestMethod]
        public void UpdateInterviewGrade_ALlFourGradesPassingAndNoStatusSet_SetsStatusOnHold()
        {
            var applicant = MakeApplicant(id: 1);
            applicant.AppTestGrade = 8.0m;
            applicant.CvGrade = 8.0m;
            applicant.CompanyTestGrade = 8.0m;
            fakeRepo.AddApplicant(applicant);

            sut.UpdateInterviewGrade(1, 8.0m);
            Assert.AreEqual("On Hold", fakeRepo.LastUpdated.ApplicationStatus);
        }

        [TestMethod]
        public void UpdateInterviewGrade_AllFourGradesPassingButStatusAlreadySet_DoesNotOverwriteStatus()
        {
            var applicant = MakeApplicant(id: 1);
            applicant.AppTestGrade = 8.0m;
            applicant.CvGrade = 8.0m;
            applicant.CompanyTestGrade = 8.0m;
            applicant.ApplicationStatus = "Accepted";
            fakeRepo.AddApplicant(applicant);

            sut.UpdateInterviewGrade(1, 8.0m);
            Assert.AreEqual("Accepted", fakeRepo.LastUpdated.ApplicationStatus);
        }

        [TestMethod]
        public void RemoveApplicant_CallsRepositoryRemoveWithCorrectId()
        {
            sut.RemoveApplicant(42);
            Assert.AreEqual(42, fakeRepo.LastRemovedId);
        }

        [TestMethod]
        public void ScanCvXml_NullCv_ReturnsNull()
        {
            var applicant = MakeApplicant(id: 1);
            applicant.User = new User(100, "Test", "test@test.com", null);

            var result = sut.ScanCvXml(applicant);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ScanCvXml_InvalidXml_ReturnsNull()
        {
            var applicant = MakeApplicant(id: 1);
            applicant.User = new User(100, "Test", "test@test.com", "not xml at all");

            var result = sut.ScanCvXml(applicant);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void ScanCvXml_CvMissingRequiredField_ReturnsNull()
        {
            // Missing Phone
            var applicant = MakeApplicant(id: 1);
            applicant.User = new User(100, "Test", "test@test.com", @"<CV>
                <Name>Alice Smith</Name>
                <Email>alice@example.com</Email>
                <Skills>c# sql</Skills>
                <Interests>coding</Interests>
                <Summary>Passionate software developer with many years of experience</Summary>
                <Projects>Built enterprise applications using c# and sql</Projects>
            </CV>");

            var result = sut.ScanCvXml(applicant);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ScanCvXml_ValidCv_ReturnsGradeGreaterThanZero()
        {
            var applicant = MakeApplicant(id: 1);
            applicant.User = new User(100, "Test", "test@test.com", MakeValidCvXml());

            var result = sut.ScanCvXml(applicant);
            Assert.IsTrue(result > 0);
        }

        [TestMethod]
        public void ScanCvXml_ValidCv_ReturnsGradeNotExceedingTen()
        {
            var applicant = MakeApplicant(id: 1);
            applicant.User = new User(100, "Test", "test@test.com", MakeValidCvXml());

            var result = sut.ScanCvXml(applicant);
            Assert.IsTrue(result <= 10.0m);
        }

        public void ScanCvXml_CvWithInvalidEmail_ReturnsNull()
        {
            var applicant = MakeApplicant(id: 1);
            applicant.User = new User(100, "Test", "test@test.com", @"<CV>
                <Name>Alice Smith</Name>
                <Email>notanemail</Email>
                <Phone>0466428888</Phone>
                <Skills>c# sql</Skills>
                <Interests>coding</Interests>
                <Summary>Passionate software developer with many years of experience</Summary>
                <Projects>Built enterprise applications using c# and sql</Projects>
            </CV>");

            var result = sut.ScanCvXml(applicant);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ScanCvXml_CvWithNameTooShort_ReturnsNull()
        {
            var applicant = MakeApplicant(id: 1);
            applicant.User = new User(100, "Test", "test@test.com", @"<CV>
                <Name>A</Name>
                <Email>alice@example.com</Email>
                <Phone>0466428888</Phone>
                <Skills>c# sql</Skills>
                <Interests>coding</Interests>
                <Summary>Passionate software developer with many years of experience</Summary>
                <Projects>Built enterprise applications using c# and sql</Projects>
            </CV>");

            var result = sut.ScanCvXml(applicant);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ScanCvXml_CvWithSummaryTooShort_ReturnsNull()
        {
            var applicant = MakeApplicant(id: 1);
            applicant.User = new User(100, "Test", "test@test.com", @"<CV>
                <Name>Alice Smith</Name>
                <Email>alice@example.com</Email>
                <Phone>0466428888</Phone>
                <Skills>c# sql</Skills>
                <Interests>coding</Interests>
                <Summary>Too short</Summary>
                <Projects>Built enterprise applications using c# and sql</Projects>
            </CV>");

            var result = sut.ScanCvXml(applicant);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ScanCvXml_CvWithProjectsTooShort_ReturnsNull()
        {
            var applicant = MakeApplicant(id: 1);
            applicant.User = new User(100, "Test", "test@test.com", @"<CV>
                <Name>Alice Smith</Name>
                <Email>alice@example.com</Email>
                <Phone>0466428888</Phone>
                <Skills>c# sql</Skills>
                <Interests>coding</Interests>
                <Summary>Passionate software developer with many years of experience</Summary>
                <Projects>Short</Projects>
            </CV>");

            var result = sut.ScanCvXml(applicant);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ScanCvXml_CvWithPhoneTooFewDigits_ReturnsNull()
        {
            var applicant = MakeApplicant(id: 1);
            applicant.User = new User(100, "Test", "test@test.com", @"<CV>
                <Name>Alice Smith</Name>
                <Email>alice@example.com</Email>
                <Phone>123</Phone>
                <Skills>c# sql</Skills>
                <Interests>coding</Interests>
                <Summary>Passionate software developer with many years of experience</Summary>
                <Projects>Built enterprise applications using c# and sql</Projects>
            </CV>");

            var result = sut.ScanCvXml(applicant);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ScanCvXml_CvWithSkillsTooShort_ReturnsNull()
        {
            var applicant = MakeApplicant(id: 1);
            applicant.User = new User(100, "Test", "test@test.com", @"<CV>
                <Name>Alice Smith</Name>
                <Email>alice@example.com</Email>
                <Phone>0466428888</Phone>
                <Skills>ab</Skills>
                <Interests>coding</Interests>
                <Summary>Passionate software developer with many years of experience</Summary>
                <Projects>Built enterprise applications using c# and sql</Projects>
            </CV>");

            var result = sut.ScanCvXml(applicant);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ScanCvXml_CvWithInterestsTooShort_ReturnsNull()
        {
            var applicant = MakeApplicant(id: 1);
            applicant.User = new User(100, "Test", "test@test.com", @"<CV>
                <Name>Alice Smith</Name>
                <Email>alice@example.com</Email>
                <Phone>0466428888</Phone>
                <Skills>c# sql</Skills>
                <Interests>ab</Interests>
                <Summary>Passionate software developer with many years of experience</Summary>
                <Projects>Built enterprise applications using c# and sql</Projects>
            </CV>");

            var result = sut.ScanCvXml(applicant);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ScanCvXml_ValidCvWithJobSkills_ReturnsGradeGreaterThanZero()
        {
            var applicant = MakeApplicant(id: 1);
            applicant.Job = new JobPosting
            {
                JobId = 10,
                JobSkills = new List<JobSkill>
        {
            new JobSkill { Skill = new Skill { SkillName = "c#" }, RequiredPercentage = 80 },
            new JobSkill { Skill = new Skill { SkillName = "sql" }, RequiredPercentage = 60 }
        }
            };
            applicant.User = new User(100, "Test", "test@test.com", MakeValidCvXml());

            var result = sut.ScanCvXml(applicant);

            Assert.IsTrue(result > 0);
        }

        [TestMethod]
        public void ScanCvXml_CvWithContactNumberInsteadOfPhone_ReturnsGrade()
        {
            var applicant = MakeApplicant(id: 1);
            applicant.User = new User(100, "Test", "test@test.com", @"<CV>
                <Name>Alice Smith</Name>
                <Email>alice@example.com</Email>
                <ContactNumber>0466428888</ContactNumber>
                <Skills>c# sql python</Skills>
                <Interests>coding</Interests>
                <Summary>Passionate software developer with many years of experience</Summary>
                <Projects>Built enterprise applications using c# and sql</Projects>
            </CV>");

            var result = sut.ScanCvXml(applicant);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void ScanCvXml_CvWithEmailMissingDotInDomain_ReturnsNull()
        {
            var applicant = MakeApplicant(id: 1);
            applicant.User = new User(100, "Test", "test@test.com", @"<CV>
                <Name>Alice Smith</Name>
                <Email>alice@nodot</Email>
                <Phone>0466428888</Phone>
                <Skills>c# sql</Skills>
                <Interests>coding</Interests>
                <Summary>Passionate software developer with many years of experience</Summary>
                <Projects>Built enterprise applications using c# and sql</Projects>
            </CV>");

            var result = sut.ScanCvXml(applicant);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ScanCvXml_CvWithEmailStartingWithAt_ReturnsNull()
        {
            var applicant = MakeApplicant(id: 1);
            applicant.User = new User(100, "Test", "test@test.com", @"<CV>
                <Name>Alice Smith</Name>
                <Email>@example.com</Email>
                <Phone>0466428888</Phone>
                <Skills>c# sql</Skills>
                <Interests>coding</Interests>
                <Summary>Passionate software developer with many years of experience</Summary>
                <Projects>Built enterprise applications using c# and sql</Projects>
            </CV>");

            var result = sut.ScanCvXml(applicant);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ScanCvXml_ValidCvWithSynonymKeyword_ReturnsGradeGreaterThanZero()
        {
            var applicant = MakeApplicant(id: 1);
            applicant.User = new User(100, "Test", "test@test.com", @"<CV>
                <Name>Alice Smith</Name>
                <Email>alice@example.com</Email>
                <Phone>0466428888</Phone>
                <Skills>csharp sql dotnet</Skills>
                <Interests>coding</Interests>
                <Summary>Passionate software developer with many years of experience</Summary>
                <Projects>Built enterprise applications using csharp and sql</Projects>
            </CV>");

            var result = sut.ScanCvXml(applicant);
            Assert.IsTrue(result > 0);
        }

        [TestMethod]
        public void ScanCvXml_ValidCvWithRepeatedKeywords_ReturnsGradeGreaterThanZero()
        {
            var applicant = MakeApplicant(id: 1);
            applicant.User = new User(100, "Test", "test@test.com", @"<CV>
                <Name>Alice Smith</Name>
                <Email>alice@example.com</Email>
                <Phone>0466428888</Phone>
                <Skills>c# c# sql sql python python</Skills>
                <Interests>coding</Interests>
                <Summary>Passionate software developer with many years of experience in c# and sql</Summary>
                <Projects>Built enterprise applications using c# sql and python</Projects>
            </CV>");

            var result = sut.ScanCvXml(applicant);
            Assert.IsTrue(result > 0);
        }

        [TestMethod]
        public void ScanCvXml_ValidCvWithManyKeywords_ReturnsGradeCappedAtTen()
        {
            var applicant = MakeApplicant(id: 1);
            applicant.Job = new JobPosting
            {
                JobId = 10,
                JobSkills = new List<JobSkill>
        {
            new JobSkill { Skill = new Skill { SkillName = "python" }, RequiredPercentage = 80 },
            new JobSkill { Skill = new Skill { SkillName = "sql" }, RequiredPercentage = 80 },
            new JobSkill { Skill = new Skill { SkillName = "java" }, RequiredPercentage = 80 },
            new JobSkill { Skill = new Skill { SkillName = "react" }, RequiredPercentage = 80 },
            new JobSkill { Skill = new Skill { SkillName = "docker" }, RequiredPercentage = 80 }
        }
            };
            applicant.User = new User(100, "Test", "test@test.com", @"<CV>
                <Name>Alice Smith</Name>
                <Email>alice@example.com</Email>
                <Phone>0466428888</Phone>
                <Skills>python python python python python sql sql sql sql sql java java java java java react react react react react docker docker docker docker docker</Skills>
                <Interests>python sql java react docker python sql java react docker python sql java react docker python sql java react docker python sql java react docker</Interests>
                <Summary>Passionate python sql java react docker developer with many years of experience in python sql java react docker development</Summary>
                <Projects>Built enterprise python sql java react docker applications using python sql java react docker technologies and frameworks</Projects>
            </CV>");

            var result = sut.ScanCvXml(applicant);
            Assert.AreEqual(10.0m, result);
        }

        [TestMethod]
        public void ProcessCv_NonExistingApplicant_DoesNotCallRepositoryUpdate()
        {
            sut.ProcessCv(999);
            Assert.IsNull(fakeRepo.LastUpdated);
        }

        [TestMethod]
        public void ProcessCv_ValidCv_SetsCvGradeOnApplicant()
        {
            var applicant = MakeApplicant(id: 1);
            applicant.User = new User(100, "Test", "test@test.com", MakeValidCvXml());
            fakeRepo.AddApplicant(applicant);

            sut.ProcessCv(1);
            Assert.IsNotNull(fakeRepo.LastUpdated.CvGrade);
        }

        [TestMethod]
        public void ProcessCv_InvalidCv_LeavesGradeNull()
        {
            var applicant = MakeApplicant(id: 1);
            applicant.User = new User(100, "Test", "test@test.com", null);
            fakeRepo.AddApplicant(applicant);

            sut.ProcessCv(1);
            Assert.IsNull(fakeRepo.LastUpdated.CvGrade);
        }

        [TestMethod]
        public void ProcessCv_AnyApplicant_CallsRepositoryUpdate()
        {
            var applicant = MakeApplicant(id: 1);
            applicant.User = new User(100, "Test", "test@test.com", null);
            fakeRepo.AddApplicant(applicant);

            sut.ProcessCv(1);
            Assert.IsNotNull(fakeRepo.LastUpdated);
        }

        [TestMethod]
        public void UpdateApplicant_CallsRepositoryUpdate()
        {
            var applicant = MakeApplicant(id: 1);
            fakeRepo.AddApplicant(applicant);

            sut.UpdateApplicant(applicant);
            Assert.IsNotNull(fakeRepo.LastUpdated);
        }

        [TestMethod]
        public void UpdateApplicant_GradeBelowIndividualThreshold_SetsStatusRejected()
        {
            var applicant = MakeApplicant(id: 1);
            applicant.AppTestGrade = 4.0m;
            fakeRepo.AddApplicant(applicant);

            sut.UpdateApplicant(applicant);
            Assert.AreEqual("Rejected", fakeRepo.LastUpdated.ApplicationStatus);
        }


    }
}