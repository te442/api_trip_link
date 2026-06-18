using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_trip_link.Models
{
    [Table("Nature_trip")]
    public class NatureTrip
    {
        [Key]
        [Column("natureTrip_id")]
        public int NatureTripId { get; set; }

        [Column("trip_id")]
        public int TripId { get; set; }

        [Column("Num_break")]
        public int? NumBreak { get; set; }

        [Column("Min_num_des")]
        public int? MinNumDes { get; set; }

        [Column("Max_num_des")]
        public int? MaxNumDes { get; set; }

        [Column("level_id")]
        public int? LevelId { get; set; }

        [Column("Region")]
        public string Region { get; set; }

        public Trip Trip { get; set; }
        public DifficultyLevel DifficultyLevel { get; set; }
    }
}
