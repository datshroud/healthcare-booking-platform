using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingCareManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAvatarUrlAndDateOfBirthToAppUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'AvatarUrl' AND Object_ID = OBJECT_ID(N'[dbo].[AspNetUsers]'))
                  ALTER TABLE [AspNetUsers] ADD [AvatarUrl] nvarchar(max) NULL;"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'AvatarUrl' AND Object_ID = OBJECT_ID(N'[dbo].[AspNetUsers]'))
                  ALTER TABLE [AspNetUsers] DROP COLUMN [AvatarUrl];"
            );
        }
    }
}
