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

    public class Order
    {
        [Key]
        public int oid { get; set; }
        public int uid { get; set; }
        public int lid { get; set; }
        public DateTime date { get; set; } = DateTime.Now; // ค่า default วันที่ซื้อ
        public int amount { get; set; } = 1; // ค่า default = 1
        public string statusbonus { get; set; } = "ยังไม่ขึ้นรางวัล"; // ค่า default


    }

    public static class RewardHelper
    {
        public static int PrizeByRank(string rank) => rank switch
        {
            "1" => 6040000,
            "2" => 200000,
            "3" => 80000,
            "4" => 40000,
            "5" => 20000,
            _ => 0
        };
    }
}