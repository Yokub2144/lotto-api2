using LottoApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;
using LottoApi.Models;
using LottoApi.Models.req_res;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LottoApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _db;

        public AdminController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("lotto")]
        public async Task<IActionResult> Lotto()
        {
            List<Lottery> lotteries = await _db.Lottery.ToListAsync();
            return Ok(lotteries);
        }

        [HttpPost("Createlotto")]
        public async Task<IActionResult> Admin_create_lotto([FromBody] Admin_create_Req request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newLotto = new Lottery
            {
                uid = 1,
                price = request.Price,
                number = request.Number,
                start_date = DateOnly.FromDateTime(request.StartDate),
                end_date = DateOnly.FromDateTime(request.EndDate),
                status = request.Status
            };

            _db.Lottery.Add(newLotto);
            await _db.SaveChangesAsync();

            var response = new lotto_Res(
                newLotto.lid,
                newLotto.uid,
                newLotto.price,
                newLotto.number,
                newLotto.start_date,
                newLotto.end_date,
                newLotto.status
            );

            return Ok(response);
        }

        [HttpPost("addreward")]
        public async Task<IActionResult> Admin_create_reward([FromBody] Reward_req request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newReward = new Reward
            {
                Lid = request.Lid,
                Rank = request.Rank
            };

            _db.Reward.Add(newReward);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Reward added successfully.", reward = newReward });
        }

                [HttpPost("random-rewards")]
            public async Task<IActionResult> RandomRewards()
            {
                try
                {
                    // 1. ดึงข้อมูลลอตเตอรี่ทั้งหมด
                    var allLotteries = await _db.Lottery.ToListAsync();
                    if (allLotteries == null || !allLotteries.Any())
                    {
                        return BadRequest("ไม่พบข้อมูลลอตเตอรี่ที่จะทำการสุ่มรางวัล");
                    }

                    var random = new Random();
                    var usedLottoIds = new HashSet<int>();

                    // 2. สุ่มรางวัลที่ 1 (rank = "1") - 1 รางวัล
                    var winnerLotto = allLotteries[random.Next(allLotteries.Count)];
                    usedLottoIds.Add(winnerLotto.lid);
                    _db.Reward.Add(new Reward { Lid = winnerLotto.lid, Rank = "1" });

                    // 3. สุ่มรางวัลที่ 2 (rank = "2") - 1 รางวัล
                    var remainingLotteries = allLotteries.Where(l => !usedLottoIds.Contains(l.lid)).ToList();
                    if (remainingLotteries.Any())
                    {
                        var random2Winner = remainingLotteries[random.Next(remainingLotteries.Count)];
                        usedLottoIds.Add(random2Winner.lid);
                        _db.Reward.Add(new Reward { Lid = random2Winner.lid, Rank = "2" });
                    }

                    // 4. สุ่มรางวัลที่ 3 (rank = "3") - 1 รางวัล
                    remainingLotteries = allLotteries.Where(l => !usedLottoIds.Contains(l.lid)).ToList();
                    if (remainingLotteries.Any())
                    {
                        var random3Winner = remainingLotteries[random.Next(remainingLotteries.Count)];
                        usedLottoIds.Add(random3Winner.lid);
                        _db.Reward.Add(new Reward { Lid = random3Winner.lid, Rank = "3" });
                    }

                    // 5. สุ่มเลขท้าย 3 ตัว (rank = "4") - 1 รางวัล
                    string last3Digits = winnerLotto.number.Substring(winnerLotto.number.Length - 3);
                    var last3WinnersByNumber = allLotteries.Where(l => l.number.EndsWith(last3Digits) && !usedLottoIds.Contains(l.lid)).ToList();
                    if (last3WinnersByNumber.Any())
                    {
                        var last3Winner = last3WinnersByNumber[random.Next(last3WinnersByNumber.Count)];
                        _db.Reward.Add(new Reward { Lid = last3Winner.lid, Rank = "4" });
                    }

                    // 6. บันทึกข้อมูลทั้งหมด
                    await _db.SaveChangesAsync();

                    return Ok(new { message = "Random rewards (1, 2, 3, and last 3 digits) have been generated and saved." });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "An error occurred while generating rewards.", error = ex.Message });
                }
            }
                        [HttpPost("select-reward")]
            public async Task<IActionResult> SelectReward([FromBody] SelectRewardRequest request)
            {
                // 1. ตรวจสอบความถูกต้องของข้อมูล
                if (string.IsNullOrEmpty(request.Number) || request.Number.Length != 2)
                {
                    return BadRequest(new { message = "กรุณากรอกเลขท้าย 2 ตัวให้ถูกต้อง" });
                }
                
                // 2. ค้นหาหมายเลขลอตเตอรี่ที่มีเลขท้าย 2 ตัวที่ตรงกัน
                var lotteries = await _db.Lottery.Where(l => l.number.EndsWith(request.Number)).ToListAsync();
                
                if (lotteries == null || !lotteries.Any())
                {
                    return NotFound(new { message = "ไม่พบหมายเลขลอตเตอรี่ที่มีเลขท้าย 2 ตัวที่ระบุ" });
                }

                // 3. บันทึกข้อมูลรางวัลที่ 5 สำหรับทุกใบที่มีเลขท้ายตรงกัน
                foreach (var lottery in lotteries)
                {
                    var existingReward = await _db.Reward.FirstOrDefaultAsync(r => r.Lid == lottery.lid);
                    if (existingReward == null) // ตรวจสอบว่ายังไม่เคยได้รับรางวัล
                    {
                        _db.Reward.Add(new Reward { Lid = lottery.lid, Rank = "5" });
                    }
                }

                // 4. บันทึกข้อมูลลงในฐานข้อมูล
                await _db.SaveChangesAsync();

                return Ok(new { message = $"บันทึกรางวัลที่ 5 (เลขท้าย 2 ตัว: {request.Number}) สำเร็จ" });
            }
          [HttpGet("showrank")]
public async Task<IActionResult> ShowRank()
{
    var winningLotteries = await _db.Lottery
        .Join(
            _db.Reward,
            lottery => lottery.lid, 
            reward => reward.Lid,    
            (lottery, reward) => new Lotteryreward
            {
                lid = lottery.lid,
                number = lottery.number,
                rank = reward.Rank
            }
        )
        // เปลี่ยน r.Rank เป็น r.rank
        .Where(r => r.rank != null)
        .ToListAsync();

    if (!winningLotteries.Any())
    {
        return NotFound("No winning lotteries found.");
    }

    return Ok(winningLotteries);
}
        
                }
}