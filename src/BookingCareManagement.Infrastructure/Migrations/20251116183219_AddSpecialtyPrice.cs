using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingCareManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSpecialtyPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Specialties",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "Specialties");
        }
    }
}
