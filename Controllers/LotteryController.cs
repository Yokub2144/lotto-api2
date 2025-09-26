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

    [ApiController]
    [Route("api/[controller]")]
    public class RewardController : ControllerBase
    {
        // mock data จำลอง
        private static readonly List<Lotteryreward> rewards = new()
        {
            new Lotteryreward { lid = 1, number = "123456", rank = "1" },
            new Lotteryreward { lid = 2, number = "234567", rank = "2" },
            new Lotteryreward { lid = 3, number = "345678", rank = "3" },
            new Lotteryreward { lid = 4, number = "456789", rank = "4" },
            new Lotteryreward { lid = 5, number = "567890", rank = "5" }
        };

        [HttpGet]
        public IActionResult GetAllRewards()
        {
            return Ok(rewards);
        }

        [HttpGet("{rank}")]
        public IActionResult GetRewardByRank(string rank)
        {
            var reward = rewards.Find(r => r.rank == rank);
            if (reward == null) return NotFound("ไม่พบรางวัล");
            return Ok(new
            {
                reward.number,
                reward.rank,
            });
        }
    }
    [HttpPost("check/{uid:int}")]
    public async Task<IActionResult> CheckReward(int uid)
    {
        var orders = await _db.Orders
            .Where(o => o.uid == uid && o.statusbonus == "ยังไม่ขึ้นรางวัล")
            .ToListAsync();

        if (orders.Count == 0)
            return Ok(new { message = "ไม่มีออเดอร์ที่รอเช็คผล" });

        // ✅ เลือก “รางวัลดีที่สุด” ต่อ lid
        var rewards = await _db.Reward.AsNoTracking().ToListAsync();
        var bestRewardByLid = rewards
            .GroupBy(r => r.Lid)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(r => int.TryParse(r.Rank, out var n) ? n : int.MaxValue).First()
            );

        var winners = new List<object>();
        var losers = new List<int>();

        foreach (var o in orders)
        {
            if (!bestRewardByLid.TryGetValue(o.lid, out var reward))
                continue; // ยังไม่ประกาศเลขนี้

            var prizeEach = RewardHelper.PrizeByRank(reward.Rank);
            var prizeTotal = prizeEach * o.amount;

            if (prizeEach > 0)
            {
                o.statusbonus = $"ถูกรางวัล {prizeEach} บาท x {o.amount} = {prizeTotal} (รอรับเงิน)";
                winners.Add(new { o.oid, o.lid, reward.Rank, prizeEach, o.amount, prizeTotal });
            }
            else
            {
                o.statusbonus = "ไม่ถูกรางวัล";
                losers.Add(o.oid);
            }
        }

        await _db.SaveChangesAsync();

        return Ok(new { message = "อัปเดตผลรางวัลแล้ว (ยังไม่จ่ายเงิน)", winners, losers });
    }


    [HttpPost("claim/{oid:int}")]
    public async Task<IActionResult> Claim(int oid)
    {
        var o = await _db.Orders.FirstOrDefaultAsync(x => x.oid == oid);
        if (o is null) return NotFound(new { message = "ไม่พบออเดอร์" });

        if (o.statusbonus.Contains("จ่ายแล้ว"))
            return BadRequest(new { message = "ออเดอร์นี้รับรางวัลไปแล้ว" });
        if (!o.statusbonus.Contains("(รอรับเงิน)"))
            return BadRequest(new { message = "ออเดอร์นี้ยังไม่อยู่ในสถานะรอรับเงิน" });

        var reward = await _db.Reward.FirstOrDefaultAsync(r => r.Lid == o.lid);
        if (reward is null) return BadRequest(new { message = "ไม่พบข้อมูลรางวัลของเลขนี้" });

        var prizeEach = RewardHelper.PrizeByRank(reward.Rank);
        if (prizeEach <= 0) return BadRequest(new { message = "ออเดอร์นี้ไม่ถูกรางวัล" });

        var wallet = await _db.Wallet.FirstOrDefaultAsync(w => w.uid == o.uid);
        if (wallet is null) return NotFound(new { message = "ไม่พบ Wallet ของลูกค้า" });

        var prizeTotal = prizeEach * o.amount;

        using var tx = await _db.Database.BeginTransactionAsync();
        wallet.money += prizeTotal;
        o.statusbonus = $"จ่ายแล้ว {prizeEach} บาท x {o.amount} = {prizeTotal}";
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return Ok(new { message = "จ่ายรางวัลสำเร็จ", order = new { o.oid, o.lid, o.statusbonus }, balance = wallet.money });
    }


    [HttpPost("claim-all/{uid:int}")]
    public async Task<IActionResult> ClaimAll(int uid)
    {
        var pending = await _db.Orders
            .Where(o => o.uid == uid && o.statusbonus.Contains("(รอรับเงิน)"))
            .ToListAsync();

        if (pending.Count == 0)
            return Ok(new { message = "ไม่มีรายการรอรับเงิน" });

        var rewardsByLid = await _db.Reward
            .AsNoTracking()
            .ToDictionaryAsync(r => r.Lid, r => r.Rank);

        var wallet = await _db.Wallet.FirstOrDefaultAsync(w => w.uid == uid);
        if (wallet is null) return NotFound(new { message = "ไม่พบ Wallet ของลูกค้า" });

        var paid = new List<object>();

        using var tx = await _db.Database.BeginTransactionAsync();

        foreach (var o in pending)
        {
            if (!rewardsByLid.TryGetValue(o.lid, out var rank))
                continue;

            var prizeEach = RewardHelper.PrizeByRank(rank);
            if (prizeEach <= 0) continue;

            var prizeTotal = prizeEach * o.amount;

            wallet.money += prizeTotal;
            o.statusbonus = $"จ่ายแล้ว {prizeEach} บาท x {o.amount} = {prizeTotal}";

            paid.Add(new { o.oid, o.lid, rank, prizeEach, o.amount, prizeTotal });
        }

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return Ok(new { message = "จ่ายรางวัลทั้งหมดสำเร็จ", paid, balance = wallet.money });
    }

    [HttpGet("getreward")]
    public async Task<IActionResult> GetReward()
    {
        try
        {
            // ดึงข้อมูลทั้งหมดจากตาราง Reward
            var rewards = await _db.Reward
                .OrderBy(r => r.Rank)
                .Select(r => new
                {
                    r.Rid,
                    r.Lid,
                    r.Rank
                })
                .ToListAsync();

            return Ok(rewards);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }


}
