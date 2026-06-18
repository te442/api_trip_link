using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_trip_link.Models
{
    [Table("Categories_to_trip")]
    public class CategoriesToTrip
    {
        [Key, Column("categories_id", Order = 0)]
        public int CategoriesId { get; set; }

        [Key, Column("trip_id", Order = 1)]
        public int TripId { get; set; }

        public Category Category { get; set; }
        public Trip Trip { get; set; }
    }
}
