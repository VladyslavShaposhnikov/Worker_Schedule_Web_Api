using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Worker_Schedule_Web_Api.Models.Domain;

namespace Worker_Schedule_Web_Api.Data
{
    public class AddressConfiguration : IEntityTypeConfiguration<Address>
    {
        public void Configure(EntityTypeBuilder<Address> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(a => a.Country)
                .HasMaxLength(100)
                .IsRequired();
            builder.Property(a => a.City)
                .HasMaxLength(100)
                .IsRequired();
            builder.Property(a => a.ZipCode)
                .HasMaxLength(10)
                .IsRequired();
            builder.Property(a => a.Street)
                .HasMaxLength(120)
                .IsRequired();
            builder.Property(a => a.BuildingNumber)
                .HasMaxLength(10)
                .IsRequired();
            builder.Property(a => a.Apartment)
                .HasMaxLength(10);

            builder.HasData(new Address
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Country = "Poland",
                City = "Cracov",
                ZipCode = "30-644",
                Street = "Kaminskiego",
                BuildingNumber = "11"
            });
        }
    }
}
