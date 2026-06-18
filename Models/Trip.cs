using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_trip_link.Models
{
    [Table("Trip")]
    public class Trip
    {
        [Key]
        [Column("Trip_id")]
        public int TripId { get; set; }

        [Column("Trip_name")]
        public string TripName { get; set; }

        [Column("user_id")]
        public string UserId { get; set; }

        [Column("Trip_Date")]
        public DateTime? TripDate { get; set; }

        [Column("Address_start")]
        public string AddressStart { get; set; }

        [Column("Start_time")]
        public TimeSpan? StartTime { get; set; }

        [Column("End_time")]
        public TimeSpan? EndTime { get; set; }

        [Column("trip_cost")]
        public decimal? TripCost { get; set; }

        public User User { get; set; }
        public ICollection<DesOfTrip> DesOfTrips { get; set; }
        public ICollection<CategoriesToTrip> CategoriesToTrips { get; set; }
        public ICollection<FeatureToTrip> FeatureToTrips { get; set; }
        public ICollection<NatureTrip> NatureTrips { get; set; }
    }
}
