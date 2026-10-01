using CardapioInterativo.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CardapioInterativo.API.Controllers
{
    [ApiController]
    [Route("api")]
    public class ApiController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ApiController(AppDbContext context)
        {
            _db = context;
        }


        [HttpPost("auth/login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var usuario = await _db.Usuarios
                                   .Where(u => u.Email == dto.Email && u.Senha == dto.Senha)
                                   .FirstOrDefaultAsync();

            if (usuario == null)
            {
                return Unauthorized("Credenciais inválidas");
            }

            return Ok(new
            {
                usuario.Id,
                usuario.Nome,
                usuario.Cpf,
                usuario.PerfilId
            });
        }
    }

    public class LoginDto
    {
        public string Email { get; set; }
        public string Senha { get; set; }
    }
}
