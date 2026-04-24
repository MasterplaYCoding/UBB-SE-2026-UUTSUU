using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OurApp.Core.Models;

namespace OurApp.Core.Services
{
    public class SessionService
    {
        public Company LoggedInUser { get; }

        public SessionService(Company user)
        {
            this.LoggedInUser = user;
        }
    }
}
