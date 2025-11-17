using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingCareManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSupportChatParticipants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupportConversations_AspNetUsers_AppUserId",
                table: "SupportConversations");

            migrationBuilder.RenameColumn(
                name: "AppUserId",
                table: "SupportConversations",
                newName: "StaffId");

            migrationBuilder.RenameIndex(
                name: "IX_SupportConversations_AppUserId",
                table: "SupportConversations",
                newName: "IX_SupportConversations_StaffId");

            migrationBuilder.AddColumn<string>(
                name: "CustomerId",
                table: "SupportConversations",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "DoctorId",
                table: "SupportConversations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StaffRole",
                table: "SupportConversations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
UPDATE sc
SET CustomerId = COALESCE(NULLIF(CustomerId, ''), StaffId),
    StaffRole = CASE WHEN StaffRole = 0 THEN 1 ELSE StaffRole END
FROM SupportConversations sc
WHERE StaffId IS NOT NULL;
");

            migrationBuilder.CreateIndex(
                name: "IX_SupportConversations_CustomerId_StaffId",
                table: "SupportConversations",
                columns: new[] { "CustomerId", "StaffId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupportConversations_DoctorId",
                table: "SupportConversations",
                column: "DoctorId");

            migrationBuilder.AddForeignKey(
                name: "FK_SupportConversations_AspNetUsers_CustomerId",
                table: "SupportConversations",
                column: "CustomerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SupportConversations_AspNetUsers_StaffId",
                table: "SupportConversations",
                column: "StaffId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SupportConversations_Doctors_DoctorId",
                table: "SupportConversations",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupportConversations_AspNetUsers_CustomerId",
                table: "SupportConversations");

            migrationBuilder.DropForeignKey(
                name: "FK_SupportConversations_AspNetUsers_StaffId",
                table: "SupportConversations");

            migrationBuilder.DropForeignKey(
                name: "FK_SupportConversations_Doctors_DoctorId",
                table: "SupportConversations");

            migrationBuilder.DropIndex(
                name: "IX_SupportConversations_CustomerId_StaffId",
                table: "SupportConversations");

            migrationBuilder.DropIndex(
                name: "IX_SupportConversations_DoctorId",
                table: "SupportConversations");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "SupportConversations");

            migrationBuilder.DropColumn(
                name: "DoctorId",
                table: "SupportConversations");

            migrationBuilder.DropColumn(
                name: "StaffRole",
                table: "SupportConversations");

            migrationBuilder.RenameColumn(
                name: "StaffId",
                table: "SupportConversations",
                newName: "AppUserId");

            migrationBuilder.RenameIndex(
                name: "IX_SupportConversations_StaffId",
                table: "SupportConversations",
                newName: "IX_SupportConversations_AppUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_SupportConversations_AspNetUsers_AppUserId",
                table: "SupportConversations",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
