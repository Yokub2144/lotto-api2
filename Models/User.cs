using System.ComponentModel.DataAnnotations;

namespace LottoApi.Models
{
    public class User
    {
        [Key]
        public int uid { get; set; }
        public string? email { get; set; }
        public string? password { get; set; }
        public string? fullname { get; set; }
        public DateOnly birthday { get; set; }
        public string? phone { get; set; }
        public string role { get; set; } = "user";
    }
}