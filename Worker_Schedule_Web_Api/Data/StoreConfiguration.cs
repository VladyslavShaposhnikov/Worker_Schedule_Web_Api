using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Worker_Schedule_Web_Api.Models.Domain;

namespace Worker_Schedule_Web_Api.Data
{
    public class StoreConfiguration : IEntityTypeConfiguration<Store>
    {
        public void Configure(EntityTypeBuilder<Store> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasOne(s => s.Address)
                .WithMany(a => a.Stores)
                .HasForeignKey(s => s.AddressId);
            builder.Property(s => s.Name)
                .HasMaxLength(200)
                .IsRequired();
        }
    }
}
