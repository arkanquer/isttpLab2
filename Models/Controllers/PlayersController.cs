using Microsoft.AspNetCore.Mvc;
using GameLibraryAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GameLibraryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlayersController : ControllerBase
    {
        private readonly GameContext _context;

        public PlayersController(GameContext context)
        {
            _context = context;
        }

        [HttpGet]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Player>>> GetPlayers()
        {
            return await _context.Players
                .Include(p => p.Library)
                .Where(p => !p.IsDeleted)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Player>> RegisterPlayer(Player player)
        {
            if (string.IsNullOrWhiteSpace(player.Nickname))
            {
                return BadRequest(new { message = "Нікнейм не може бути порожнім!" });
            }

            var nicknameExists = await _context.Players
                .AnyAsync(p => p.Nickname.ToLower() == player.Nickname.ToLower());

            if (nicknameExists)
            {
                return BadRequest(new { message = $"Нікнейм '{player.Nickname}' вже зайнятий іншим гравцем!" });
            }

            _context.Players.Add(player);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetPlayers), new { id = player.Id }, player);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlayer(int id)
        {
            var player = await _context.Players.FindAsync(id);
            if (player is null)
            {
                return NotFound(new { message = "Гравця не знайдено!" });
            }

            player.IsDeleted = true;
            var playerReviews = await _context.Reviews
                .Where(r => r.PlayerId == id && !r.IsDeleted)
                .ToListAsync();

            foreach (var review in playerReviews)
            {
                review.IsDeleted = true;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = $"Профіль гравця '{player.Nickname}' та всі його відгуки успішно деактивовані." });
        }

        [HttpPost("{id}/library/{gameId}")]
        public async Task<IActionResult> AddGameToLibrary(int id, int gameId)
        {
            var player = await _context.Players
                .Include(p => p.Library)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            var game = await _context.Games.FindAsync(gameId);

            if (player == null || game == null)
            {
                return NotFound(new { message = "Гравця або гру не знайдено в системі!" });
            }

            if (player.Library.Any(g => g.Id == gameId))
            {
                return BadRequest(new { message = "Ця гра вже додана до вашої особистої бібліотеки!" });
            }

            player.Library.Add(game);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Гру '{game.Name}' успішно додано до вашої бібліотеки!" });
        }
    }
}