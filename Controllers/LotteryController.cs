using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LottoApi.Data;
using LottoApi.Models;

namespace LottoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LotteryController : ControllerBase
{
    private readonly AppDbContext _db;
    public LotteryController(AppDbContext db) => _db = db;
    
    public record BuyDto(int uid, int lid);

    [HttpPost("buy")]
    public async Task<IActionResult> Buy([FromBody] BuyDto dto)
    {
        var userExists = await _db.User.AnyAsync(u => u.uid == dto.uid);
        if (!userExists) return NotFound(new { message = "User not found" });

        var wallet = await _db.Wallet.FirstOrDefaultAsync(w => w.uid == dto.uid);
        if (wallet is null) return NotFound(new { message = "Wallet not found" });

        var lottery = await _db.Lottery.FirstOrDefaultAsync(l => l.lid == dto.lid);
        if (lottery is null) return NotFound(new { message = "Lottery not found" });
        if (lottery.status != "have") return BadRequest(new { message = "This lottery is not available." });

        if (wallet.money < lottery.price)
            return BadRequest(new { message = "Insufficient balance", balance = wallet.money });

        using var tx = await _db.Database.BeginTransactionAsync();

        // บันทึกการซื้อ
        var buy = new BuyLottery
        {
            uid = dto.uid,
            lid = dto.lid
        };
        _db.BuyLottery.Add(buy);

        // หักเงิน
        wallet.money -= lottery.price;

        // เปลี่ยนสถานะเป็น sold
        lottery.status = "sold";

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return Ok(new
        {
            message = "Buy success",
            buyid = buy.buyid,
            number = lottery.number,
            price = lottery.price,
            balance = wallet.money
        });
    }

    // ดูรายการที่ user ซื้อไปแล้ว
    [HttpGet("my/{uid:int}")]
    public async Task<IActionResult> MyTickets(int uid)
    {
        var list = await _db.BuyLottery
            .Where(b => b.uid == uid)
            .Join(_db.Lottery,
                b => b.lid,
                l => l.lid,
                (b, l) => new
                {
                    b.buyid,
                    l.lid,
                    l.number,
                    l.price,
                    l.status
                })
            .ToListAsync();

        return Ok(list);
    }
}
