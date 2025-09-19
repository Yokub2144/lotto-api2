using LottoApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LottoApi.Models.req_res;
using LottoApi.Models;
namespace LottoApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _db;

        public UserController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("user_uid")]
        public async Task<IActionResult> getUserById(int id)
        {
            var User = await _db.User.FirstOrDefaultAsync(u => u.uid == id);
            if (User == null) return NotFound(new { message = "ไม่พบผู้ใช้ที่มี Id นี้" });
            return Ok(new
            {
                message = "พบผู้ใช้",
                data = User
            });
        }
    }
}