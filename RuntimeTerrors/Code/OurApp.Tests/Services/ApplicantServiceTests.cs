using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OurApp.Core.Models;
using OurApp.Core.Services;
using OurApp.Tests.Helpers;

namespace OurApp.Tests.Services
{
    [TestClass]
    public class ApplicantServiceTests
    {
        private const string StatusRejected = "Rejected";
        private const string StatusOnHold = "On Hold";
        private const string StatusAccepted = "Accepted";

        private const int ValidApplicantId = 1;
        private const int SecondApplicantId = 2;
        private const int InvalidApplicantId = 999;
        private const int RemovedApplicantId = 42;
        private const int ValidJobId = 10;
        private const int UserIdMultiplier = 100;
        private const int EmptyCount = 0;
        private const int TwoCount = 2;

        private const decimal GradeMax = 10.0m;
        private const decimal GradeExcellent = 9.0m;
        private const decimal GradePass = 8.0m;
        private const decimal GradeGood = 7.0m;
        private const decimal GradeMediocre = 6.0m;
        private const decimal GradeBorderlineFail = 5.5m;
        private const decimal GradeFail = 5.4m;
        private const decimal GradeLow = 4.0m;
        private const decimal GradeZero = 0.0m;

        private const int RequirementHigh = 80;
        private const int RequirementMedium = 60;

        private const string DefaultUserName = "Test User";
        private const string DefaultUserEmail = "test@test.com";
        private const string InvalidXmlText = "not xml at all";
        private const string EmptyString = "";

        private const string SkillCSharp = "c#";
        private const string SkillSql = "sql";
        private const string SkillPython = "python";
        private const string SkillJava = "java";
        private const string SkillReact = "react";
        private const string SkillDocker = "docker";

        private const string ValidCvXml = @"<CV>
                     <Name>Alice</Name>
                     <Email>alice@example.com</Email>
                     <Phone>0466428888</Phone>
                     <Skills>c# sql python</Skills>
                     <Interests>coding</Interests>
                     <Summary>Passionate software developer with many years of experience</Summary>
                     <Projects>Built enterprise applications using c# and sql</Projects>
            </CV>";

        private const string XmlMissingPhone = @"<CV>
                <Name>Alice Smith</Name>
                <Email>alice@example.com</Email>
                <Skills>c# sql</Skills>
                <Interests>coding</Interests>
                <Summary>Passionate software developer with many years of experience</Summary>
                <Projects>Built enterprise applications using c# and sql</Projects>
            </CV>";

        private const string XmlWhitespaceInterests = @"<CV>
                <Name>Alice Smith</Name>
                <Email>alice@example.com</Email>
                <Phone>0466428888</Phone>
                <Skills>c# sql python</Skills>
                <Interests>   </Interests>
                <Summary>Passionate software developer with many years of experience</Summary>
                <Projects>Built enterprise applications using c# and sql</Projects>
             </CV>";

        private const string XmlInvalidEmail = @"<CV>
                <Name>Alice Smith</Name>
                <Email>notanemail</Email>
                <Phone>0466428888</Phone>
                <Skills>c# sql</Skills>
                <Interests>coding</Interests>
                <Summary>Passionate software developer with many years of experience</Summary>
                <Projects>Built enterprise applications using c# and sql</Projects>
            </CV>";

        private const string XmlShortName = @"<CV>
                <Name>A</Name>
                <Email>alice@example.com</Email>
                <Phone>0466428888</Phone>
                <Skills>c# sql</Skills>
                <Interests>coding</Interests>
                <Summary>Passionate software developer with many years of experience</Summary>
                <Projects>Built enterprise applications using c# and sql</Projects>
            </CV>";

        private const string XmlShortSummary = @"<CV>
                <Name>Alice Smith</Name>
                <Email>alice@example.com</Email>
                <Phone>0466428888</Phone>
                <Skills>c# sql</Skills>
                <Interests>coding</Interests>
                <Summary>Too short</Summary>
                <Projects>Built enterprise applications using c# and sql</Projects>
            </CV>";

        private const string XmlShortProjects = @"<CV>
                <Name>Alice Smith</Name>
                <Email>alice@example.com</Email>
                <Phone>0466428888</Phone>
                <Skills>c# sql</Skills>
                <Interests>coding</Interests>
                <Summary>Passionate software developer with many years of experience</Summary>
                <Projects>Short</Projects>
            </CV>";

        private const string XmlShortPhone = @"<CV>
                <Name>Alice Smith</Name>
                <Email>alice@example.com</Email>
                <Phone>123</Phone>
                <Skills>c# sql</Skills>
                <Interests>coding</Interests>
                <Summary>Passionate software developer with many years of experience</Summary>
                <Projects>Built enterprise applications using c# and sql</Projects>
            </CV>";

        private const string XmlShortSkills = @"<CV>
                <Name>Alice Smith</Name>
                <Email>alice@example.com</Email>
                <Phone>0466428888</Phone>
                <Skills>ab</Skills>
                <Interests>coding</Interests>
                <Summary>Passionate software developer with many years of experience</Summary>
                <Projects>Built enterprise applications using c# and sql</Projects>
            </CV>";

        private const string XmlShortInterests = @"<CV>
                <Name>Alice Smith</Name>
                <Email>alice@example.com</Email>
                <Phone>0466428888</Phone>
                <Skills>c# sql</Skills>
                <Interests>ab</Interests>
                <Summary>Passionate software developer with many years of experience</Summary>
                <Projects>Built enterprise applications using c# and sql</Projects>
            </CV>";

        private const string XmlContactNumberTag = @"<CV>
                <Name>Alice Smith</Name>
                <Email>alice@example.com</Email>
                <ContactNumber>0466428888</ContactNumber>
                <Skills>c# sql python</Skills>
                <Interests>coding</Interests>
                <Summary>Passionate software developer with many years of experience</Summary>
                <Projects>Built enterprise applications using c# and sql</Projects>
            </CV>";

        private const string XmlEmailNoDot = @"<CV>
                <Name>Alice Smith</Name>
                <Email>alice@nodot</Email>
                <Phone>0466428888</Phone>
                <Skills>c# sql</Skills>
                <Interests>coding</Interests>
                <Summary>Passionate software developer with many years of experience</Summary>
                <Projects>Built enterprise applications using c# and sql</Projects>
            </CV>";

        private const string XmlEmailStartsWithAt = @"<CV>
                <Name>Alice Smith</Name>
                <Email>@example.com</Email>
                <Phone>0466428888</Phone>
                <Skills>c# sql</Skills>
                <Interests>coding</Interests>
                <Summary>Passionate software developer with many years of experience</Summary>
                <Projects>Built enterprise applications using c# and sql</Projects>
            </CV>";

        private const string XmlSynonymKeywords = @"<CV>
                <Name>Alice Smith</Name>
                <Email>alice@example.com</Email>
                <Phone>0466428888</Phone>
                <Skills>csharp sql dotnet</Skills>
                <Interests>coding</Interests>
                <Summary>Passionate software developer with many years of experience</Summary>
                <Projects>Built enterprise applications using csharp and sql</Projects>
            </CV>";

        private const string XmlRepeatedKeywords = @"<CV>
                <Name>Alice Smith</Name>
                <Email>alice@example.com</Email>
                <Phone>0466428888</Phone>
                <Skills>c# c# sql sql python python</Skills>
                <Interests>coding</Interests>
                <Summary>Passionate software developer with many years of experience in c# and sql</Summary>
                <Projects>Built enterprise applications using c# sql and python</Projects>
            </CV>";

        private const string XmlManyKeywords = @"<CV>
                <Name>Alice Smith</Name>
                <Email>alice@example.com</Email>
                <Phone>0466428888</Phone>
                <Skills>python python python python python sql sql sql sql sql java java java java java react react react react react docker docker docker docker docker</Skills>
                <Interests>python sql java react docker python sql java react docker python sql java react docker python sql java react docker python sql java react docker</Interests>
                <Summary>Passionate python sql java react docker developer with many years of experience in python sql java react docker development</Summary>
                <Projects>Built enterprise python sql java react docker applications using python sql java react docker technologies and frameworks</Projects>
            </CV>";

        private const string XmlMissingName = @"<CV>
                <Email>alice@example.com</Email>
                <Phone>0466428888</Phone>
                <Skills>c# sql</Skills>
                <Interests>coding</Interests>
                <Summary>Passionate software developer with many years of experience</Summary>
                <Projects>Built enterprise applications using c# and sql</Projects>
            </CV>";

        private const string XmlMissingEmail = @"<CV>
                <Name>Alice Smith</Name>
                <Phone>0466428888</Phone>
                <Skills>c# sql</Skills>
                <Interests>coding</Interests>
                <Summary>Passionate software developer with many years of experience</Summary>
                <Projects>Built enterprise applications using c# and sql</Projects>
            </CV>";

        private const string XmlMissingSkills = @"<CV>
                <Name>Alice Smith</Name>
                <Email>alice@example.com</Email>
                <Phone>0466428888</Phone>
                <Interests>coding</Interests>
                <Summary>Passionate software developer with many years of experience</Summary>
                <Projects>Built enterprise applications using c# and sql</Projects>
            </CV>";

        private const string XmlMissingSummary = @"<CV>
                <Name>Alice Smith</Name>
                <Email>alice@example.com</Email>
                <Phone>0466428888</Phone>
                <Skills>c# sql</Skills>
                <Interests>coding</Interests>
                <Projects>Built enterprise applications using c# and sql</Projects>
            </CV>";

        private const string XmlMissingProjects = @"<CV>
                <Name>Alice Smith</Name>
                <Email>alice@example.com</Email>
                <Phone>0466428888</Phone>
                <Skills>c# sql</Skills>
                <Interests>coding</Interests>
                <Summary>Passionate software developer with many years of experience</Summary>
            </CV>";

        private FakeApplicantRepository fakeRepo = null!;
        private ApplicantService sut = null!;

        [TestInitialize]
        public void Setup()
        {
            fakeRepo = new FakeApplicantRepository();
            sut = new ApplicantService(fakeRepo);
        }

        private static Applicant MakeApplicant(int id = ValidApplicantId)
        {
            return new Applicant
            {
                ApplicantId = id,
                Job = new JobPosting { JobId = ValidJobId },
                User = new User(id * UserIdMultiplier, DefaultUserName, DefaultUserEmail)
            };
        }

        [TestMethod]
        public void GetApplicant_ExistingId_ReturnsApplicant()
        {
            fakeRepo.AddApplicant(MakeApplicant(ValidApplicantId));
            var result = sut.GetApplicant(ValidApplicantId);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void GetApplicant_NonExistingId_ReturnsNull()
        {
            var result = sut.GetApplicant(InvalidApplicantId);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetApplicantsForJob_NullJob_ReturnsEmptyList()
        {
            var result = sut.GetApplicantsForJob(null!);
            Assert.AreEqual(EmptyCount, result.Count());
        }

        [TestMethod]
        public void GetApplicantsForJob_TwoApplicantsSameJob_ReturnsBoth()
        {
            var job = new JobPosting { JobId = ValidJobId };
            fakeRepo.AddApplicant(MakeApplicant(ValidApplicantId));
            fakeRepo.AddApplicant(MakeApplicant(SecondApplicantId));

            var result = sut.GetApplicantsForJob(job);
            Assert.AreEqual(TwoCount, result.Count());
        }

        [TestMethod]
        public void UpdateAppTestGrade_ExistingApplicant_StoresGrade()
        {
            fakeRepo.AddApplicant(MakeApplicant(ValidApplicantId));
            sut.UpdateAppTestGrade(ValidApplicantId, GradeGood);
            Assert.AreEqual(GradeGood, fakeRepo.LastUpdated?.AppTestGrade);
        }

        [TestMethod]
        public void UpdateAppTestGrade_ExistingApplicant_CallsRepositoryUpdate()
        {
            fakeRepo.AddApplicant(MakeApplicant(ValidApplicantId));
            sut.UpdateAppTestGrade(ValidApplicantId, GradeGood);
            Assert.IsNotNull(fakeRepo.LastUpdated);
        }

        [TestMethod]
        public void UpdateAppTestGrade_NonExistingApplicant_DoesNotCallRepositoryUpdate()
        {
            sut.UpdateAppTestGrade(InvalidApplicantId, GradeGood);
            Assert.IsNull(fakeRepo.LastUpdated);
        }

        [TestMethod]
        public void UpdateCompanyTestGrade_ExistingApplicant_StoresGrade()
        {
            fakeRepo.AddApplicant(MakeApplicant(ValidApplicantId));
            sut.UpdateCompanyTestGrade(ValidApplicantId, GradePass);
            Assert.AreEqual(GradePass, fakeRepo.LastUpdated?.CompanyTestGrade);
        }

        [TestMethod]
        public void UpdateCompanyTestGrade_NonExistingApplicant_DoesNotCallRepositoryUpdate()
        {
            sut.UpdateCompanyTestGrade(InvalidApplicantId, GradePass);
            Assert.IsNull(fakeRepo.LastUpdated);
        }

        [TestMethod]
        public void UpdateInterviewGrade_ExistingApplicant_StoresGrade()
        {
            fakeRepo.AddApplicant(MakeApplicant(ValidApplicantId));
            sut.UpdateInterviewGrade(ValidApplicantId, GradeExcellent);
            Assert.AreEqual(GradeExcellent, fakeRepo.LastUpdated?.InterviewGrade);
        }

        [TestMethod]
        public void UpdateInterviewGrade_NonExistingApplicant_DoesNotCallRepositoryUpdate()
        {
            sut.UpdateInterviewGrade(InvalidApplicantId, GradeExcellent);
            Assert.IsNull(fakeRepo.LastUpdated);
        }

        [TestMethod]
        public void UpdateAppTestGrade_GradeBelowIndividualThreshold_SetsStatusRejected()
        {
            fakeRepo.AddApplicant(MakeApplicant(ValidApplicantId));
            sut.UpdateAppTestGrade(ValidApplicantId, GradeFail);
            Assert.AreEqual(StatusRejected, fakeRepo.LastUpdated?.ApplicationStatus);
        }

        [TestMethod]
        public void UpdateAppTestGrade_GradeExactlyAtIndividualThreshold_SetsStatusRejected()
        {
            fakeRepo.AddApplicant(MakeApplicant(ValidApplicantId));
            sut.UpdateAppTestGrade(ValidApplicantId, GradeBorderlineFail);
            Assert.AreEqual(StatusRejected, fakeRepo.LastUpdated?.ApplicationStatus);
        }

        [TestMethod]
        public void UpdateAppTestGrade_GradeAboveIndividualThreshold_DoesNotReject()
        {
            fakeRepo.AddApplicant(MakeApplicant(ValidApplicantId));
            sut.UpdateAppTestGrade(ValidApplicantId, GradePass);
            Assert.AreNotEqual(StatusRejected, fakeRepo.LastUpdated?.ApplicationStatus);
        }

        [TestMethod]
        public void UpdateCompanyTestGrade_AverageBelowCollectiveThreshold_SetsStatusRejected()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            applicant.AppTestGrade = GradeMediocre;
            fakeRepo.AddApplicant(applicant);

            sut.UpdateCompanyTestGrade(ValidApplicantId, GradeBorderlineFail);
            Assert.AreEqual(StatusRejected, fakeRepo.LastUpdated?.ApplicationStatus);
        }

        [TestMethod]
        public void UpdateCompanyTestGrade_AverageExactlyAtCollectiveThresholdDoesNotReject()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            applicant.AppTestGrade = GradeGood;
            fakeRepo.AddApplicant(applicant);

            sut.UpdateCompanyTestGrade(ValidApplicantId, GradeGood);
            Assert.AreNotEqual(StatusRejected, fakeRepo.LastUpdated?.ApplicationStatus);
        }

        [TestMethod]
        public void UpdateInterviewGrade_ALlFourGradesPassingAndNoStatusSet_SetsStatusOnHold()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            applicant.AppTestGrade = GradePass;
            applicant.CvGrade = GradePass;
            applicant.CompanyTestGrade = GradePass;
            fakeRepo.AddApplicant(applicant);

            sut.UpdateInterviewGrade(ValidApplicantId, GradePass);
            Assert.AreEqual(StatusOnHold, fakeRepo.LastUpdated?.ApplicationStatus);
        }

        [TestMethod]
        public void UpdateInterviewGrade_AllFourGradesPassingButStatusAlreadySet_DoesNotOverwriteStatus()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            applicant.AppTestGrade = GradePass;
            applicant.CvGrade = GradePass;
            applicant.CompanyTestGrade = GradePass;
            applicant.ApplicationStatus = StatusAccepted;
            fakeRepo.AddApplicant(applicant);

            sut.UpdateInterviewGrade(ValidApplicantId, GradePass);
            Assert.AreEqual(StatusAccepted, fakeRepo.LastUpdated?.ApplicationStatus);
        }

        [TestMethod]
        public void RemoveApplicant_CallsRepositoryRemoveWithCorrectId()
        {
            sut.RemoveApplicant(RemovedApplicantId);
            Assert.AreEqual(RemovedApplicantId, fakeRepo.LastRemovedId);
        }

        [TestMethod]
        public void ScanCvXml_NullCv_ReturnsNull()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            applicant.User = new User(UserIdMultiplier, DefaultUserName, DefaultUserEmail, null);

            var result = sut.ScanCvXml(applicant);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ScanCvXml_InvalidXml_ReturnsNull()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            applicant.User = new User(UserIdMultiplier, DefaultUserName, DefaultUserEmail, InvalidXmlText);

            var result = sut.ScanCvXml(applicant);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void ScanCvXml_CvMissingRequiredField_ReturnsNull()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            applicant.User = new User(UserIdMultiplier, DefaultUserName, DefaultUserEmail, XmlMissingPhone);

            var result = sut.ScanCvXml(applicant);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ScanCvXml_ValidCv_ReturnsGradeGreaterThanZero()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            applicant.User = new User(UserIdMultiplier, DefaultUserName, DefaultUserEmail, ValidCvXml);

            var result = sut.ScanCvXml(applicant);
            Assert.IsTrue(result > GradeZero);
        }

        [TestMethod]
        public void ScanCvXml_ValidCv_ReturnsGradeNotExceedingTen()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            applicant.User = new User(UserIdMultiplier, DefaultUserName, DefaultUserEmail, ValidCvXml);

            var result = sut.ScanCvXml(applicant);
            Assert.IsTrue(result <= GradeMax);
        }

        [TestMethod]
        public void ScanCvXml_CvWithWhitespaceOnlyInterests_ReturnsBaseGrade()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            applicant.User = new User(UserIdMultiplier, DefaultUserName, DefaultUserEmail, XmlWhitespaceInterests);

            var result = sut.ScanCvXml(applicant);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ScanCvXml_CvWithInvalidEmail_ReturnsNull()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            applicant.User = new User(UserIdMultiplier, DefaultUserName, DefaultUserEmail, XmlInvalidEmail);

            var result = sut.ScanCvXml(applicant);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ScanCvXml_CvWithNameTooShort_ReturnsNull()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            applicant.User = new User(UserIdMultiplier, DefaultUserName, DefaultUserEmail, XmlShortName);

            var result = sut.ScanCvXml(applicant);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ScanCvXml_CvWithSummaryTooShort_ReturnsNull()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            applicant.User = new User(UserIdMultiplier, DefaultUserName, DefaultUserEmail, XmlShortSummary);

            var result = sut.ScanCvXml(applicant);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ScanCvXml_CvWithProjectsTooShort_ReturnsNull()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            applicant.User = new User(UserIdMultiplier, DefaultUserName, DefaultUserEmail, XmlShortProjects);

            var result = sut.ScanCvXml(applicant);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ScanCvXml_CvWithPhoneTooFewDigits_ReturnsNull()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            applicant.User = new User(UserIdMultiplier, DefaultUserName, DefaultUserEmail, XmlShortPhone);

            var result = sut.ScanCvXml(applicant);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ScanCvXml_CvWithSkillsTooShort_ReturnsNull()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            applicant.User = new User(UserIdMultiplier, DefaultUserName, DefaultUserEmail, XmlShortSkills);

            var result = sut.ScanCvXml(applicant);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ScanCvXml_CvWithInterestsTooShort_ReturnsNull()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            applicant.User = new User(UserIdMultiplier, DefaultUserName, DefaultUserEmail, XmlShortInterests);

            var result = sut.ScanCvXml(applicant);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ScanCvXml_ValidCvWithJobSkills_ReturnsGradeGreaterThanZero()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            applicant.Job = new JobPosting
            {
                JobId = ValidJobId,
                JobSkills = new List<JobSkill>
                {
                    new JobSkill { Skill = new Skill { SkillName = SkillCSharp }, RequiredPercentage = RequirementHigh },
                    new JobSkill { Skill = new Skill { SkillName = SkillSql }, RequiredPercentage = RequirementMedium }
                }
            };
            applicant.User = new User(UserIdMultiplier, DefaultUserName, DefaultUserEmail, ValidCvXml);

            var result = sut.ScanCvXml(applicant);

            Assert.IsTrue(result > GradeZero);
        }

        [TestMethod]
        public void ScanCvXml_CvWithContactNumberInsteadOfPhone_ReturnsGrade()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            applicant.User = new User(UserIdMultiplier, DefaultUserName, DefaultUserEmail, XmlContactNumberTag);

            var result = sut.ScanCvXml(applicant);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void ScanCvXml_CvWithEmailMissingDotInDomain_ReturnsNull()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            applicant.User = new User(UserIdMultiplier, DefaultUserName, DefaultUserEmail, XmlEmailNoDot);

            var result = sut.ScanCvXml(applicant);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ScanCvXml_CvWithEmailStartingWithAt_ReturnsNull()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            applicant.User = new User(UserIdMultiplier, DefaultUserName, DefaultUserEmail, XmlEmailStartsWithAt);

            var result = sut.ScanCvXml(applicant);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ScanCvXml_ValidCvWithSynonymKeyword_ReturnsGradeGreaterThanZero()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            applicant.User = new User(UserIdMultiplier, DefaultUserName, DefaultUserEmail, XmlSynonymKeywords);

            var result = sut.ScanCvXml(applicant);
            Assert.IsTrue(result > GradeZero);
        }

        [TestMethod]
        public void ScanCvXml_ValidCvWithRepeatedKeywords_ReturnsGradeGreaterThanZero()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            applicant.User = new User(UserIdMultiplier, DefaultUserName, DefaultUserEmail, XmlRepeatedKeywords);

            var result = sut.ScanCvXml(applicant);
            Assert.IsTrue(result > GradeZero);
        }

        [TestMethod]
        public void ScanCvXml_ValidCvWithManyKeywords_ReturnsGradeCappedAtTen()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            applicant.Job = new JobPosting
            {
                JobId = ValidJobId,
                JobSkills = new List<JobSkill>
                {
                    new JobSkill { Skill = new Skill { SkillName = SkillPython }, RequiredPercentage = RequirementHigh },
                    new JobSkill { Skill = new Skill { SkillName = SkillSql }, RequiredPercentage = RequirementHigh },
                    new JobSkill { Skill = new Skill { SkillName = SkillJava }, RequiredPercentage = RequirementHigh },
                    new JobSkill { Skill = new Skill { SkillName = SkillReact }, RequiredPercentage = RequirementHigh },
                    new JobSkill { Skill = new Skill { SkillName = SkillDocker }, RequiredPercentage = RequirementHigh }
                }
            };
            applicant.User = new User(UserIdMultiplier, DefaultUserName, DefaultUserEmail, XmlManyKeywords);

            var result = sut.ScanCvXml(applicant);
            Assert.AreEqual(GradeMax, result);
        }

        [TestMethod]
        public void ProcessCv_NonExistingApplicant_DoesNotCallRepositoryUpdate()
        {
            sut.ProcessCv(InvalidApplicantId);
            Assert.IsNull(fakeRepo.LastUpdated);
        }

        [TestMethod]
        public void ProcessCv_ValidCv_SetsCvGradeOnApplicant()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            applicant.User = new User(UserIdMultiplier, DefaultUserName, DefaultUserEmail, ValidCvXml);
            fakeRepo.AddApplicant(applicant);

            sut.ProcessCv(ValidApplicantId);
            Assert.IsNotNull(fakeRepo.LastUpdated?.CvGrade);
        }

        [TestMethod]
        public void ProcessCv_InvalidCv_LeavesGradeNull()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            applicant.User = new User(UserIdMultiplier, DefaultUserName, DefaultUserEmail, null);
            fakeRepo.AddApplicant(applicant);

            sut.ProcessCv(ValidApplicantId);
            Assert.IsNull(fakeRepo.LastUpdated?.CvGrade);
        }

        [TestMethod]
        public void ProcessCv_AnyApplicant_CallsRepositoryUpdate()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            applicant.User = new User(UserIdMultiplier, DefaultUserName, DefaultUserEmail, null);
            fakeRepo.AddApplicant(applicant);

            sut.ProcessCv(ValidApplicantId);
            Assert.IsNotNull(fakeRepo.LastUpdated);
        }

        [TestMethod]
        public void UpdateApplicant_CallsRepositoryUpdate()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            fakeRepo.AddApplicant(applicant);

            sut.UpdateApplicant(applicant);
            Assert.IsNotNull(fakeRepo.LastUpdated);
        }

        [TestMethod]
        public void UpdateApplicant_GradeBelowIndividualThreshold_SetsStatusRejected()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            applicant.AppTestGrade = GradeLow;
            fakeRepo.AddApplicant(applicant);

            sut.UpdateApplicant(applicant);
            Assert.AreEqual(StatusRejected, fakeRepo.LastUpdated?.ApplicationStatus);
        }

        [TestMethod]
        public void ScanCvXml_JobSkillWithNullSkillName_UsesDefaultKeywords()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            applicant.Job = new JobPosting
            {
                JobId = ValidJobId,
                JobSkills = new List<JobSkill>
                {
                    new JobSkill { Skill = new Skill { SkillName = EmptyString }, RequiredPercentage = RequirementHigh },
                    new JobSkill { Skill = null, RequiredPercentage = RequirementHigh }
                }
            };
            applicant.User = new User(UserIdMultiplier, DefaultUserName, DefaultUserEmail, ValidCvXml);

            var result = sut.ScanCvXml(applicant);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void ScanCvXml_ValidCvWithNullJobSkills_ReturnsGrade()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            applicant.Job = new JobPosting
            {
                JobId = ValidJobId,
                JobSkills = null
            };
            applicant.User = new User(UserIdMultiplier, DefaultUserName, DefaultUserEmail, ValidCvXml);

            var result = sut.ScanCvXml(applicant);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void ScanCvXml_CvWithMissingName_ReturnsNull()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            applicant.User = new User(UserIdMultiplier, DefaultUserName, DefaultUserEmail, XmlMissingName);

            var result = sut.ScanCvXml(applicant);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ScanCvXml_CvWithMissingEmail_ReturnsNull()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            applicant.User = new User(UserIdMultiplier, DefaultUserName, DefaultUserEmail, XmlMissingEmail);

            var result = sut.ScanCvXml(applicant);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ScanCvXml_CvWithMissingSkills_ReturnsNull()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            applicant.User = new User(UserIdMultiplier, DefaultUserName, DefaultUserEmail, XmlMissingSkills);

            var result = sut.ScanCvXml(applicant);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ScanCvXml_CvWithMissingSummary_ReturnsNull()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            applicant.User = new User(UserIdMultiplier, DefaultUserName, DefaultUserEmail, XmlMissingSummary);

            var result = sut.ScanCvXml(applicant);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ScanCvXml_CvWithMissingProjects_ReturnsNull()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            applicant.User = new User(UserIdMultiplier, DefaultUserName, DefaultUserEmail, XmlMissingProjects);

            var result = sut.ScanCvXml(applicant);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ScanCvXml_NullUser_ReturnsNull()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            applicant.User = null;

            var result = sut.ScanCvXml(applicant);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ScanCvXml_ValidCvWithNullJob_ReturnsGrade()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            applicant.Job = null;
            applicant.User = new User(UserIdMultiplier, DefaultUserName, DefaultUserEmail, ValidCvXml);

            var result = sut.ScanCvXml(applicant);
            Assert.IsNotNull(result);
        }
    }
}