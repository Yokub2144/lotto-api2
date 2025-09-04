using LottoApi.Data;
using LottoApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public async Task<IActionResult> Lotto() // Removed 'respon' parameter
        {
            var lottoData = await _db.Lotto.ToListAsync();
            return Ok(lottoData);
        }
    }
}