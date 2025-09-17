namespace LottoApi.Models.req_res
{
    public record Admin_create_Req
    (
        decimal Price,         // เปลี่ยน float -> decimal
        string Number,         // เปลี่ยน int -> string
        DateTime StartDate,
        DateTime EndDate,
        string Status
    );
}