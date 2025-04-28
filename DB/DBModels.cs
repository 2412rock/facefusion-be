using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FacefusionBE.DB
{
    public class DBUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        public string Email { get; set; }
        public string Password { get; set; }
        public int Credit { get; set; }

        // Navigation properties
    }

    public class DBUserSession
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SessionId { get; set; }

        public string Email { get; set; }

        public string SessionToken { get; set; }

        public DateTime LastActiveTime { get; set; }

        public bool IsActive { get; set; }

        // Navigation property
    }
}
