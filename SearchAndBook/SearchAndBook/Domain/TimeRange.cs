using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Vpn;

namespace SearchAndBook.Domain
{
    internal class TimeRange
    {
        public DateTime StartTime {  get; set; }
        public DateTime EndTime { get; set; }

        public bool IsValid() { 
            return StartTime < EndTime;
        }

    }
}
