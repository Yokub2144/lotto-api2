namespace LottoApi.Models.req_res
{
    public record lotto_Res
    (
        int lid,
        int uid,
        decimal price,        // float -> decimal
        string number,        // int -> string
        DateOnly start_date,  // DateTime -> DateOnly
        DateOnly end_date,
        string status
    );
}