using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_trip_link.Models
{
    [Table("Categories_of_Destination")]
    public class CategoriesOfDestination
    {
        [Key, Column("Categories_id", Order = 0)]
        public int CategoriesId { get; set; }

        [Key, Column("Des_id", Order = 1)]
        public int DesId { get; set; }

        public Destination Destination { get; set; }
        public Category Category { get; set; }
    }
}
