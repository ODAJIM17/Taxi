using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Taxi.Common.Models
{
    public class TaxiResponse
    {
        public int Id { get; set; }
        public string Plate { get; set; }
        public ICollection<TripResponse> Trips { get; set; }
        public UserResponse User { get; set; }
        public float? Qualification => Trips == null ? 0 : Trips.Average(t => t.Qualification);
        public int? NumberOfTrips => Trips == null ? 0 : Trips.Count;


    }
}
