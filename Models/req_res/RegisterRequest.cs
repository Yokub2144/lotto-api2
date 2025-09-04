namespace LottoApi.Models.req_res
{
    public record RegisterRequest(
        string Email,
        string Password,
        string Fullname,
        string? Phone,
        string? Birthday // YYYY-MM-DD
    );
}