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
        [HttpGet("restaurante/todosRestaurantes")]
        public async Task<IActionResult> GetRestaurantes([FromQuery] int usuarioId, [FromQuery] int perfilId, [FromQuery] string? busca)
        {

            // Query base, ela pega todos os restaurantes
            var query = _db.Restaurantes.AsNoTracking();

            // Se ele for Dono de restaurante, vai mostrar todos
            if (perfilId == 1)
            {
                query = query.Where(r => r.DonoId == usuarioId);
            }
            // Se ele for um cliente, vai mostrar todos os restaurantes que não estão "Deletado"
            else
            {
                query = query.Where(r => r.DeletedAt == null);
            }

            // se o usuário digitou texto para buscar algum restaurente
            // vai procurar ou pelo nome ou pelo tipo de restaurante
            if (!string.IsNullOrWhiteSpace(busca) && busca.Length >= 2)
            {
                busca = busca.ToLower();
                query = query.Where(r => r.Nome.ToLower().Contains(busca) ||
                                         r.Tipo.Nome.ToLower().Contains(busca));
            }

            var restaurantes = await query
                .OrderBy(r => r.DeletedAt == null)
                .ThenBy(r => r.Nome)
                .Select(r => new RestauranteDto
                {
                    IdRestaurante = r.Id,
                    NomeRestaurante = r.Nome,
                    NomeCidade = r.Cidade.Nome,
                    TipoRestaurante = r.Tipo.Nome,
                    IsDeleted = r.DeletedAt != null,
                    TotalCurtidas = _db.Cardapios
                                        .Where(c => c.RestauranteId == r.Id)
                                        .SelectMany(c => _db.ClienteCurtidas.Where(cc => cc.IdPrato == c.PratoId))
                                        .Count()
                }).ToListAsync();

            return Ok(restaurantes);
        }
    }
    public class LoginDto
    {
        public string Email { get; set; }
        public string Senha { get; set; }
    }

    public class RestauranteDto
    {
        public int IdRestaurante { get; set; }
        public string NomeRestaurante { get; set; } = string.Empty;
        public string NomeCidade { get; set; } = string.Empty;
        public string TipoRestaurante { get; set; } = string.Empty;
        public int TotalCurtidas { get; set; }
        public bool IsDeleted { get; set; }

    }
}



