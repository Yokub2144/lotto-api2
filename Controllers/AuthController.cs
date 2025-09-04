using LottoApi.Data;
using LottoApi.Models.req_res;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LottoApi.Models;

namespace LottoApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        public AuthController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var user = await _db.User.FirstOrDefaultAsync(u => u.email == request.Email);
            if (user == null || user.password != request.Password)
            {
                return BadRequest(new { message = "Email หรือรหัสผ่านไม่ถูกต้อง" });
            }

            return Ok(new
            {
                message = "Login สำเร็จ",
                uid = user.uid,
                email = user.email,
                fullname = user.fullname,
                role = user.role
            });
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var existingUser = await _db.User.FirstOrDefaultAsync(u => u.email == request.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "Email นี้มีผู้ใช้แล้ว" });
            }

            var user = new User
            {
                email = request.Email,
                password = request.Password,
                fullname = request.Fullname,
                phone = request.Phone,
                birthday = DateOnly.Parse(request.Birthday ?? "2000-01-01"),
                role = "user"
            };

            _db.User.Add(user);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "สมัครสมาชิกสำเร็จ",
                uid = user.uid,
                email = user.email,
                fullname = user.fullname
            });
        }
    }
}