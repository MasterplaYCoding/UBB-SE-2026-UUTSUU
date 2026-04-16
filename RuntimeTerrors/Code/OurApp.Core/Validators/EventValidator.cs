using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OurApp.Core.Validators
{
    public class EventValidator
    {
        /// <summary>
        /// Function that checks if the event's title is valid
        /// </summary>
        /// <param name="eventTitle"> the title of the event </param>
        /// <returns> true if title is valid </returns>
        /// <exception cref="Exception"> throws exception if title is not valid </exception>
        public bool IsEventTitleValid(string eventTitle)
        {
            if (eventTitle.Length == 0)
            {
                throw new Exception("Title is mandatory");
            }
            if (eventTitle.Length > 200)
            {
                throw new Exception("Title is too long");
            }
            return true;
        }

        /// <summary>
        /// Function that checks if the event's description is valid
        /// </summary>
        /// <param name="eventDescription"> the description of the event </param>
        /// <returns> true if the description is valid </returns>
        /// <exception cref="Exception"> throws exception if description is not valid </exception>
        public bool IsEventDescriptionValid(string eventDescription)
        {
            if (eventDescription.Length > 2000)
            {
                throw new Exception("Description is too long");
            }
            return true;
        }


        /// <summary>
        /// Function that checks if the event's location is valid
        /// </summary>
        /// <param name="eventLocation"> the location of the event </param>
        /// <returns> true if the location is valid </returns>
        /// <exception cref="Exception"> throws exception if location is not valid </exception>
        public bool IsEventLocationValid(string eventLocation)
        {
            if (eventLocation.Length == 0)
            {
                throw new Exception("Location is mandatory");
            }
            if (eventLocation.Length > 300)
            {
                throw new Exception("Location is too long");
            }
            return true;
        }

        /// <summary>
        /// Function that checks if the event's starting date is valid
        /// </summary>
        /// <param name="eventStartDate"> the starting date of the event </param>
        /// <returns> true if starting date is valid </returns>
        /// <exception cref="Exception"> throws exception if starting date is not valid </exception>
        public bool IsEventStartDateValid(DateTimeOffset? eventStartDate)
        {
            if (eventStartDate == null)
            {
                throw new Exception("Starting date is mandatory");
            }
            if (eventStartDate < DateTimeOffset.Now)
            {
                throw new Exception("Event must start after creation");
            }
            return true;
        }

        /// <summary>
        /// Function that checks if the event's ending date is valid
        /// </summary>
        /// <param name="eventEndDate"> the ending date of the event </param>
        /// <returns> true if ending date is valid </returns>
        /// <exception cref="Exception"> throws exception if ending date is not valid </exception>
        public bool IsEventEndDateValid(DateTimeOffset? eventEndDate)
        {
            if (eventEndDate == null)
            {
                throw new Exception("Ending date is mandatory");
            }
            if (eventEndDate < DateTimeOffset.Now)
            {
                throw new Exception("Event must end after creation");
            }
            return true;
        }

        /// <summary>
        /// Function that checks if the event's dates are cronologically correct
        /// </summary>
        /// <param name="eventStartDate"> the starting date of the event </param>
        /// <param name="eventEndDate"> the ending date of the event </param>
        /// <returns> true if dates are in cronological order </returns>
        /// <exception cref="Exception">  throws exception if dates are not in cronological order </exception>
        public bool AreEventDatesCronologicallyValid(DateTimeOffset? eventStartDate, DateTimeOffset? eventEndDate)
        {
            if (eventStartDate > eventEndDate)
            {
                throw new Exception("Event must begin before ending");
            }
            return true;
        }
    }
}
