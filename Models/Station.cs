using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_trip_link.Models
{
    [Table("Station")]
    public class Station
    {
        [Key]
        [Column("Station_num")]
        public int StationNum { get; set; }

        [Column("Statoin_code")]
        public string StationCode { get; set; }

        [Column("Station_name")]
        public string StationName { get; set; }

        [Column("area")]
        public string Area { get; set; }

        [Column("lat")]
        public decimal? Lat { get; set; }

        [Column("lon")]
        public decimal? Lon { get; set; }

        [Column("government_stop_id")]
        public string GovernmentStopId { get; set; }

        public ICollection<StationToDestination> StationToDestinations { get; set; }
        public ICollection<BusStation> BusStations { get; set; }
    }
}
