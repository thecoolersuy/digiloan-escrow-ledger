using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigiLoan.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountNumberAndFullName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApplicationNumber",
                table: "LoanApplications",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplicationNumber",
                table: "LoanApplications");
        }
    }
}
