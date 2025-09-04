using LottoApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LottoApi.Models.req_res;
using LottoApi.Models;
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
        public async Task<IActionResult> Lotto(lotto_Respon respon)
        {
            List<Lottery> lotteries = await _db.Lottery.ToListAsync();
            return Ok(lotteries);
        }
    }
}