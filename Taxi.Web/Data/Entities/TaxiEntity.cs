using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Taxi.Web.Data.Entities
{
    public class TaxiEntity
    {
        public int Id { get; set; }

        [StringLength(6, MinimumLength = 6, ErrorMessage = "The {0} field must have {1} characters.")]
        [Required(ErrorMessage = "The field {0} is required.")]
        [Display(Name = "Plate No")]
        public string Plate { get; set; }

        public ICollection<TripEntity> Trips { get; set; }
        public UserEntity User { get; set; }
    }
}
