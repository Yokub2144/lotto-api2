namespace LottoApi.Models.req_res
{
    public record lotto_Respon(
        int lid,
        int uid,
        float price,
        int number,
        string start_date, // YYYY-MM-DD
        string end_date,
        string status
    );
}