using Microsoft.EntityFrameworkCore;

namespace GameLibraryAPI.Models {
    public class GameContext : DbContext {
        public GameContext(DbContextOptions<GameContext> options) : base(options) {
            Database.EnsureCreated(); // Автоматично створить базу при запуску в Docker
        }

        public DbSet<Game> Games { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Review> Reviews { get; set; }
    }
}