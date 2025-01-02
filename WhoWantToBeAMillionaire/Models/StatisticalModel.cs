using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WhoWantToBeAMillionaire.Models
{
    public class StatisticalModel
    {
        [Key]
        public int Id { get; set; }
        [Column("DateTime")]
        public DateTime DateAccess { get; set; }
        [Column("PlayerRounds")]
        public long CountAccess { get; set; }
    }
}
