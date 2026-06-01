using Microsoft.AspNetCore.Mvc;
using GameLibraryAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GameLibraryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly GameContext _context;

        public ReviewsController(GameContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<Review>> PostReview(Review review)
        {
            if (review.Rating < 1 || review.Rating > 5)
            {
                return BadRequest(new { message = "Оцінка повинна бути строго в діапазоні від 1 до 5 зірок!" });
            }

            var playerExists = await _context.Players.AnyAsync(p => p.Id == review.PlayerId && !p.IsDeleted);
            if (!playerExists)
            {
                return BadRequest(new { message = $"Помилка: Гравця з ID {review.PlayerId} не існує або профіль видалено!" });
            }

            var gameExists = await _context.Games.AnyAsync(g => g.Id == review.GameId && !g.IsArchived);
            if (!gameExists)
            {
                return BadRequest(new { message = $"Помилка: Ігри з ID {review.GameId} не існує в каталозі!" });
            }

            var alreadyReviewed = await _context.Reviews
                .AnyAsync(r => r.GameId == review.GameId && r.PlayerId == review.PlayerId && !r.IsDeleted);

            if (alreadyReviewed)
            {
                return BadRequest(new { message = "Ви вже залишали відгук для цієї гри!" });
            }

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Відгук успішно опубліковано!" });
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);

            if (review is null)
            {
                return NotFound(new { message = "Відгук не знайдено" });
            }

            review.IsDeleted = true;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Відгук видалений." });
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Review>>> GetReviews()
        {
            return await _context.Reviews
                .Where(r => !r.IsDeleted)
                .ToListAsync();
        }
    }
}