using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LottoApi.Data;
using LottoApi.Models;

namespace LottoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WalletController : ControllerBase
{
    private readonly AppDbContext _db;
    public WalletController(AppDbContext db) => _db = db;

    // ดูยอดเงินตาม uid
    [HttpGet("{uid:int}")]
    public async Task<IActionResult> GetWallet(int uid)
    {
        var wallet = await _db.Wallet.AsNoTracking().FirstOrDefaultAsync(w => w.uid == uid);
        return wallet is null ? NotFound(new { message = "Wallet not found" }) : Ok(wallet);
    }

    public record AddMoneyDto(int uid, decimal amount);

    // เติมเงิน
    [HttpPost("add")]
    public async Task<IActionResult> AddMoney([FromBody] AddMoneyDto dto)
    {
        if (dto.amount <= 0) return BadRequest(new { message = "Amount must be > 0" });

        // ใช้ transaction กัน race condition
        using var tx = await _db.Database.BeginTransactionAsync();

        var wallet = await _db.Wallet.FirstOrDefaultAsync(w => w.uid == dto.uid);
        if (wallet is null) return NotFound(new { message = "Wallet not found" });

        wallet.money += dto.amount;
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return Ok(new { message = "Money added", balance = wallet.money });
    }

    public record WithdrawDto(int uid, decimal amount);

    // ถอนเงิน
    [HttpPost("withdraw")]
    public async Task<IActionResult> Withdraw([FromBody] WithdrawDto dto)
    {
        if (dto.amount <= 0) return BadRequest(new { message = "Amount must be > 0" });

        using var tx = await _db.Database.BeginTransactionAsync();

        
        var wallet = await _db.Wallet.FirstOrDefaultAsync(w => w.uid == dto.uid);
        if (wallet is null) return NotFound(new { message = "Wallet not found" });

        if (wallet.money < dto.amount)
            return BadRequest(new { message = "Insufficient balance", balance = wallet.money });

        wallet.money -= dto.amount;
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return Ok(new { message = "Withdrawn", balance = wallet.money });
    }

    public record SetAccountDto(int uid, string account_id);


    public record UpdateWalletDto(string account_id, decimal? money);

[HttpPost("{uid:int}/update")]
public async Task<IActionResult> UpdateWallet(int uid, [FromBody] UpdateWalletDto dto)
{
    var wallet = await _db.Wallet.FirstOrDefaultAsync(w => w.uid == uid);
    if (wallet is null)
        return NotFound(new { message = "Wallet not found" });

    // แก้เลขบัญชี (ถ้ามีส่งมา)
    if (!string.IsNullOrWhiteSpace(dto.account_id))
        wallet.account_id = dto.account_id.Trim();

    // แก้ยอดเงิน (ถ้ามีส่งมา)
    if (dto.money.HasValue)
    {
        if (dto.money < 0)
            return BadRequest(new { message = "money must be >= 0" });
        wallet.money = dto.money.Value;
    }

    await _db.SaveChangesAsync();

    return Ok(new
    {
        message = "Wallet updated",
        wallet.wid,
        wallet.uid,
        wallet.money,
        wallet.account_id
    });
}
    public record CreateWalletDto(int uid);

    // (เสริม) สร้าง wallet ถ้ายังไม่มี (กรณีผู้ใช้เดิม)
    [HttpPost("create")]
    public async Task<IActionResult> CreateIfMissing([FromBody] CreateWalletDto dto)
    {
        var existsUser = await _db.User.AnyAsync(u => u.uid == dto.uid);
        if (!existsUser) return NotFound(new { message = "User not found" });

        var existsWallet = await _db.Wallet.AnyAsync(w => w.uid == dto.uid);
        if (existsWallet) return Conflict(new { message = "Wallet already exists" });

        var wallet = new Wallet { uid = dto.uid, money = 0m, account_id = null };
        _db.Wallet.Add(wallet);
        await _db.SaveChangesAsync();

        return Created($"/api/wallet/{dto.uid}", wallet);
    }
}
