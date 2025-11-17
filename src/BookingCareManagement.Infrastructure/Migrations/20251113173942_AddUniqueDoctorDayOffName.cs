using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingCareManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueDoctorDayOffName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Eliminate duplicates so the upcoming unique index creation cannot fail at runtime.
            migrationBuilder.Sql(
                sql: "WITH RankedDayOffs AS (SELECT Id, ROW_NUMBER() OVER (PARTITION BY DoctorId, Name ORDER BY Id) AS RN FROM DoctorDayOffs) DELETE FROM DoctorDayOffs WHERE Id IN (SELECT Id FROM RankedDayOffs WHERE RN > 1)");

            migrationBuilder.DropIndex(
                name: "IX_DoctorDayOffs_DoctorId",
                table: "DoctorDayOffs");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorDayOffs_DoctorId_Name",
                table: "DoctorDayOffs",
                columns: new[] { "DoctorId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DoctorDayOffs_DoctorId_Name",
                table: "DoctorDayOffs");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorDayOffs_DoctorId",
                table: "DoctorDayOffs",
                column: "DoctorId");
        }
    }
}
