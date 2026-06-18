using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_trip_link.Models
{
    [Table("bus_station")]
    public class BusStation
    {
        [Key, Column("bus_id", Order = 0)]
        public int BusId { get; set; }

        [Key, Column("station_id", Order = 1)]
        public int StationId { get; set; }

        [Column("stop_sequence")]
        public int? StopSequence { get; set; }

        public Bus Bus { get; set; }
        public Station Station { get; set; }
    }
}
