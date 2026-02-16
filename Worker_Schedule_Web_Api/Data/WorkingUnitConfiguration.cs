using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Worker_Schedule_Web_Api.Models.Domain;

namespace Worker_Schedule_Web_Api.Data
{
    public class WorkingUnitConfiguration : IEntityTypeConfiguration<WorkingUnit>
    {
        public void Configure(EntityTypeBuilder<WorkingUnit> builder)
        {
            builder.HasKey(wu => wu.Id);
            builder.Property(wu => wu.From)
                .HasColumnType("time");
            builder.Property(wu => wu.To)
                .HasColumnType("time");
        }
    }
}
