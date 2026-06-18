using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_trip_link.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        [Column("user_id")]
        public string UserId { get; set; }

        [Column("FullName")]
        public string FullName { get; set; }

        [Column("Phon")]
        public string Phone { get; set; }

        [Column("Email")]
        public string? Email { get; set; }

        [Column("PasswordHash")]
        public string? PasswordHash { get; set; }

        public ICollection<Trip> Trips { get; set; }
    }
}
