using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_trip_link.Models
{
    [Table("FeatureTypes")]
    public class FeatureType
    {
        [Key]
        [Column("Feature_id")]
        public int FeatureId { get; set; }

        [Column("Feature")]
        public string Feature { get; set; }

        public ICollection<FeatureToTrip> FeatureToTrips { get; set; }
    }
}
