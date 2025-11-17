using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingCareManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSpecialtyColorAndDoctors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DoctorSpecialties_Doctors_DoctorId",
                table: "DoctorSpecialties");

            migrationBuilder.RenameColumn(
                name: "DoctorId",
                table: "DoctorSpecialties",
                newName: "DoctorsId");

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Specialties",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "#1a73e8");

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorSpecialties_Doctors_DoctorsId",
                table: "DoctorSpecialties",
                column: "DoctorsId",
                principalTable: "Doctors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DoctorSpecialties_Doctors_DoctorsId",
                table: "DoctorSpecialties");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "Specialties");

            migrationBuilder.RenameColumn(
                name: "DoctorsId",
                table: "DoctorSpecialties",
                newName: "DoctorId");

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorSpecialties_Doctors_DoctorId",
                table: "DoctorSpecialties",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
