using System;
using System.Collections.Generic;
using System.Text;

namespace Taxi.Common.Models
{
    public class TaxiResponse
    {
        public int Id { get; set; }
        public string Plate { get; set; }
        public ICollection<TripResponse> Trips { get; set; }
        public UserResponse User { get; set; }

    }
}
