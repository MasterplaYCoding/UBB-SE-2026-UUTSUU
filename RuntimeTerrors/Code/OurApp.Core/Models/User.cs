using System;

namespace OurApp.Core.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        public string? CvXml { get; set; }

        public User(int id, string name, string email, string? cvXml = null)
        {
            Id = id;
            Name = name;
            Email = email;
            CvXml = cvXml;
        }
    }
}
