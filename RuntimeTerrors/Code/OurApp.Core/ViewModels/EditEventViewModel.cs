using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OurApp.Core.Models;
using OurApp.Core.Services;
using OurApp.Core.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OurApp.Core.ViewModels
{
    public partial class EditEventViewModel : ObservableObject
    {
        private readonly IEventsService eventsService;
        private readonly Event eventToEdit;
        public EventValidator eventValidator = new EventValidator();

        [ObservableProperty] private string photo;

        [ObservableProperty] private string title;
        [ObservableProperty] private string titleError;
        private bool titleIsValid = true;

        [ObservableProperty] private string description;
        [ObservableProperty] private string descriptionError;
        private bool descriptionIsValid = true;

        [ObservableProperty] private DateTimeOffset? startDate;
        [ObservableProperty] private string startDateError;
        private bool startDateIsValid = true;

        [ObservableProperty] private DateTimeOffset? endDate;
        [ObservableProperty] private string endDateError;
        private bool endDateIsValid = true;

        [ObservableProperty] private string location;
        [ObservableProperty] private string locationError;
        private bool locationIsValid = true;

        [ObservableProperty] private string addError = "";

        public bool isEverythingValid => (addError == "");
        public bool eventUpdatedSuccessfully = false;
        public bool eventDeletedSuccessfully = false;


        /// <summary>
        /// Edit Event View Model constructor which sets the textboxes' values to the event's
        /// </summary>
        /// <param name="eventsService"> events service </param>
        /// <param name="selectedEvent"> the selected event to be modified </param>
        public EditEventViewModel(IEventsService eventsService, Event selectedEvent)
        {
            this.eventsService = eventsService;
            this.eventToEdit = selectedEvent;

            this.title = selectedEvent.Title;
            this.description = selectedEvent.Description;
            this.startDate = new DateTimeOffset?(selectedEvent.StartDate);
            this.endDate = new DateTimeOffset?(selectedEvent.EndDate);
            this.location = selectedEvent.Location;
        }


        /// <summary>
        /// Function that tries to update an event
        /// </summary>
        [RelayCommand] public void EditEvent()
        {
            if (!titleIsValid || !descriptionIsValid || !startDateIsValid || !endDateIsValid || !locationIsValid)
            {
                AddError = "Please enter valid inputs before creating an event";
                return;
            }

            try
            {
                AddError = "";
                DateTime eventStartDateTime = startDate.Value.DateTime;
                DateTime eventEndDateTime = endDate.Value.DateTime;

                eventsService.UpdateEvent(eventToEdit.Id, Photo, Title, Description, eventStartDateTime, eventEndDateTime, Location);
                eventUpdatedSuccessfully = true;
            }
            catch (Exception)
            {
                eventUpdatedSuccessfully = false;
            }
        }

        /// <summary>
        /// Function that tries to delete an event
        /// </summary>
        [RelayCommand] public void DeleteEvent()
        {
            try
            {
                eventsService.DeleteEvent(eventToEdit);
                eventDeletedSuccessfully = true;
            }
            catch (Exception)
            {
                eventDeletedSuccessfully = false;
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
                if (eventValidator.IsEventTitleValid(Title))
                {
                    TitleError = "";
                    titleIsValid = true;
                    return true;
                }
            }
            catch (Exception ex)
            {
                TitleError = ex.Message;
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
                if (eventValidator.IsEventDescriptionValid(Description))
                {
                    DescriptionError = "";
                    descriptionIsValid = true;
                    return true;
                }
            }
            catch (Exception ex)
            {
                DescriptionError = ex.Message;
                descriptionIsValid = false;
            }
            return false;
        }


        /// <summary>
        /// Function that sets some flags, used in the View, if the event dates are cronologically valid
        /// </summary>
        /// <returns> true if the dates are in cronological order, false otherwise </returns>
        public bool ValidateDatesCronologity()
        {
            try
            {
                if (eventValidator.AreEventDatesCronologicallyValid(StartDate, EndDate))
                {
                    StartDateError = "";
                    EndDateError = "";
                    endDateIsValid = true;
                    startDateIsValid = true;
                    return true;
                }
            }
            catch (Exception ex)
            {
                StartDateError = ex.Message;
                EndDateError = ex.Message;
                endDateIsValid = false;
                startDateIsValid = false;
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
                if (eventValidator.IsEventLocationValid(Location))
                {
                    LocationError = "";
                    locationIsValid = true;
                    return true;
                }
            }
            catch (Exception ex)
            {
                LocationError = ex.Message;
                locationIsValid = false;
            }
            return false;
        }
    }
}
