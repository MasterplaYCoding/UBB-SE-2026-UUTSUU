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
        private const string EmptyStringValue = "";
        private const string ErrorInputsInvalid = "Please enter valid inputs before creating an event";

        private readonly IEventsService _eventsService;
        private readonly Event _eventToEdit;
        private readonly IEventValidator _eventValidator;

        [ObservableProperty] private string _photo = EmptyStringValue;

        [ObservableProperty] private string _title = EmptyStringValue;
        [ObservableProperty] private string _titleError = EmptyStringValue;
        private bool _titleIsValid = true;

        [ObservableProperty] private string _description = EmptyStringValue;
        [ObservableProperty] private string _descriptionError = EmptyStringValue;
        private bool _descriptionIsValid = true;

        [ObservableProperty] private DateTimeOffset? _startDate;
        [ObservableProperty] private string _startDateError = EmptyStringValue;
        private bool _startDateIsValid = true;

        [ObservableProperty] private DateTimeOffset? _endDate;
        [ObservableProperty] private string _endDateError = EmptyStringValue;
        private bool _endDateIsValid = true;

        [ObservableProperty] private string _location = EmptyStringValue;
        [ObservableProperty] private string _locationError = EmptyStringValue;
        private bool _locationIsValid = true;

        [ObservableProperty] private string _addError = EmptyStringValue;

        public bool isEverythingValid => (AddError == EmptyStringValue);
        public bool eventUpdatedSuccessfully = false;
        public bool eventDeletedSuccessfully = false;


        /// <summary>
        /// Edit Event View Model constructor which sets the textboxes' values to the event's
        /// </summary>
        /// <param name="eventsService"> events service </param>
        /// <param name="selectedEvent"> the selected event to be modified </param>
        /// <param name="eventValidator"> event validator service </param>
        public EditEventViewModel(IEventsService eventsService, Event selectedEvent, IEventValidator eventValidator)
        {
            _eventsService = eventsService;
            _eventToEdit = selectedEvent;
            _eventValidator = eventValidator;

            _title = selectedEvent.Title;
            _description = selectedEvent.Description;
            _startDate = selectedEvent.StartDate;
            _endDate = selectedEvent.EndDate;
            _location = selectedEvent.Location;
        }


        /// <summary>
        /// Function that tries to update an event
        /// </summary>
        [RelayCommand]
        public void EditEvent()
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

                _eventsService.UpdateEvent(_eventToEdit.Id, Photo, Title, Description, eventStartDateTime, eventEndDateTime, Location);
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
        [RelayCommand]
        public void DeleteEvent()
        {
            try
            {
                _eventsService.DeleteEvent(_eventToEdit);
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
        /// Function that sets some flags, used in the View, if the event dates are cronologically valid
        /// </summary>
        /// <returns> true if the dates are in cronological order, false otherwise </returns>
        public bool ValidateDatesCronologity()
        {
            try
            {
                if (_eventValidator.ValidateEventDatesChronologically(StartDate, EndDate))
                {
                    StartDateError = EmptyStringValue;
                    EndDateError = EmptyStringValue;
                    _endDateIsValid = true;
                    _startDateIsValid = true;
                    return true;
                }
            }
            catch (Exception exception)
            {
                StartDateError = exception.Message;
                EndDateError = exception.Message;
                _endDateIsValid = false;
                _startDateIsValid = false;
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
    }
}