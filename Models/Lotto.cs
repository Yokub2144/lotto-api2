using System.ComponentModel.DataAnnotations;

namespace LottoApi.Models
{
    public class Lottery
    {
        [Key]
        public int lid { get; set; }

        public int uid { get; set; }

        public decimal price { get; set; }

        public string? number { get; set; }

        public DateOnly start_date { get; set; }

        public DateOnly end_date { get; set; }

        public string? status { get; set; }
    }
}