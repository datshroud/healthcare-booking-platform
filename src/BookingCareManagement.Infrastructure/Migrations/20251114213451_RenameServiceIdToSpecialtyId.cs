using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingCareManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameServiceIdToSpecialtyId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ServiceId",
                table: "Appointments",
                newName: "SpecialtyId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_SpecialtyId",
                table: "Appointments",
                column: "SpecialtyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Specialties_SpecialtyId",
                table: "Appointments",
                column: "SpecialtyId",
                principalTable: "Specialties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Specialties_SpecialtyId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_SpecialtyId",
                table: "Appointments");

            migrationBuilder.RenameColumn(
                name: "SpecialtyId",
                table: "Appointments",
                newName: "ServiceId");
        }
    }
}
