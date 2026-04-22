using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using OurApp.Core.Models;
using OurApp.Core.Services;
using OurApp.Core.Repositories;

namespace OurApp.WinUI.ViewModels
{
    public class JobApplicantsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly IApplicantService _applicantService;
        private readonly SessionService? _sessionService;

        public JobPosting SelectedJob { get; private set; }

        public ObservableCollection<Applicant> Applicants { get; } = new ObservableCollection<Applicant>();
        public ObservableCollection<string> ApplicationStatusOptions { get; } = new ObservableCollection<string> 
        { 
            "Accepted", "Rejected", "On Hold", "Recommandation" 
        };

        private string _draftStatus;
        public string DraftStatus { get { return _draftStatus; } set { if (_draftStatus != value) { _draftStatus = value; OnPropertyChanged(nameof(DraftStatus)); } } }

        private string _draftAppTestGrade;
        public string DraftAppTestGrade { get { return _draftAppTestGrade; } set { if (_draftAppTestGrade != value) { _draftAppTestGrade = value; OnPropertyChanged(nameof(DraftAppTestGrade)); } } }

        private string _draftCvGrade;
        public string DraftCvGrade { get { return _draftCvGrade; } set { if (_draftCvGrade != value) { _draftCvGrade = value; OnPropertyChanged(nameof(DraftCvGrade)); } } }

        private string _draftCompanyTestGrade;
        public string DraftCompanyTestGrade { get { return _draftCompanyTestGrade; } set { if (_draftCompanyTestGrade != value) { _draftCompanyTestGrade = value; OnPropertyChanged(nameof(DraftCompanyTestGrade)); } } }

        private string _draftInterviewGrade;
        public string DraftInterviewGrade { get { return _draftInterviewGrade; } set { if (_draftInterviewGrade != value) { _draftInterviewGrade = value; OnPropertyChanged(nameof(DraftInterviewGrade)); } } }

        private string _cvScanErrorMessage = "";
        public string CvScanErrorMessage
        {
            get => _cvScanErrorMessage;
            private set
            {
                if (_cvScanErrorMessage != value)
                {
                    _cvScanErrorMessage = value ?? "";
                    OnPropertyChanged(nameof(CvScanErrorMessage));
                    OnPropertyChanged(nameof(CvScanErrorVisibility));
                }
            }
        }

        public Visibility CvScanErrorVisibility =>
            string.IsNullOrEmpty(_cvScanErrorMessage) ? Visibility.Collapsed : Visibility.Visible;

        private bool _isCvScanning;
        public bool IsCvScanning
        {
            get => _isCvScanning;
            private set
            {
                if (_isCvScanning != value)
                {
                    _isCvScanning = value;
                    OnPropertyChanged(nameof(IsCvScanning));
                    OnPropertyChanged(nameof(CanScanCv));
                }
            }
        }

        public bool CanScanCv => SelectedApplicant != null && !IsCvScanning;

        private Applicant _selectedApplicant;
        public Applicant SelectedApplicant
        {
            get { return _selectedApplicant; }
            set
            {
                if (_selectedApplicant != value)
                {
                    _selectedApplicant = value;
                    if (_selectedApplicant != null)
                    {
                        LoadDraft(_selectedApplicant);
                        DetailsVisibility = Visibility.Visible;
                        TableVisibility = Visibility.Collapsed;
                    }
                    else
                    {
                        DetailsVisibility = Visibility.Collapsed;
                        TableVisibility = Visibility.Visible;
                    }
                    OnPropertyChanged(nameof(SelectedApplicant));
                    OnPropertyChanged(nameof(CanScanCv));
                }
            }
        }

        private Visibility _tableVisibility = Visibility.Visible;
        public Visibility TableVisibility
        {
            get { return _tableVisibility; }
            set
            {
                if (_tableVisibility != value)
                {
                    _tableVisibility = value;
                    OnPropertyChanged(nameof(TableVisibility));
                }
            }
        }

        private Visibility _detailsVisibility = Visibility.Collapsed;
        public Visibility DetailsVisibility
        {
            get { return _detailsVisibility; }
            set
            {
                if (_detailsVisibility != value)
                {
                    _detailsVisibility = value;
                    OnPropertyChanged(nameof(DetailsVisibility));
                }
            }
        }

        public JobApplicantsViewModel(JobPosting job, SessionService sessionService)
        {
            SelectedJob = job;
            _sessionService = sessionService;
            IApplicantRepository repo = new ApplicantRepository();
            _applicantService = new ApplicantService(repo);

            LoadApplicants();
        }

        /// <summary>
        /// Sends the current draft application status to the applicant by email (same SMTP setup as event invitations).
        /// </summary>
        public async Task<(bool Ok, string Message)> SendStatusMailAsync()
        {
            if (_sessionService?.loggedInUser == null)
            {
                return (false, "No company session; cannot send mail.");
            }

            if (SelectedApplicant?.User == null)
            {
                return (false, "No applicant selected.");
            }

            var email = SelectedApplicant.User.Email?.Trim();
            if (string.IsNullOrEmpty(email))
            {
                return (false, "This applicant has no email address on file.");
            }

            var statusForMail = string.IsNullOrWhiteSpace(DraftStatus) ? "Pending" : DraftStatus;
            var jobTitle = SelectedJob?.JobTitle ?? "the position";
            var sourceName = _sessionService.loggedInUser.Name ?? "Our company";
            var applicantName = string.IsNullOrWhiteSpace(SelectedApplicant.User.Name) ? "Applicant" : SelectedApplicant.User.Name;

            const string fromPassword = "angxokbiqoyodwgm";
            var fromAddress = new MailAddress("carla.draghiciu@cnglsibiu.ro", sourceName);
            var toAddress = new MailAddress(email, applicantName);
            const string subject = "Application status update";
            string body =
                $"Hello {applicantName},\n\n" +
                $"Your application status for \"{jobTitle}\" at {sourceName} is: {statusForMail}.\n\n" +
                "If you have questions, please reply to this email.\n";

            try
            {
                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
                    Timeout = 60000
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body
                })
                {
                    await smtp.SendMailAsync(message).ConfigureAwait(true);
                }

                System.Diagnostics.Debug.WriteLine("Status email sent to applicant.");
                return (true, $"Status \"{statusForMail}\" was sent to {email}.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        private void LoadApplicants()
        {
            Applicants.Clear();
            if (SelectedJob != null)
            {
                try
                {
                    var applicants = _applicantService.GetApplicantsForJob(SelectedJob);
                    foreach (var applicant in applicants)
                    {
                        Applicants.Add(applicant);
                    }
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Database error: {ex.Message}");
                    Applicants.Add(new Applicant { ApplicantId = 999, User = new OurApp.Core.Models.User(1, "Mock Applicant (DB Error)", ""), ApplicationStatus = "On Hold", Job = SelectedJob });
                }
            }
        }

        private void LoadDraft(Applicant applicant)
        {
            CvScanErrorMessage = "";
            DraftStatus = applicant.ApplicationStatus;
            DraftAppTestGrade = applicant.AppTestGrade?.ToString() ?? "";
            DraftCvGrade = applicant.CvGrade?.ToString() ?? "";
            DraftCompanyTestGrade = applicant.CompanyTestGrade?.ToString() ?? "";
            DraftInterviewGrade = applicant.InterviewGrade?.ToString() ?? "";
        }

        public async Task ScanCvAsync()
        {
            if (SelectedApplicant == null || IsCvScanning)
            {
                return;
            }

            CvScanErrorMessage = "";
            IsCvScanning = true;
            try
            {
                var applicant = SelectedApplicant;
                decimal? grade = await Task.Run(() => _applicantService.ScanCvXml(applicant)).ConfigureAwait(true);

                if (grade.HasValue)
                {
                    DraftCvGrade = grade.Value.ToString(CultureInfo.InvariantCulture);
                    CvScanErrorMessage = "";
                }
                else
                {
                    CvScanErrorMessage = "Invalid CV";
                }
            }
            finally
            {
                IsCvScanning = false;
            }
        }

        public void SaveChanges()
        {
            if (SelectedApplicant == null) return;

            SelectedApplicant.ApplicationStatus = DraftStatus;
            
            if (decimal.TryParse(DraftAppTestGrade, out decimal t1)) SelectedApplicant.AppTestGrade = t1;
            else if (string.IsNullOrWhiteSpace(DraftAppTestGrade)) SelectedApplicant.AppTestGrade = null;

            if (decimal.TryParse(DraftCvGrade, out decimal t2)) SelectedApplicant.CvGrade = t2;
            else if (string.IsNullOrWhiteSpace(DraftCvGrade)) SelectedApplicant.CvGrade = null;

            if (decimal.TryParse(DraftCompanyTestGrade, out decimal t3)) SelectedApplicant.CompanyTestGrade = t3;
            else if (string.IsNullOrWhiteSpace(DraftCompanyTestGrade)) SelectedApplicant.CompanyTestGrade = null;

            if (decimal.TryParse(DraftInterviewGrade, out decimal t4)) SelectedApplicant.InterviewGrade = t4;
            else if (string.IsNullOrWhiteSpace(DraftInterviewGrade)) SelectedApplicant.InterviewGrade = null;

            _applicantService.UpdateApplicant(SelectedApplicant);
            
            int index = Applicants.IndexOf(SelectedApplicant);
            if (index >= 0)
            {
                Applicants[index] = SelectedApplicant;
            }

            GoBackFromDetails();
        }

        public void GoBackFromDetails()
        {
            SelectedApplicant = null; // This will trigger the setter to collapse details and show table
        }

        public void RemoveApplicant(Applicant applicant)
        {
            if (applicant != null)
            {
                _applicantService.RemoveApplicant(applicant.ApplicantId);
                Applicants.Remove(applicant);
                if (SelectedApplicant == applicant)
                {
                    GoBackFromDetails();
                }
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
