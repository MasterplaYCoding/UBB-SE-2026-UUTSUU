using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchAndBook.Domain
{
    public class Rental
    {
        public int RentalId { get; set; }
        public int GameId { get; set; }
        public int RenterId { get; set; }
        public int OwnerId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal? TotalPrice { get; set; }
    }
}
