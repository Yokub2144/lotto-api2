namespace LottoApi.Models
{
    public record lotto_Respon
    (
        int lid,
        int uid,
        float price,
        int number,
        DateTime start_date, // Use DateTime instead of date
        DateTime end_date,   // Use DateTime instead of date
        string status
    );
}