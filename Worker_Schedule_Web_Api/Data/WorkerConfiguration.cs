using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Worker_Schedule_Web_Api.Models.Domain;

namespace Worker_Schedule_Web_Api.Data
{
    public class WorkerConfiguration : IEntityTypeConfiguration<Worker>
    {
        public void Configure(EntityTypeBuilder<Worker> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasOne(w => w.Store)
                .WithMany(s => s.Workers)
                .HasForeignKey(w => w.StoreId)
                .OnDelete(DeleteBehavior.SetNull);
            builder.HasOne(w => w.Position)
                .WithMany(p => p.Workers)
                .HasForeignKey(w => w.PositionId)
                .OnDelete(DeleteBehavior.SetNull);
            builder.HasOne(w => w.AppUser)
                .WithOne(u => u.Worker)
                .HasForeignKey<Worker>(w => w.AppUserId)
                .IsRequired();
            builder.Property(w => w.WorkerInternalNumber)
                .IsRequired();
            builder.HasIndex(w => w.WorkerInternalNumber)
                .IsUnique();
            builder.Property(w => w.FirstName)
                .HasMaxLength(50);
            builder.Property(w => w.LastName)
                .HasMaxLength(75);
        }
    }
}
