using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingCareManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CustomerBookingStatusLocalization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Appointments",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Pending",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldDefaultValue: "Confirmed");

            migrationBuilder.Sql("UPDATE [Appointments] SET [Status] = 'Approved' WHERE [Status] = 'Confirmed';");
            migrationBuilder.Sql("UPDATE [Appointments] SET [Status] = 'Canceled' WHERE [Status] = 'Cancelled';");
            migrationBuilder.Sql("UPDATE [Appointments] SET [Status] = 'Pending' WHERE [Status] IS NULL OR LTRIM(RTRIM([Status])) = '';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Appointments",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Confirmed",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldDefaultValue: "Pending");

            migrationBuilder.Sql("UPDATE [Appointments] SET [Status] = 'Confirmed' WHERE [Status] = 'Approved';");
            migrationBuilder.Sql("UPDATE [Appointments] SET [Status] = 'Cancelled' WHERE [Status] = 'Canceled';");
        }
    }
}
