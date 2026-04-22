using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OurApp.Core.Models;
using OurApp.Core.Repositories;
using OurApp.Core.Services;
using OurApp.Core.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;

namespace OurApp.Core.ViewModels
{
    public partial class CreateEventViewModel : ObservableObject
    {
        private const string AdminEmailAddress = "carla.draghiciu@cnglsibiu.ro";
        private const string AdminEmailPassword = "[REDACTED_PASSWORD]";
        private const string SmtpHostAddress = "smtp.gmail.com";
        private const int SmtpHostPort = 587;
        private const int SmtpTimeoutMilliseconds = 60000;
        private const string EmailSubject = "Event Invitation";
        private const string EmailSentDebugMessage = "Email sent!";
        private const string MissingEmailDebugMessage = "Company has no email";

        private const string EmptyStringValue = "";
        private const string ErrorInputsInvalid = "Please enter valid inputs before creating an event";
        private const string ErrorCompanyNameMissing = "Please enter a company name.";
        private const string ErrorCompanyNotFound = "Company was not found.";
        private const string ErrorCompanyAlreadyAdded = "Company is already added as a collaborator.";

        private readonly ICollaboratorsService _collaboratorsService;
        private readonly IEventsService _eventsService;
        private readonly ICompanyService _companyService;
        private readonly SessionService _sessionService;
        private readonly IEventValidator _eventValidator;

        public List<Company> SelectedCollaborators { get; } = new List<Company>();

        [ObservableProperty] private string _photo;

        [ObservableProperty] private string _title;
        [ObservableProperty] private string _titleError;
        private bool _titleIsValid = false;

        [ObservableProperty] private string _description;
        [ObservableProperty] private string _descriptionError;
        private bool _descriptionIsValid = true;

        [ObservableProperty] private DateTimeOffset? _startDate = DateTimeOffset.Now;
        [ObservableProperty] private string _startDateError;
        private bool _startDateIsValid = true;

        [ObservableProperty] private DateTimeOffset? _endDate = DateTimeOffset.Now;
        [ObservableProperty] private string _endDateError;
        private bool _endDateIsValid = true;

        [ObservableProperty] private string _location;
        [ObservableProperty] private string _locationError;
        private bool _locationIsValid = false;

        [ObservableProperty] private string _addError = EmptyStringValue;

        public bool isEverythingValid => (AddError == EmptyStringValue);
        public bool eventCreatedSuccessfully = false;


        /// <summary>
        /// Create Event View Model constructor
        /// </summary>
        /// <param name="eventsService"> events service </param>
        /// <param name="companyService"> company service </param>
        /// <param name="sessionService"> session service </param>
        public CreateEventViewModel(IEventsService eventsService, ICompanyService companyService, SessionService sessionService, ICollaboratorsService collaboratorsService, IEventValidator eventValidator)
        {
            _eventsService = eventsService;
            _companyService = companyService;
            _sessionService = sessionService;
            _collaboratorsService = collaboratorsService;
            _eventValidator = eventValidator;
        }


        /// <summary>
        /// Function that sends an email to a company
        /// </summary>
        /// <param name="destinationCompany"> company to send email to </param>
        private async void SendMailToCompany(Company destinationCompany)
        {
            if (string.IsNullOrEmpty(destinationCompany.Email))
            {
                System.Diagnostics.Debug.WriteLine(MissingEmailDebugMessage);
                return;
            }

            string sourceCompanyName = _sessionService.loggedInUser.Name;
            var fromAddress = new MailAddress(AdminEmailAddress, sourceCompanyName);

            var toAddress = new MailAddress(destinationCompany.Email, destinationCompany.Name);
            string emailBodyText = $"Hello, you have been invited to collaborate on {sourceCompanyName}'s event: {Title}\nPlease reply to this email within 7 days from receiving it, if you would like to accept the invitation.";

            var smtpClient = new SmtpClient
            {
                Host = SmtpHostAddress,
                Port = SmtpHostPort,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(fromAddress.Address, AdminEmailPassword),
                Timeout = SmtpTimeoutMilliseconds
            };

            using (var mailMessage = new MailMessage(fromAddress, toAddress)
            {
                Subject = EmailSubject,
                Body = emailBodyText
            })
            {
                await smtpClient.SendMailAsync(mailMessage);
            }

            System.Diagnostics.Debug.WriteLine(EmailSentDebugMessage);
        }

        /// <summary>
        /// Function that sends the invitations to all the selected companies, 
        /// after the user creates the event
        /// </summary>
        private void SendInvitations()
        {
            foreach (Company invitedCompany in this.SelectedCollaborators)
            {
                this.SendMailToCompany(invitedCompany);
            }
        }

        private void AddAllCollaboratorsWhenEventCreated(Event eventOfCollaboration)
        {
            foreach (Company invitedCompany in SelectedCollaborators)
            {
                _collaboratorsService.AddCollaborator(eventOfCollaboration, invitedCompany, _sessionService.loggedInUser.CompanyId);
            }
        }


        /// <summary>
        /// Function that tries to create a new event
        /// </summary>
        [RelayCommand]
        public void CreateEvent()
        {
            if (!_titleIsValid || !_descriptionIsValid || !_startDateIsValid || !_endDateIsValid || !_locationIsValid)
            {
                AddError = ErrorInputsInvalid;
                return;
            }

            try
            {
                AddError = EmptyStringValue;
                DateTime eventStartDateTime = StartDate.Value.DateTime;
                DateTime eventEndDateTime = EndDate.Value.DateTime;

                int hostCompanyId = _sessionService.loggedInUser.CompanyId;
                Event createdEvent = _eventsService.AddEvent(Photo, Title, Description, eventStartDateTime, eventEndDateTime, Location, hostCompanyId, SelectedCollaborators.ToList());
                eventCreatedSuccessfully = true;

                AddAllCollaboratorsWhenEventCreated(createdEvent);
                SendInvitations();
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception);
                eventCreatedSuccessfully = false;
            }
        }


        /// <summary>
        /// Function that sets some flags, used in the View, if the event title is valid
        /// </summary>
        /// <returns> true if the title is valid, false otherwise </returns>
        public bool ValidateTitle()
        {
            try
            {
                if (_eventValidator.ValidateEventTitle(Title))
                {
                    TitleError = EmptyStringValue;
                    _titleIsValid = true;
                    return true;
                }
            }
            catch (Exception exception)
            {
                TitleError = exception.Message;
                _titleIsValid = false;
            }
            return false;
        }


        /// <summary>
        /// Function that sets some flags, used in the View, if the event description is valid
        /// </summary>
        /// <returns> true if the description is valid, false otherwise </returns>
        public bool ValidateDescription()
        {
            try
            {
                if (_eventValidator.ValidateEventDescription(Description))
                {
                    DescriptionError = EmptyStringValue;
                    _descriptionIsValid = true;
                    return true;
                }
            }
            catch (Exception exception)
            {
                DescriptionError = exception.Message;
                _descriptionIsValid = false;
            }
            return false;
        }

        /// <summary>
        /// Function that sets some flags, used in the View, if the event location is valid
        /// </summary>
        /// <returns> true if the location is valid, false otherwise </returns>
        public bool ValidateLocation()
        {
            try
            {
                if (_eventValidator.ValidateEventLocation(Location))
                {
                    LocationError = EmptyStringValue;
                    _locationIsValid = true;
                    return true;
                }
            }
            catch (Exception exception)
            {
                LocationError = exception.Message;
                _locationIsValid = false;
            }
            return false;
        }

        /// <summary>
        /// Function that sets some flags, used in the View, if the event starting date is valid
        /// </summary>
        /// <returns> true if the starting date is valid, false otherwise </returns>
        public bool ValidateStartDate()
        {
            try
            {
                if (_eventValidator.ValidateEventStartDate(StartDate))
                {
                    StartDateError = EmptyStringValue;
                    _startDateIsValid = true;
                    return true;
                }
            }
            catch (Exception exception)
            {
                StartDateError = exception.Message;
                _startDateIsValid = false;
            }
            return false;
        }


        /// <summary>
        /// Function that sets some flags, used in the View, if the event ending date is valid
        /// </summary>
        /// <returns> true if the ending date is valid, false otherwise </returns>
        public bool ValidateEndDate()
        {
            try
            {
                if (_eventValidator.ValidateEventEndDate(EndDate))
                {
                    EndDateError = EmptyStringValue;
                    _endDateIsValid = true;
                    return true;
                }
            }
            catch (Exception exception)
            {
                EndDateError = exception.Message;
                _endDateIsValid = false;
            }
            return false;
        }

        /// <summary>
        /// Function that sets some flags, used in the View, if the event dates are cronologically valid
        /// </summary>
        /// <returns> true if the dates are valid, false otherwise </returns>
        public bool ValidateDatesCronologity()
        {
            try
            {
                if (_eventValidator.ValidateEventDatesChronologically(StartDate, EndDate))
                {
                    EndDateError = EmptyStringValue;
                    _endDateIsValid = true;
                    return true;
                }
            }
            catch (Exception exception)
            {
                EndDateError = exception.Message;
                _endDateIsValid = false;
            }
            return false;
        }

        /// <summary>
        /// Function that tries to add a collaborator to the event
        /// </summary>
        /// <param name="companyName"> the invited company's name </param>
        /// <param name="errorMessage"> the error message returned </param>
        /// <returns> true if the company name exists, false otherwise </returns>
        public bool TryAddCollaboratorByName(string companyName, out string errorMessage)
        {
            errorMessage = EmptyStringValue;

            if (string.IsNullOrWhiteSpace(companyName))
            {
                errorMessage = ErrorCompanyNameMissing;
                return false;
            }

            Company? companyToInvite = _companyService.GetCompanyByName(companyName);
            if (companyToInvite == null)
            {
                errorMessage = ErrorCompanyNotFound;
                return false;
            }

            if (SelectedCollaborators.Any(collaborator => string.Equals(collaborator.Name, companyToInvite.Name, StringComparison.OrdinalIgnoreCase)))
            {
                errorMessage = ErrorCompanyAlreadyAdded;
                return false;
            }

            SelectedCollaborators.Add(companyToInvite);
            return true;
        }

        /// <summary>
        /// Function that removes a collaborator
        /// </summary>
        /// <param name="companyName"> the name of the company to be removed from the collaborators list </param>
        public void RemoveCollaboratorByName(string companyName)
        {
            foreach (Company selectedCompany in SelectedCollaborators.ToList())
            {
                if (selectedCompany.Name == companyName)
                {
                    SelectedCollaborators.Remove(selectedCompany);
                }
            }
        }
    }
}