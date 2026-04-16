using System;
using System.Collections.Generic;
using System.Xml.Linq;
using OurApp.Core.Models;
using OurApp.Core.Repositories;
using System.Linq;

namespace OurApp.Core.Services
{
    public class ApplicantService : IApplicantService
    {
        private readonly IApplicantRepository _repository;
        private const decimal PassIndividual = 5.5m;
        private const decimal PassCollective = 7.0m;

        public ApplicantService(IApplicantRepository repository)
        {
            _repository = repository;
        }

        public IEnumerable<Applicant> GetApplicantsForJob(JobPosting job)
        {
            return _repository.GetApplicantsByJob(job);
        }

        public Applicant GetApplicant(int applicantId)
        {
            return _repository.GetApplicantById(applicantId);
        }

        

        public void UpdateCompanyTestGrade(int applicantId, decimal grade)
        {
            Applicant applicant = _repository.GetApplicantById(applicantId);
            if (applicant != null)
            {
                applicant.CompanyTestGrade = grade;
                EvaluateApplicantStatus(applicant);
                _repository.UpdateApplicant(applicant);
            }
        }

        public void UpdateInterviewGrade(int applicantId, decimal grade)
        {
            Applicant applicant = _repository.GetApplicantById(applicantId);
            if (applicant != null)
            {
                applicant.InterviewGrade = grade;
                EvaluateApplicantStatus(applicant);
                _repository.UpdateApplicant(applicant);
            }
        }

        private static bool TryGetTrimmedElement(XDocument doc, string localName, out string value)
        {
            value = "";
            var el = doc.Descendants(localName).FirstOrDefault();
            if (el == null || string.IsNullOrWhiteSpace(el.Value))
            {
                return false;
            }

            value = el.Value.Trim();
            return true;
        }

        private static bool LooksLikeEmail(string email)
        {
            var at = email.IndexOf('@', StringComparison.Ordinal);
            if (at <= 0 || at >= email.Length - 1)
            {
                return false;
            }

            var domain = email[(at + 1)..];
            return domain.Contains('.', StringComparison.Ordinal);
        }

        private static bool HasMeaningfulSkillsText(string skillsText)
        {
            if (skillsText.Length < 3)
            {
                return false;
            }

            var parts = skillsText.Split(new[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length >= 1;
        }

        private static bool HasMeaningfulProjectsText(string projectsText)
        {
            if (projectsText.Length < 15)
            {
                return false;
            }

            var parts = projectsText.Split(new[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length >= 2;
        }

        private static bool HasPlausiblePhone(string phoneText)
        {
            var digitCount = phoneText.Count(char.IsDigit);
            return digitCount >= 8;
        }

        private bool IsCvValid(string? cvXml)
        {
            if (string.IsNullOrWhiteSpace(cvXml))
            {
                return false;
            }

            try
            {
                XDocument doc = XDocument.Parse(cvXml);

                if (!TryGetTrimmedElement(doc, "Name", out var name) || name.Length < 2)
                {
                    return false;
                }

                if (!TryGetTrimmedElement(doc, "Email", out var email) || !LooksLikeEmail(email))
                {
                    return false;
                }

                if (!TryGetTrimmedElement(doc, "Skills", out var skills) || !HasMeaningfulSkillsText(skills))
                {
                    return false;
                }

                if (!TryGetTrimmedElement(doc, "Interests", out var interests) || interests.Length < 3)
                {
                    return false;
                }

                string phone;
                if (!TryGetTrimmedElement(doc, "Phone", out phone) && !TryGetTrimmedElement(doc, "ContactNumber", out phone))
                {
                    return false;
                }

                if (!HasPlausiblePhone(phone))
                {
                    return false;
                }

                if (!TryGetTrimmedElement(doc, "Summary", out var summary) || summary.Length < 20)
                {
                    return false;
                }

                if (!TryGetTrimmedElement(doc, "Projects", out var projects) || !HasMeaningfulProjectsText(projects))
                {
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private List<string> TokenizeAndClean(string text)
        {
            List<string> stopWords = new List<string> { "the", "and", "is", "a", "an", "in", "to", "of", "for" };
            
            string cleanText = "";
            foreach (char c in text)
            {
                if (char.IsPunctuation(c))
                {
                    cleanText += " ";
                }
                else
                {
                    cleanText += c;
                }
            }

            string[] rawWords = cleanText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> finalWords = new List<string>();

            foreach (string word in rawWords)
            {
                string lowerWord = word.ToLower();
                if (stopWords.Contains(lowerWord) == false)
                {
                    finalWords.Add(lowerWord);
                }
            }

            return finalWords;
        }

        private List<string> ApplySynonyms(List<string> words)
        {
            Dictionary<string, string> map = new Dictionary<string, string>();
            map.Add("csharp", "c#");
            map.Add("js", "javascript");
            map.Add("reactjs", "react");
            map.Add("dot net", ".net");
            map.Add("dotnet", ".net");
            
            List<string> newWords = new List<string>();

            foreach (string word in words)
            {
                if (map.ContainsKey(word))
                {
                    newWords.Add(map[word]);
                }
                else
                {
                    newWords.Add(word);
                }
            }

            return newWords;
        }

        private decimal CalculateTfIdfGrade(List<string> words, List<string> importantKeywords, decimal sectionWeight)
        {
            
            Dictionary<string, int> termFrequencies = new Dictionary<string, int>();

            foreach (string word in words)
            {
                if (importantKeywords.Contains(word))
                {
                    if (termFrequencies.ContainsKey(word))
                    {
                        termFrequencies[word] += 1;
                    }
                    else
                    {
                        termFrequencies[word] = 1;
                    }
                }
            }

            decimal score = 0;
            foreach (var keywordAppearance in termFrequencies)
            {
                int count = keywordAppearance.Value;
                decimal wordScore = 0;
                decimal currentPointValue = 0.5m;

                for (int i = 0; i < count; i++)
                {
                    wordScore += currentPointValue;

                    
                    currentPointValue = currentPointValue * 0.7m;
                }

                score += wordScore;
            }
                
            return score * sectionWeight;
        }

        private decimal ScoreCvSection(XDocument doc, string elementName, List<string> expectedKeywords, decimal sectionWeight)
        {
            var node = doc.Descendants(elementName).FirstOrDefault();
            if (node == null || string.IsNullOrWhiteSpace(node.Value))
            {
                return 0m;
            }

            List<string> words = TokenizeAndClean(node.Value);
            words = ApplySynonyms(words);
            return CalculateTfIdfGrade(words, expectedKeywords, sectionWeight);
        }

        public void ProcessCv(int applicantId)
        {
            Applicant applicant = _repository.GetApplicantById(applicantId);
            if (applicant == null)
            {
                return;
            }

            // If the CV XML is invalid, grade remains null
            decimal? cvGrade = ScanCvXml(applicant);
            if (cvGrade != null)
            {
                applicant.CvGrade = cvGrade;
            }

            EvaluateApplicantStatus(applicant);
            _repository.UpdateApplicant(applicant);
        }

        public void UpdateAppTestGrade(int applicantId, decimal grade)
        {
            Applicant applicant = _repository.GetApplicantById(applicantId);
            if (applicant != null)
            {
                applicant.AppTestGrade = grade;
                EvaluateApplicantStatus(applicant);
                _repository.UpdateApplicant(applicant);
            }
        }

        public decimal? ScanCvXml(Applicant applicant)
        {
            var cvXml = applicant.User?.CvXml;
            if (IsCvValid(cvXml) == false)
            {
                return null;
            }

            List<string> expectedKeywords = new List<string>();
            if (applicant.Job != null && applicant.Job.JobSkills != null)
            {
                foreach (var js in applicant.Job.JobSkills)
                {
                    if (js.Skill != null && !string.IsNullOrWhiteSpace(js.Skill.SkillName))
                    {
                        expectedKeywords.Add(js.Skill.SkillName.ToLower());
                    }
                }
            }

            if (expectedKeywords.Count == 0)
            {
                expectedKeywords = new List<string> { "c#", "java", "sql", "react", "agile", "javascript", ".net", "python", "docker", "azure" };
            }

            XDocument doc = XDocument.Parse(cvXml!);
            decimal totalGrade = 3.5m;
            totalGrade += ScoreCvSection(doc, "Skills", expectedKeywords, 1.35m);
            totalGrade += ScoreCvSection(doc, "Interests", expectedKeywords, 0.55m);
            totalGrade += ScoreCvSection(doc, "Summary", expectedKeywords, 1.15m);
            totalGrade += ScoreCvSection(doc, "Projects", expectedKeywords, 1.35m);

            if (totalGrade > 10.0m)
            {
                return 10.0m;
            }
            
            return totalGrade;
        }

        public void UpdateApplicant(Applicant applicant)
        {
            EvaluateApplicantStatus(applicant);
            _repository.UpdateApplicant(applicant);
        }

        public void RemoveApplicant(int applicantId)
        {
            _repository.RemoveApplicant(applicantId);
        }

        private void EvaluateApplicantStatus(Applicant applicant)
        {
            List<decimal> nonNullGrades = new List<decimal>();

            if (applicant.AppTestGrade != null) { nonNullGrades.Add(applicant.AppTestGrade.Value); }
            if (applicant.CvGrade != null) { nonNullGrades.Add(applicant.CvGrade.Value); }
            if (applicant.CompanyTestGrade != null) { nonNullGrades.Add(applicant.CompanyTestGrade.Value); }
            if (applicant.InterviewGrade != null) {nonNullGrades.Add(applicant.InterviewGrade.Value); }

            foreach (decimal grade in nonNullGrades)
            {
                if (grade < PassIndividual)
                {
                    applicant.ApplicationStatus = "Rejected";
                    return;
                }
            }

            if (nonNullGrades.Count > 0)
            {
                decimal sum = 0;
                foreach (decimal grade in nonNullGrades)
                {
                    sum += grade;
                }
                decimal average = sum / nonNullGrades.Count;
                
                if (average < PassCollective)
                {
                    applicant.ApplicationStatus = "Rejected";
                    return;
                }
            }

            if (nonNullGrades.Count == 4 && string.IsNullOrEmpty(applicant.ApplicationStatus))
            {
                applicant.ApplicationStatus = "On Hold";
            }
        }
    }
}
