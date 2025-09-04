namespace LottoApi.Models
{
    public record RegisterRequest(
        string Email,
        string Password,
        string Fullname,
        string? Phone,
        string? Birthday // YYYY-MM-DD
    );
}