using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OurApp.Core.Models;
using OurApp.Core.Repositories;
using OurApp.Core.Services;
using OurApp.Core.Validators;

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

        private readonly ICollaboratorsService collaboratorsService;
        private readonly IEventsService eventsService;
        private readonly ICompanyService companyService;
        private readonly SessionService sessionService;
        private readonly IEventValidator eventValidator;

        public List<Company> SelectedCollaborators { get; } = new List<Company>();

        [ObservableProperty] private string photo;

        [ObservableProperty] private string title;
        [ObservableProperty] private string titleError;
        private bool titleIsValid = false;

        [ObservableProperty] private string description;
        [ObservableProperty] private string descriptionError;
        private bool descriptionIsValid = true;

        [ObservableProperty] private DateTimeOffset? startDate = DateTimeOffset.Now;
        [ObservableProperty] private string startDateError;
        private bool startDateIsValid = true;

        [ObservableProperty] private DateTimeOffset? endDate = DateTimeOffset.Now;
        [ObservableProperty] private string endDateError;
        private bool endDateIsValid = true;

        [ObservableProperty] private string location;
        [ObservableProperty] private string locationError;
        private bool locationIsValid = false;

        [ObservableProperty] private string addError = EmptyStringValue;

        public bool IsEverythingValid => AddError == EmptyStringValue;
        public bool EventCreatedSuccessfully = false;

        /// <summary>
        /// Create Event View Model constructor
        /// </summary>
        /// <param name="eventsService"> events service </param>
        /// <param name="companyService"> company service </param>
        /// <param name="sessionService"> session service </param>
        public CreateEventViewModel(IEventsService eventsService, ICompanyService companyService, SessionService sessionService, ICollaboratorsService collaboratorsService, IEventValidator eventValidator)
        {
            this.eventsService = eventsService;
            this.companyService = companyService;
            this.sessionService = sessionService;
            this.collaboratorsService = collaboratorsService;
            this.eventValidator = eventValidator;
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

            string sourceCompanyName = sessionService.LoggedInUser.Name;
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
                collaboratorsService.AddCollaborator(eventOfCollaboration, invitedCompany, sessionService.LoggedInUser.CompanyId);
            }
        }

        /// <summary>
        /// Function that tries to create a new event
        /// </summary>
        [RelayCommand]
        public void CreateEvent()
        {
            if (!titleIsValid || !descriptionIsValid || !startDateIsValid || !endDateIsValid || !locationIsValid)
            {
                AddError = ErrorInputsInvalid;
                return;
            }

            try
            {
                AddError = EmptyStringValue;
                DateTime eventStartDateTime = StartDate.Value.DateTime;
                DateTime eventEndDateTime = EndDate.Value.DateTime;

                int hostCompanyId = sessionService.LoggedInUser.CompanyId;
                Event createdEvent = eventsService.AddEvent(Photo, Title, Description, eventStartDateTime, eventEndDateTime, Location, hostCompanyId, SelectedCollaborators.ToList());
                EventCreatedSuccessfully = true;

                AddAllCollaboratorsWhenEventCreated(createdEvent);
                SendInvitations();
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception);
                EventCreatedSuccessfully = false;
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
                if (eventValidator.ValidateEventTitle(Title))
                {
                    TitleError = EmptyStringValue;
                    titleIsValid = true;
                    return true;
                }
            }
            catch (Exception exception)
            {
                TitleError = exception.Message;
                titleIsValid = false;
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
                if (eventValidator.ValidateEventDescription(Description))
                {
                    DescriptionError = EmptyStringValue;
                    descriptionIsValid = true;
                    return true;
                }
            }
            catch (Exception exception)
            {
                DescriptionError = exception.Message;
                descriptionIsValid = false;
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
                if (eventValidator.ValidateEventLocation(Location))
                {
                    LocationError = EmptyStringValue;
                    locationIsValid = true;
                    return true;
                }
            }
            catch (Exception exception)
            {
                LocationError = exception.Message;
                locationIsValid = false;
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
                if (eventValidator.ValidateEventStartDate(StartDate))
                {
                    StartDateError = EmptyStringValue;
                    startDateIsValid = true;
                    return true;
                }
            }
            catch (Exception exception)
            {
                StartDateError = exception.Message;
                startDateIsValid = false;
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
                if (eventValidator.ValidateEventEndDate(EndDate))
                {
                    EndDateError = EmptyStringValue;
                    endDateIsValid = true;
                    return true;
                }
            }
            catch (Exception exception)
            {
                EndDateError = exception.Message;
                endDateIsValid = false;
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
                if (eventValidator.ValidateEventDatesChronologically(StartDate, EndDate))
                {
                    EndDateError = EmptyStringValue;
                    endDateIsValid = true;
                    return true;
                }
            }
            catch (Exception exception)
            {
                EndDateError = exception.Message;
                endDateIsValid = false;
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

            Company? companyToInvite = companyService.GetCompanyByName(companyName);
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