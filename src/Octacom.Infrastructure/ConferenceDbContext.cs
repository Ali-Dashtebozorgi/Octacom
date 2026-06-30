using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Octacom.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Octacom.Infrastructure;

public class ConferenceDbContext : DbContext
{
    public ConferenceDbContext(DbContextOptions<ConferenceDbContext> options) : base(options)
    {
    }

    public DbSet<Conference> Conferences { get; set; }
    public DbSet<Booking> Bookings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
        modelBuilder.ApplyConfiguration(new ConferenceConfiguration());
        modelBuilder.ApplyConfiguration(new BookingConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}

public class ConferenceConfiguration : IEntityTypeConfiguration<Conference>
{
    public void Configure(EntityTypeBuilder<Conference> builder)
    {
        builder.ToTable("Conferences");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.TotalCapacity)
            .IsRequired();

        builder.Property(x => x.BookedSeats)
            .IsRequired();

        builder.Property(x => x.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasMany(x => x.Bookings)
            .WithOne()
            .HasForeignKey(z => z.ConferenceId)
            .OnDelete(deleteBehavior: DeleteBehavior.Cascade); 
    }
}