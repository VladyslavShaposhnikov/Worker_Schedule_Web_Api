using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Worker_Schedule_Web_Api.Models.Domain;

namespace Worker_Schedule_Web_Api.Data
{
    public class ShiftDemandConfiguration : IEntityTypeConfiguration<ShiftDemand>
    {
        public void Configure(EntityTypeBuilder<ShiftDemand> builder)
        {
            builder.HasKey(sd => sd.Id);
            builder.Property(s => s.Date)
                .HasColumnType("date");
            builder.HasOne(sd => sd.WorkingUnit)
                .WithMany(wu => wu.ShiftDemands)
                .HasForeignKey(sd => sd.WorkingUnitId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
