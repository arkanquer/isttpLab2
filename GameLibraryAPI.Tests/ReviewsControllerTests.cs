using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using GameLibraryAPI.Controllers;
using GameLibraryAPI.Models;

namespace GameLibraryAPI.Tests
{
    public class ReviewsControllerTests
    {
        private GameContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<GameContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            return new GameContext(options);
        }
        [Fact]
        public async Task PostReview_AddsReviewSuccessfully()
        {
            using var context = GetInMemoryDbContext();
            context.Games.Add(new Game { Id = 1, Name = "Test Game for Review", GenreId = 1 });
            context.Players.Add(new Player { Id = 1, Nickname = "Reviewer", Email = "reviewer@mail.com" });
            await context.SaveChangesAsync();
            var controller = new ReviewsController(context);
            var newReview = new Review { Id = 1, GameId = 1, PlayerId = 1, Text = "Cool game!", Rating = 5 };
            var result = await controller.PostReview(newReview);
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(actionResult.Value);
        }
        [Fact]
        public async Task DeleteReview_ReturnsNotFound_WhenReviewDoesNotExist()
        {
            using var context = GetInMemoryDbContext();
            var controller = new ReviewsController(context);
            var result = await controller.DeleteReview(96446);
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}