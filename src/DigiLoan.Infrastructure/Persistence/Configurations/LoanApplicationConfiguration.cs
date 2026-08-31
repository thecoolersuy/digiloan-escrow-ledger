using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DigiLoan.Domain.Entities;
namespace DigiLoan.Infrastructure.Persistence.Configurations;

public class LoanApplicationConfiguration : IEntityTypeConfiguration<LoanApplication>
{
    public void Configure(EntityTypeBuilder<LoanApplication> builder)
    {
        builder.Property(e => e.RequestedAmount).HasColumnType("decimal(18,2)");

        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);

        builder.Property(l => l.RiskScore).HasColumnType("decimal(18,2)");


    }
}