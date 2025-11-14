using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingCareManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LinkPatientToAspUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PatientId",
                table: "Appointments",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.Sql("UPDATE Appointments SET PatientId = NULL WHERE PatientId = ''");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PatientId",
                table: "Appointments",
                column: "PatientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_AspNetUsers_PatientId",
                table: "Appointments",
                column: "PatientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_AspNetUsers_PatientId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_PatientId",
                table: "Appointments");

            migrationBuilder.Sql("UPDATE Appointments SET PatientId = '' WHERE PatientId IS NULL");

            migrationBuilder.AlterColumn<string>(
                name: "PatientId",
                table: "Appointments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);
        }
    }
}
