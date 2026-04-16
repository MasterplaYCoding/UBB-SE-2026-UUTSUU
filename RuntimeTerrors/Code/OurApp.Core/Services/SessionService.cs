using OurApp.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OurApp.Core.Services
{
    public class SessionService
    {
        public Company loggedInUser { get; }

        public SessionService(Company user)
        {
            this.loggedInUser = user;
        }
    }
}
