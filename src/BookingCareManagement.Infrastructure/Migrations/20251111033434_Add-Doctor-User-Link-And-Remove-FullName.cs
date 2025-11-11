using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingCareManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDoctorUserLinkAndRemoveFullName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DoctorSpecialties_Specialties_SpecialtyId",
                table: "DoctorSpecialties");

            migrationBuilder.DropForeignKey(
                name: "FK_Specialties_Doctors_DoctorId",
                table: "Specialties");

            migrationBuilder.DropIndex(
                name: "IX_Specialties_DoctorId",
                table: "Specialties");

            migrationBuilder.DropColumn(
                name: "DoctorId",
                table: "Specialties");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Doctors");

            migrationBuilder.RenameColumn(
                name: "SpecialtyId",
                table: "DoctorSpecialties",
                newName: "SpecialtiesId");

            migrationBuilder.RenameIndex(
                name: "IX_DoctorSpecialties_SpecialtyId",
                table: "DoctorSpecialties",
                newName: "IX_DoctorSpecialties_SpecialtiesId");

            migrationBuilder.AddColumn<string>(
                name: "AppUserId",
                table: "Doctors",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_AppUserId",
                table: "Doctors",
                column: "AppUserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Doctors_AspNetUsers_AppUserId",
                table: "Doctors",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorSpecialties_Specialties_SpecialtiesId",
                table: "DoctorSpecialties",
                column: "SpecialtiesId",
                principalTable: "Specialties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Doctors_AspNetUsers_AppUserId",
                table: "Doctors");

            migrationBuilder.DropForeignKey(
                name: "FK_DoctorSpecialties_Specialties_SpecialtiesId",
                table: "DoctorSpecialties");

            migrationBuilder.DropIndex(
                name: "IX_Doctors_AppUserId",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "AppUserId",
                table: "Doctors");

            migrationBuilder.RenameColumn(
                name: "SpecialtiesId",
                table: "DoctorSpecialties",
                newName: "SpecialtyId");

            migrationBuilder.RenameIndex(
                name: "IX_DoctorSpecialties_SpecialtiesId",
                table: "DoctorSpecialties",
                newName: "IX_DoctorSpecialties_SpecialtyId");

            migrationBuilder.AddColumn<Guid>(
                name: "DoctorId",
                table: "Specialties",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Doctors",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Specialties_DoctorId",
                table: "Specialties",
                column: "DoctorId");

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorSpecialties_Specialties_SpecialtyId",
                table: "DoctorSpecialties",
                column: "SpecialtyId",
                principalTable: "Specialties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Specialties_Doctors_DoctorId",
                table: "Specialties",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "Id");
        }
    }
}
