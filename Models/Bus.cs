using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_trip_link.Models
{
    [Table("Bus")]
    public class Bus
    {
        [Key]
        [Column("bus_id")]
        public int BusId { get; set; }

        [Column("Bus_code")]
        public string BusCode { get; set; }

        [Column("Bus_number")]
        public int BusNumber { get; set; }

        [Column("agency_id")]
        public int? AgencyId { get; set; }

        [Column("Direction")]
        public string Direction { get; set; }

        [Column("government_route_id")]
        public string GovernmentRouteId { get; set; }

        public ICollection<BusStation> BusStations { get; set; }
    }
}
