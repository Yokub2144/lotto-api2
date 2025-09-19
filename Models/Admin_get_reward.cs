using System.ComponentModel.DataAnnotations;

namespace LottoApi.Models
{
    public class Lotteryreward
    {
        public int lid { get; set; }
        public string number { get; set; }
        public string rank { get; set; }
    }
}