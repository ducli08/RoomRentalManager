using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RoomRentalManagerServer.Infrastructure.Data
{
    public class RoomRentalManagerServerDbContextFactory : IDesignTimeDbContextFactory<RoomRentalManagerServerDbContext>
    {
        public RoomRentalManagerServerDbContext CreateDbContext(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                ?? "Host=localhost;Port=5432;Database=RoomRentalManager;Username=postgres;Password=postgres";

            var optionsBuilder = new DbContextOptionsBuilder<RoomRentalManagerServerDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new RoomRentalManagerServerDbContext(optionsBuilder.Options);
        }
    }
}
