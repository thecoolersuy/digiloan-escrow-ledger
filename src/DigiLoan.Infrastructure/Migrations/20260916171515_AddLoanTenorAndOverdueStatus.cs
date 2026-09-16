using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigiLoan.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLoanTenorAndOverdueStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "InterestRate",
                table: "LoanApplications",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "LoanTenor",
                table: "LoanApplications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "RepaymentAmount",
                table: "LoanApplications",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "RepaymentDate",
                table: "LoanApplications",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InterestRate",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "LoanTenor",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "RepaymentAmount",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "RepaymentDate",
                table: "LoanApplications");
        }
    }
}
