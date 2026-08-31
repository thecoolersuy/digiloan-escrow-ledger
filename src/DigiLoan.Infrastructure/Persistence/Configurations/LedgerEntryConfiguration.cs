using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using DigiLoan.Domain.Entities;
using Microsoft.Identity.Client;

namespace DigiLoan.Infrastructure.Persistence.Configurations;

public class LedgerEntryConfiguration : IEntityTypeConfiguration<LedgerEntry>
{
    public void Configure(EntityTypeBuilder<LedgerEntry> builder)
    {
        builder.Property(a => a.Amount).HasColumnType("decimal(18,2)");

        builder.Property(a => a.TransactionType).HasConversion<string>().HasMaxLength(10);

        builder.HasOne(a => a.SourceAccount)
            .WithMany(a => a.OutgoingEntries)
            .HasForeignKey(a => a.SourceAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.DestinationAccount)
               .WithMany(a => a.IncomingEntries)
               .HasForeignKey(a => a.DestinationAccountId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}