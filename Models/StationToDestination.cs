using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_trip_link.Models
{
    [Table("station_to_destination")]
    public class StationToDestination
    {
        [Key, Column("Des_id", Order = 0)]
        public int DesId { get; set; }

        [Column("Station_num")]
        public int StationNum { get; set; }

        [Column("Direction_Type")]
        public string DirectionType { get; set; }

        [Column("Walking_distance")]
        public double? WalkingDistance { get; set; }

        [Column("Walking time")]
        public TimeSpan? WalkingTime { get; set; }

        [Column("Walking instructions")]
        public string WalkingInstructions { get; set; }

        [Column("level_id")]
        public int? LevelId { get; set; }

        [Column("Feature_id")]
        public int? FeatureId { get; set; }

        public Destination Destination { get; set; }
        public Station Station { get; set; }
    }
}
