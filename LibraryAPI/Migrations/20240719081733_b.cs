using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryAPI.Migrations
{
    /// <inheritdoc />
    public partial class b : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RemainingTime",
                table: "Borrow");

            migrationBuilder.AddColumn<decimal>(
                name: "PenaltyFee",
                table: "Borrow",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PenaltyFee",
                table: "Borrow");

            migrationBuilder.AddColumn<int>(
                name: "RemainingTime",
                table: "Borrow",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
