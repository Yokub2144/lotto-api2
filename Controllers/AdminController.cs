using LottoApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using LottoApi.Models;
using LottoApi.Models.req_res;

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
            // 1. Validate the incoming request data
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 2. Map the request model to the database model (Lottery)
            var newLotto = new Lottery
{
                uid = 1,
                price = request.Price,
                number = request.Number,
                start_date = DateOnly.FromDateTime(request.StartDate),
                end_date = DateOnly.FromDateTime(request.EndDate),
                status = request.Status
            };

            // 3. Add the new lotto record to the database
            _db.Lottery.Add(newLotto);
            await _db.SaveChangesAsync();

            // 4. Create a response object to send back to the client
            // แก้ชื่อ record จาก lotto_Respon เป็น lotto_Res
                    var response = new lotto_Res(
                newLotto.lid,
                newLotto.uid,
                newLotto.price,
                newLotto.number,
                newLotto.start_date,
                newLotto.end_date,
                
                newLotto.status
            );  

            // 5. Return a 201 Created status with the new resource
            return Ok(response);
        }
    }
}