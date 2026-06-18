using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_trip_link.Models
{
    [Table("Feature_to_trip")]
    public class FeatureToTrip
    {
        [Key]
        [Column("Feature_id")]
        public int FeatureId { get; set; }

        [Column("trip_id")]
        public int TripId { get; set; }

        public FeatureType FeatureType { get; set; }
        public Trip Trip { get; set; }
    }
}
