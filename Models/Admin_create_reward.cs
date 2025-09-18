using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LottoApi.Models
{
    [Table("reward")]
    public class Reward
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("rid")]
        public int Rid { get; set; }

        [Column("lid")]
        public int Lid { get; set; }

        [Column("rank")]
        public string? Rank { get; set; }
    }
}