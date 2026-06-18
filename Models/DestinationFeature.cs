using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_trip_link.Models
{
    [Table("DestinationFeatures")]
    public class DestinationFeature
    {
        [Key]
        [Column("Feature_id")]
        public int FeatureId { get; set; }

        [Column("topical_id")]
        public int TopicalId { get; set; }
    }
}
