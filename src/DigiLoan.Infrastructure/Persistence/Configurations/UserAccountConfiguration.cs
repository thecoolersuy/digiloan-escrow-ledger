using DigiLoan.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigiLoan.Infrastructure.Persistence.Configurations;

public class UserAccountConfiguration : IEntityTypeConfiguration<UserAccount>
{
    public void Configure(EntityTypeBuilder<UserAccount> builder)
    {
        builder.HasIndex(a => a.AccountNumber).IsUnique();

        builder.Property(a => a.CurrentBalance).HasColumnType("decimal(18,2)");

        builder.Property(a => a.RowVersion).IsRowVersion();

        builder.HasMany(a => a.LoanApplications)
          .WithOne(l => l.UserAccount)
          .HasForeignKey(l => l.UserAccountId)
          .OnDelete(DeleteBehavior.Restrict);
    }
}