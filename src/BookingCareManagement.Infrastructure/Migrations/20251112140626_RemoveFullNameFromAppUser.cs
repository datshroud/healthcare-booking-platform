using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingCareManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFullNameFromAppUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"UPDATE u
                  SET FirstName = CASE
                        WHEN (u.FirstName IS NULL OR LTRIM(RTRIM(u.FirstName)) = '') AND LTRIM(RTRIM(u.FullName)) <> ''
                             THEN CASE
                                 WHEN CHARINDEX(' ', LTRIM(RTRIM(u.FullName))) = 0 THEN LTRIM(RTRIM(u.FullName))
                                 ELSE LEFT(LTRIM(RTRIM(u.FullName)), LEN(LTRIM(RTRIM(u.FullName))) - CHARINDEX(' ', REVERSE(LTRIM(RTRIM(u.FullName)))))
                             END
                        ELSE u.FirstName
                    END,
                      LastName = CASE
                        WHEN (u.LastName IS NULL OR LTRIM(RTRIM(u.LastName)) = '') AND LTRIM(RTRIM(u.FullName)) <> ''
                             THEN CASE
                                 WHEN CHARINDEX(' ', LTRIM(RTRIM(u.FullName))) = 0 THEN ''
                                 ELSE RIGHT(LTRIM(RTRIM(u.FullName)), CHARINDEX(' ', REVERSE(LTRIM(RTRIM(u.FullName)))) - 1)
                             END
                        ELSE u.LastName
                    END
                  FROM AspNetUsers u
                  WHERE u.FullName IS NOT NULL");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "AspNetUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.Sql(
                @"UPDATE AspNetUsers
                  SET FullName = LTRIM(RTRIM(
                        COALESCE(NULLIF(LTRIM(RTRIM(FirstName)), ''), '') +
                        CASE
                            WHEN COALESCE(NULLIF(LTRIM(RTRIM(FirstName)), ''), '') <> '' AND COALESCE(NULLIF(LTRIM(RTRIM(LastName)), ''), '') <> ''
                                THEN ' '
                            ELSE ''
                        END +
                        COALESCE(NULLIF(LTRIM(RTRIM(LastName)), ''), '')
                  ))");
        }
    }
}
