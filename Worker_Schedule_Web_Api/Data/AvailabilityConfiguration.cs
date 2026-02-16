using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Worker_Schedule_Web_Api.Models.Domain;

namespace Worker_Schedule_Web_Api.Data
{
    public class AvailabilityConfiguration : IEntityTypeConfiguration<Availability>
    {
        public void Configure(EntityTypeBuilder<Availability> builder)
        {
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Date)
                .HasColumnType("date")
                .IsRequired();
            builder.HasOne(a => a.WorkingUnit)
                .WithMany(wu => wu.Availabilities)
                .HasForeignKey(a => a.WorkingUnitId);
            builder.HasOne(a => a.Worker)
                .WithMany(w => w.Availabilities)
                .HasForeignKey(a => a.WorkerId);
        }
    }
}
