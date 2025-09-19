using System.ComponentModel.DataAnnotations;

namespace LottoApi.Models
{
    public class Wallet
    {
        [Key]
        public int wid { get; set; }

        public int uid { get; set; }

        public decimal money { get; set; }

         public string? account_id { get; set; }

    }
}