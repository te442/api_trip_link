using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_trip_link.Models
{
    [Table("Des_of_trip")]
    public class DesOfTrip
    {
        [Key, Column("Trip_id", Order = 0)]
        public int TripId { get; set; }

        [Key, Column("Des_id", Order = 1)]
        public int DesId { get; set; }

        [Column("visit_number")]
        public int? VisitNumber { get; set; }

        public Trip Trip { get; set; }
        public Destination Destination { get; set; }
    }
}
