using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Worker_Schedule_Web_Api.Models.Domain;

namespace Worker_Schedule_Web_Api.Data
{
    public class ScheduleConfiguration : IEntityTypeConfiguration<Schedule>
    {
        public void Configure(EntityTypeBuilder<Schedule> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(s => s.Date)
                .HasColumnType("date")
                .IsRequired();
            builder.HasOne(s => s.WorkingUnit)
                .WithMany(wu => wu.Schedules)
                .HasForeignKey(s => s.WorkingUnitId);
            builder.HasOne(s => s.Worker)
                .WithMany(w => w.Schedules)
                .HasForeignKey(s => s.WorkingUnitId);
        }
    }
}
