using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Worker_Schedule_Web_Api.Models.Domain;

namespace Worker_Schedule_Web_Api.Data
{
    public class PositionConfiguration : IEntityTypeConfiguration<Position>
    {
        public void Configure(EntityTypeBuilder<Position> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(p => p.Name)
                .HasMaxLength(100)
                .IsRequired();
            builder.HasData(
                new Position { Id = Guid.Parse("11111111-1111-1111-1111-111111111112"), Name = "Manager" },
                new Position { Id = Guid.Parse("22222222-2222-2222-2222-222222222223"), Name = "Worker" },
                new Position { Id = Guid.Parse("33333333-3333-3333-3333-333333333334"), Name = "Visual merchandiser" }
            );
        }
    }
}
