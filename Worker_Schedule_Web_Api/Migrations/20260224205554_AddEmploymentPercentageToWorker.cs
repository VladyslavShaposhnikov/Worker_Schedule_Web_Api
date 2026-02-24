using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Worker_Schedule_Web_Api.Migrations
{
    /// <inheritdoc />
    public partial class AddEmploymentPercentageToWorker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EmploymentPercentage",
                table: "Workers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmploymentPercentage",
                table: "Workers");
        }
    }
}
