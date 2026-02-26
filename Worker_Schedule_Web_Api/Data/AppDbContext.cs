using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Worker_Schedule_Web_Api.Models.Domain;
using Worker_Schedule_Web_Api.Models.Identity;

namespace Worker_Schedule_Web_Api.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration configuration) : IdentityDbContext<AppUser>(options)
    {
        public DbSet<Worker> Workers => Set<Worker>();
        public DbSet<Position> Positions => Set<Position>();
        public DbSet<Store> Stores => Set<Store>();
        public DbSet<Address> Addresses => Set<Address>();
        public DbSet<Schedule> Schedules => Set<Schedule>();
        public DbSet<Availability> Availabilities => Set<Availability>();
        public DbSet<ShiftDemand> ShiftDemands => Set<ShiftDemand>();
        public DbSet<WorkingUnit> WorkingUnits => Set<WorkingUnit>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfiguration(new WorkerConfiguration());
            builder.ApplyConfiguration(new AddressConfiguration());
            builder.ApplyConfiguration(new AvailabilityConfiguration());
            builder.ApplyConfiguration(new PositionConfiguration());
            builder.ApplyConfiguration(new ScheduleConfiguration());
            builder.ApplyConfiguration(new ShiftDemandConfiguration());
            builder.ApplyConfiguration(new StoreConfiguration());
            builder.ApplyConfiguration(new WorkingUnitConfiguration());
        }
    }
}
