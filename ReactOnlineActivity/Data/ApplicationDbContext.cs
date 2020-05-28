using Game.Domain;
using ReactOnlineActivity.Models;
using IdentityServer4.EntityFramework.Options;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ReactOnlineActivity.Data
{
    public class ApplicationDbContext : ApiAuthorizationDbContext<ApplicationUser>
    {
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Theme> Themes { get; set; }

        public ApplicationDbContext(
            DbContextOptions options,
            IOptions<OperationalStoreOptions> operationalStoreOptions) : base(options, operationalStoreOptions)
        {
            Database.EnsureDeleted();
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ThemeRoomSettings>()
                .HasKey(t => new { t.RoomSettingsId, t.ThemeId });

            modelBuilder.Entity<ThemeRoomSettings>()
                .HasOne(sc => sc.RoomSettings)
                .WithMany(s => s.ThemeRoomSettings)
                .HasForeignKey(sc => sc.ThemeId);

            modelBuilder.Entity<ThemeRoomSettings>()
                .HasOne(sc => sc.Theme)
                .WithMany(c => c.ThemeRoomSettings)
                .HasForeignKey(sc => sc.RoomSettingsId);

            base.OnModelCreating(modelBuilder);
        }
    }
}