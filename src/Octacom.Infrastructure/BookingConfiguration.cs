using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Octacom.Domain;
using Octacom.Domain.ValueObjects;

namespace Octacom.Infrastructure;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .ValueGeneratedNever();

        builder.Property(b => b.ConferenceId)
            .IsRequired();

        builder.Property(b => b.AttendeeName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.AttendeeEmail)
            .IsRequired()
            .HasMaxLength(256)
            .HasConversion(
                email => email.Value,
                value => new Email(value)
            );

        builder.Property(b => b.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(b => b.BookedAt)
            .IsRequired();

        builder.Property(b => b.CancelledAt)
            .IsRequired(false);

        builder.HasIndex(b => new { b.ConferenceId, b.AttendeeEmail })
            .IsUnique()
            .HasFilter("[Status] = 'Confirmed'");
    }
}