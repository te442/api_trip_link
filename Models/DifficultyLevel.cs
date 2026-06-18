using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_trip_link.Models
{
    [Table("Difficulty_level")]
    public class DifficultyLevel
    {
        [Key]
        [Column("level_id")]
        public int LevelId { get; set; }

        [Column("level_type")]
        public string LevelType { get; set; }

        public ICollection<Destination> Destinations { get; set; }
        public ICollection<NatureTrip> NatureTrips { get; set; }
    }
}
