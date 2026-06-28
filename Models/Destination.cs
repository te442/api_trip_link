using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_trip_link.Models
{
    [Table("Destination")]
    public class Destination
    {
        [Key]
        [Column("Des_id")]
        public int DesId { get; set; }

        [Column("Name_des")]
        public string NameDes { get; set; }

        [Column("Rregion")]
        public string Region { get; set; }

        [Column("level_id")]
        public int? LevelId { get; set; }

        [Column("Traveler_id")]
        public int? TravelerId { get; set; }

        [Column("Time_des")]
        public TimeSpan? TimeDes { get; set; }

        [Column("opening_time")]
        public TimeSpan OpeningTime { get; set; } = TimeSpan.FromHours(8);

        [Column("closing_time")]
        public TimeSpan ClosingTime { get; set; } = TimeSpan.FromHours(17);

        [Column("lat")]
        public decimal? Lat { get; set; }

        [Column("lon")]
        public decimal? Lon { get; set; }

        [Column("image_url")]
        public string? ImageUrl { get; set; }

        public DifficultyLevel DifficultyLevel { get; set; }
        public TypeTraveler TypeTraveler { get; set; }
        public ICollection<DestinationFeature> DestinationFeatures { get; set; }
        public ICollection<CategoriesOfDestination> CategoriesOfDestinations { get; set; }
        public ICollection<StationToDestination> StationToDestinations { get; set; }
        public ICollection<DesOfTrip> DesOfTrips { get; set; }
    }
}
