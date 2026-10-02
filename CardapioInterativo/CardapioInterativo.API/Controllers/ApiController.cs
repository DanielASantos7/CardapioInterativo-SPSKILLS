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
                usuario.Email,
                usuario.Cpf,
                usuario.PerfilId
            });
        }
        [HttpGet("restaurante/todosRestaurantes")]
        public async Task<IActionResult> GetRestaurantes(
            [FromQuery] int usuarioId,
            [FromQuery] int perfilId,
            [FromQuery] string? busca)
        {
            // 1. Query base sem rastreamento de estado para máximo desempenho
            var query = _db.Restaurantes.AsNoTracking();

            // 2. Filtro de permissão por perfil
            if (perfilId == 1) // Proprietário / Dono
            {
                query = query.Where(r => r.DonoId == usuarioId);
            }
            else // Cliente comum
            {
                query = query.Where(r => r.DeletedAt == null);
            }

            // 3. Busca por texto (Apenas se tiver 2 ou mais caracteres)
            if (!string.IsNullOrWhiteSpace(busca) && busca.Trim().Length >= 2)
            {
                string termo = busca.Trim();

                // Preserva índices no SQL Server sem usar LOWER() na coluna
                query = query.Where(r => r.Nome.Contains(termo) ||
                                         r.Tipo.Nome.Contains(termo));
            }

            // 4. Projeção e Ordenação
            var restaurantes = await query
                .OrderByDescending(r => r.DeletedAt == null) // Non-deleted (1) primeiro
                .ThenBy(r => r.Nome)
                .Select(r => new RestauranteDto
                {
                    IdRestaurante = r.Id,
                    Nome = r.Nome, // Alinhado com o DTO do Mobile
                    Cidade = r.Cidade.Nome, // Alinhado com o DTO do Mobile
                    TipoRestaurante = r.Tipo.Nome, // Alinhado com o DTO do Mobile
                    Descricao = r.Descricao,
                    IsDeleted = r.DeletedAt != null,
                    // Agregação de curtidas / nota
                    Avaliacao = _db.Cardapios
                                        .Where(c => c.RestauranteId == r.Id)
                                        .SelectMany(c => _db.ClienteCurtidas.Where(cc => cc.IdPrato == c.PratoId))
                                        .Count()
                })
                .ToListAsync();

            return Ok(restaurantes);
        }


        [HttpDelete("restaurante/delete/{id}")]
        public async Task<IActionResult> SoftDeleteRestaurante([FromRoute] int id)
        {
            // busca o restaurante no banco
            var restaurante = await _db.Restaurantes.FirstOrDefaultAsync(r => r.Id == id);

            if (restaurante == null)
            {
                return NotFound(new { mensagem = $"Restaurante não foi encontrado" });
            }

            // valida se o restaurante já foi deletado
            if (restaurante.DeletedAt != null)
            {
                return BadRequest(new { mensagem = "Esse restaurante já se encontra desativado" });
            }

            restaurante.DeletedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Ok(true);
        }

        [HttpPut("restaurante/restaurar/{id}")]
        public async Task<IActionResult> RestaurarRestaurante(int id)
        {
            var restaurante = await _db.Restaurantes.FindAsync(id);

            if (restaurante == null)
            {
                return NotFound(new { mensagem = "Restaurante nao encontrado." });
            }

            restaurante.DeletedAt = null;

            await _db.SaveChangesAsync();

            return Ok(new { mensagem = "Restaurante restaurado com sucesso!" });
        }


        [HttpGet("cardapio/{restauranteId}")]
        public async Task<IActionResult> GetCardapio(int restauranteId)
        {
            var idsPratosMenu = await _db.Cardapios
                .Where(c =>
                    c.RestauranteId == restauranteId &&
                    c.PratoId != null)
                .Select(c => c.PratoId!.Value)
                .ToListAsync();


            var pratosDisponiveis = await _db.Pratos
                .Where(p => !idsPratosMenu.Contains(p.Id))
                .Select(p => new PratoDto
                {
                    Id = p.Id
                })
                .ToListAsync();


            var pratosMenu = await _db.Pratos
                .Where(p => idsPratosMenu.Contains(p.Id))
                .Select(p => new PratoDto
                {
                    Id = p.Id
                })
                .ToListAsync();


            return Ok(new CardapioDto
            {
                Disponiveis = pratosDisponiveis,

                Menu = pratosMenu
            });
        }

        [HttpPost("cardapio")]
        public async Task<IActionResult> AdicionarPrato(
            [FromBody] CardapioRequestDto dto)
        {
            var existe = await _db.Cardapios
                .AnyAsync(c =>
                    c.RestauranteId == dto.RestauranteId &&
                    c.PratoId == dto.PratoId);

            if (existe)
            {
                return BadRequest("Esse prato já está no cardápio.");
            }

            var cardapio = new Cardapio
            {
                RestauranteId = dto.RestauranteId,
                PratoId = dto.PratoId,
                Valor = dto.Valor
            };

            _db.Cardapios.Add(cardapio);

            await _db.SaveChangesAsync();

            return Ok(true);
        }


        [HttpDelete("cardapio/{restauranteId}/{pratoId}")]
        public async Task<IActionResult> RemoverPrato(
            int restauranteId,
            int pratoId)
        {
            var cardapio = await _db.Cardapios
                .FirstOrDefaultAsync(c =>
                    c.RestauranteId == restauranteId &&
                    c.PratoId == pratoId);

            if (cardapio == null)
            {
                return NotFound("Prato não encontrado no cardápio.");
            }

            _db.Cardapios.Remove(cardapio);

            await _db.SaveChangesAsync();

            return Ok(true);
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
        public string Nome { get; set; } = string.Empty;
        public string Cidade { get; set; } = string.Empty;
        public string TipoRestaurante { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public int Avaliacao { get; set; }
        public bool IsDeleted { get; set; }

    }

    public class PratoDto
    {
        public int? Id { get; set; }
    }

    public class CardapioDto
    {
        public List<PratoDto> Disponiveis { get; set; } = new();

        public List<PratoDto> Menu { get; set; } = new();
    }

    public class CardapioRequestDto
    {
        public int RestauranteId { get; set; }

        public int PratoId { get; set; }

        public double Valor { get; set; }
    }
}



