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
        // ตรวจสอบ user
        var userExists = await _db.User.AnyAsync(u => u.uid == dto.uid);
        if (!userExists) return NotFound(new { message = "User not found" });

        // ตรวจสอบ wallet
        var wallet = await _db.Wallet.FirstOrDefaultAsync(w => w.uid == dto.uid);
        if (wallet is null) return NotFound(new { message = "Wallet not found" });

        // ตรวจสอบ lottery
        var lottery = await _db.Lottery.FirstOrDefaultAsync(l => l.lid == dto.lid);
        if (lottery is null) return NotFound(new { message = "Lottery not found" });
        if (lottery.status != "have") return BadRequest(new { message = "This lottery is not available." });

        // ตรวจสอบเงิน
        if (wallet.money < lottery.price)
            return BadRequest(new { message = "Insufficient balance", balance = wallet.money });

        using var tx = await _db.Database.BeginTransactionAsync();

        try
        {
            // บันทึกการซื้อ
            var order = new Order
            {
                uid = dto.uid,
                lid = dto.lid,
                date = DateTime.Now,                            // StatusBonus ใช้ default "ยังไม่ขึ้นรางวัล"
            };

            _db.Orders.Add(order); // เพิ่ม object เข้า DbSet

            // หักเงิน
            wallet.money -= lottery.price;

            // เปลี่ยนสถานะ lottery เป็น sold
            lottery.status = "sold";

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return Ok(new
            {
                message = "Buy success",
                buyid = order.oid,          // primary key
                number = lottery.number,    // เลขลอตเตอรี่
                price = lottery.price,
                balance = wallet.money,
                statusbonus = order.statusbonus
            });
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    // ดูรายการที่ user ซื้อไปแล้ว
    [HttpGet("my/{uid:int}")]
    public async Task<IActionResult> MyTickets(int uid)
    {
        var list = await _db.Orders
            .Where(b => b.uid == uid)
            .Join(_db.Lottery,
                b => b.lid,
                l => l.lid,
                (b, l) => new
                {
                    b.oid,
                    l.lid,
                    l.number,
                    l.price,
                    l.status
                })
            .ToListAsync();

        return Ok(list);
    }
}
