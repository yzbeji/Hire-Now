using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HireNow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateListingModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[] { -1, "Other" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: -1);
        }
    }
}
