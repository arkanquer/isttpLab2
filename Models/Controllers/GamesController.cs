using Microsoft.AspNetCore.Mvc;
using GameLibraryAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GameLibraryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GamesController : ControllerBase
    {
        private readonly GameContext _context;

        public GamesController(GameContext context) 
        { 
            _context = context; 
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Game>>> GetGames()
        {
            return await _context.Games
                .Include(g => g.Genre)
                .Where(g => !g.IsArchived)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Game>> GetGame(int id)
        {
            var game = await _context.Games.Include(g => g.Genre).FirstOrDefaultAsync(g => g.Id == id);
            if (game is null) return NotFound();
            return game;
        }

        [HttpPost]
        public async Task<ActionResult<Game>> PostGame(Game game)
        {
            var genreExists = await _context.Genres.AnyAsync(g => g.Id == game.GenreId);
            if (!genreExists)
            {
                return BadRequest(new { message = "Жанру не існує" });
            }

            var gameExists = await _context.Games.AnyAsync(g => g.Name.ToLower() == game.Name.ToLower());
            if (gameExists)
            {
                return BadRequest(new { message = $"Гра '{game.Name}' вже існує в каталозі." });
            }

            _context.Games.Add(game);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetGame), new { id = game.Id }, game);
        }

        [HttpPatch("{id}/archive")]
        public async Task<IActionResult> ArchiveGame(int id)
        {
            var game = await _context.Games.FindAsync(id);
            if (game is null) return NotFound();
            game.IsArchived = true;
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Гра '{game.Name}' архівована!" });
        }
    }
}