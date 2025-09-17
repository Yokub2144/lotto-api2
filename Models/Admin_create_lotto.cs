using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LottoApi.Models
{
    public class Admin_create
    {
        // This is the primary key for the Lottery table
        [Key]
        [Column("lid")]
        public int Lid { get; set; }

        // Foreign key to link with the user who created the lottery
        [Column("uid")]
        public int Uid { get; set; }

        // The price of the lottery ticket
        [Column("price")]
        public int Price { get; set; }

        // The lottery number
        [Column("number")]
        [StringLength(10)]
        public string? Number { get; set; }

        // The date when the lottery period starts
        [Column("start_date")]
        public DateTime StartDate { get; set; }

        // The date when the lottery period ends
        [Column("end_date")]
        public DateTime EndDate { get; set; }

        // The status of the lottery (e.g., active, expired, drawn)
        [Column("status")]
        [StringLength(50)]
        public string? Status { get; set; }
    }
}