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
    // ดึงออเดอร์ของผู้ใช้ที่ "ยังไม่ขึ้นรางวัล" หรือ "ถูกรางวัล (รอรับเงิน)" อยู่แล้ว
    var candidateOrders = await _db.Orders
        .AsNoTracking()
        .Where(o => o.uid == uid &&
               (o.statusbonus == "ยังไม่ขึ้นรางวัล" || o.statusbonus.Contains("(รอรับเงิน)")))
        .OrderByDescending(o => o.date)
        .ToListAsync();

    if (candidateOrders.Count == 0)
        return Ok(Array.Empty<object>());

    // กลุ่มย่อย
    var notMarked = candidateOrders.Where(o => o.statusbonus == "ยังไม่ขึ้นรางวัล").ToList();
    var alreadyPending = candidateOrders.Where(o => o.statusbonus.Contains("(รอรับเงิน)")).ToList();

    // โหลด reward เฉพาะ lid ที่เกี่ยวข้องทั้งหมด
    var lidsAll = candidateOrders.Select(o => o.lid).Distinct().ToList();
    var rewards = await _db.Reward
        .AsNoTracking()
        .Where(r => lidsAll.Contains(r.Lid))
        .ToListAsync();

    // ถ้าหนึ่ง lid มีหลายอันดับ → เลือกอันดับดีที่สุด (เลขน้อยสุด = รางวัลสูงสุด)
    var bestRewardByLid = rewards
        .GroupBy(r => r.Lid)
        .ToDictionary(
            g => g.Key,
            g => g.OrderBy(r => int.TryParse(r.Rank, out var n) ? n : int.MaxValue).First()
        );

    var winners = new List<object>();
    var updatedCount = 0;

    // 1) เติมรายการที่ "ถูกรางวัล (รอรับเงิน)" อยู่แล้ว
    foreach (var o in alreadyPending)
    {
        if (!bestRewardByLid.TryGetValue(o.lid, out var rw)) continue; // กันกรณีไม่มีแถวใน reward
        var prizeEach  = RewardHelper.PrizeByRank(rw.Rank);
        if (prizeEach <= 0) continue;

<<<<<<< HEAD
        if (orders.Count == 0)
            return Ok(new { message = "ไม่มีออเดอร์ที่รอเช็คผล" });


        var rewards = await _db.Reward.AsNoTracking().ToListAsync();
        var lotteries = await _db.Lottery.AsNoTracking().ToListAsync();


        var bestRewardByLid = rewards
            .GroupBy(r => r.Lid)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(r =>
                    int.TryParse(r.Rank, out var n) ? n : int.MaxValue
                ).First()
            );

        var winners = new List<object>();
        var losers = new List<int>();

        foreach (var o in orders)
        {
            if (!bestRewardByLid.TryGetValue(o.lid, out var reward))
                continue;

            var lottery = lotteries.FirstOrDefault(l => l.lid == o.lid);
            var lotteryNumber = lottery?.number;
=======
        var prizeTotal = prizeEach * o.amount;
        winners.Add(new
        {
            o.oid,
            o.lid,
            rank = rw.Rank,
            prizeEach,
            amount = o.amount,
            prizeTotal
        });
    }
>>>>>>> 9cccb3a36762ec20a9e97c4c63bc281a84053666

    // 2) ประมวลผลเฉพาะที่ "ยังไม่ขึ้นรางวัล" → ถ้าถูกรางวัลให้ mark และใส่ผลลัพธ์
    foreach (var po in notMarked)
    {
        if (!bestRewardByLid.TryGetValue(po.lid, out var rw)) continue; // ยังไม่ประกาศผลเลขนี้

<<<<<<< HEAD
            if (prizeEach > 0)
            {
                o.statusbonus = $"ถูกรางวัล {prizeEach} บาท x {o.amount} = {prizeTotal} (รอรับเงิน)";
                winners.Add(new
                {
                    o.oid,
                    o.lid,
                    LotteryNumber = lotteryNumber,
                    reward.Rank,
                    prizeEach,
                    o.amount,
                    prizeTotal
                });
            }
            else
            {
                o.statusbonus = "ไม่ถูกรางวัล";
                losers.Add(o.oid);
            }
        }
=======
        var prizeEach = RewardHelper.PrizeByRank(rw.Rank);
        if (prizeEach <= 0) continue; // ไม่ใช่อันดับที่ได้รางวัลเป็นเงิน
>>>>>>> 9cccb3a36762ec20a9e97c4c63bc281a84053666

        var prizeTotal = prizeEach * po.amount;

        // อัปเดต statusbonus เป็น “ถูกรางวัล … (รอรับเงิน)”
        var toUpdate = new Order
        {
            oid = po.oid,
            uid = po.uid,
            lid = po.lid,
            amount = po.amount,
            date = po.date
        };
        _db.Attach(toUpdate);
        toUpdate.statusbonus = $"ถูกรางวัล {rw.Rank} ได้ {prizeEach} บาท x {po.amount} = {prizeTotal} (รอรับเงิน)";
        _db.Entry(toUpdate).Property(x => x.statusbonus).IsModified = true;
        updatedCount++;

        winners.Add(new
        {
            toUpdate.oid,
            toUpdate.lid,
            rank = rw.Rank,
            prizeEach,
            amount = po.amount,
            prizeTotal
        });
    }

    if (updatedCount > 0)
        await _db.SaveChangesAsync();

    // ส่งกลับเฉพาะ “ถูกรางวัล” ทั้งที่มีอยู่แล้วและที่เพิ่งเจอใหม่
    return Ok(winners);
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
