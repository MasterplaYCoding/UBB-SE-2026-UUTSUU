using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OurApp.Core.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Photo { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Location { get; set; }
        public int HostID { get; set; }
        public List<Company> Collaborators { get; set; }

        /// <summary>
        /// Event constructor
        /// </summary>
        /// <param name="eventPhoto"> event photo generated path </param>
        /// <param name="eventTitle"> event title </param>
        /// <param name="eventDescription"> event description </param>
        /// <param name="eventStartDate"> event starting date </param>
        /// <param name="eventEndDate"> event ending date </param>
        /// <param name="eventLocation"> event location </param>
        /// <param name="eventHostID"> id of the company who created the event </param>
        /// <param name="eventCollaborators"> list of all the companies invited to collaborate on the event </param>
        public Event(string eventPhoto, string eventTitle, string eventDescription, DateTime eventStartDate, DateTime eventEndDate, string eventLocation, int eventHostID, List<Company> eventCollaborators)
        {
            this.Photo = eventPhoto;
            this.Title = eventTitle;
            this.Description = eventDescription;
            this.StartDate = eventStartDate;
            this.EndDate = eventEndDate;
            this.Location = eventLocation;
            this.HostID = eventHostID;
            this.Collaborators = eventCollaborators;
        }
        public override string ToString()
        {
            return "Event: " + Photo + " " + Title + " " + Description + " " +
                StartDate.ToString() + " " + EndDate.ToString() + " " + Location + " " + HostID.ToString() +
                " " + Collaborators.ToString() + "\n";
        }
    }
}
