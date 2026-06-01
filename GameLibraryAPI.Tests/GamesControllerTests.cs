using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using GameLibraryAPI.Controllers;
using GameLibraryAPI.Models;

namespace GameLibraryAPI.Tests
{
    public class GamesControllerTests
    {
        private GameContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<GameContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            return new GameContext(options);
        }

        [Fact]
        public async Task ArchiveGame_ReturnsNotFound_WhenGameDoesNotExist()
        {
            using var context = GetInMemoryDbContext();
            var controller = new GamesController(context);
            int nonExistingGameId = 38295;
            var result = await controller.ArchiveGame(nonExistingGameId);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ArchiveGame_ChangesFlagAndReturnsOk_WhenGameExists()
        {
            using var context = GetInMemoryDbContext();
            var testGame = new Game { Id = 1, Name = "Test Game", IsArchived = false, GenreId = 1 };
            context.Games.Add(testGame);
            await context.SaveChangesAsync();
            var controller = new GamesController(context);
            var result = await controller.ArchiveGame(1);
            Assert.IsType<OkObjectResult>(result);
            var gameInDb = await context.Games.FindAsync(1);
            Assert.NotNull(gameInDb); 
            Assert.True(gameInDb.IsArchived);
        }

        [Fact]
        public async Task GetGames_ReturnsAllGames_WhenGamesExist()
        {
            using var context = GetInMemoryDbContext();
            context.Genres.Add(new Genre { Id = 1, Name = "RPG" });
            context.Games.Add(new Game { Id = 1, Name = "Witcher 3", GenreId = 1, IsArchived = false });
            context.Games.Add(new Game { Id = 2, Name = "Cyberpunk 2077", GenreId = 1, IsArchived = false });
            await context.SaveChangesAsync();
            var controller = new GamesController(context);
            var result = await controller.GetGames();
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Game>>>(result);
            var gamesList = Assert.IsAssignableFrom<IEnumerable<Game>>(actionResult.Value);
            Assert.Equal(2, ((List<Game>)gamesList).Count);
        }

        [Fact]
        public async Task GetGame_ReturnsNotFound_WhenIdIsInvalid()
        {
            using var context = GetInMemoryDbContext();
            var controller = new GamesController(context);
            var result = await controller.GetGame(7099);
            Assert.IsType<NotFoundResult>(result.Result);
        }
    }
}