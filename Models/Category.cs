using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_trip_link.Models
{
    [Table("Categories")]
    public class Category
    {
        [Key]
        [Column("categories_id")]
        public int CategoriesId { get; set; }

        [Column("categories_name")]
        public string CategoriesName { get; set; }

        public ICollection<CategoriesOfDestination> CategoriesOfDestinations { get; set; }
        public ICollection<CategoriesToTrip> CategoriesToTrips { get; set; }
    }
}
