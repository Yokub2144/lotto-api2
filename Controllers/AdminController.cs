using LottoApi.Data;
<<<<<<< HEAD
=======
using LottoApi.Models;
>>>>>>> a4a747eb437697884370d7ac9cb5e535db52c3ca
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

<<<<<<< HEAD
        [HttpPost("lotto")]
        public async Task<IActionResult> Lotto(lotto_Respon respon)
        {
            List<Lottery> lotteries = _db.Lottery.ToList();
            return Ok(lotteries);
=======
        [HttpGet("lotto")]
        public async Task<IActionResult> Lotto() // Removed 'respon' parameter
        {
            var lottoData = await _db.Lotto.ToListAsync();
            return Ok(lottoData);
>>>>>>> a4a747eb437697884370d7ac9cb5e535db52c3ca
        }
    }
}