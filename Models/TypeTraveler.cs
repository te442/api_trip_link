using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_trip_link.Models
{
    [Table("TypeTraveler")]
    public class TypeTraveler
    {
        [Key]
        [Column("Traveler_id")]
        public int TravelerId { get; set; }

        [Column("TypeTraveler")]
        public string TypeTravelerName { get; set; }

        public ICollection<Destination> Destinations { get; set; }
    }
}
