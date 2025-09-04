using LottoApi.Data;
using LottoApi.Models.req_res;
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

        [HttpPost("lotto")]
        public async Task<IActionResult> Lotto(Lotto_Respon respon)
        {
            return await _db.Lotto.ToListAsync();
        }
    }
}