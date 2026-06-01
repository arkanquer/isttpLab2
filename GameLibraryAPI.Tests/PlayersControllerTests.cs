using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using GameLibraryAPI.Controllers;
using GameLibraryAPI.Models;

namespace GameLibraryAPI.Tests
{
    public class PlayersControllerTests
    {
        private GameContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<GameContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            return new GameContext(options);
        }

        [Fact]
        public async Task PostPlayer_CreatesPlayerSuccessfully_WhenDataIsValid()
        {
            using var context = GetInMemoryDbContext();
            var controller = new PlayersController(context);
            var newPlayer = new Player { Id = 1, Nickname = "Gamer123", Email = "gamer@mail.com" };
            var result = await controller.RegisterPlayer(newPlayer);
            var actionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var createdPlayer = Assert.IsType<Player>(actionResult.Value);
            Assert.Equal("Gamer123", createdPlayer.Nickname);
        }

        [Fact]
        public async Task GetPlayers_ReturnsEmptyList_WhenNoPlayersInDatabase()
        {
            using var context = GetInMemoryDbContext();
            var controller = new PlayersController(context);
            var result = await controller.GetPlayers();
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Player>>>(result);
            var playersList = Assert.IsAssignableFrom<IEnumerable<Player>>(actionResult.Value);
            Assert.Empty(playersList);
        }
    }
}