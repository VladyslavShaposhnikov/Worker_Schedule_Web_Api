using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Worker_Schedule_Web_Api.Migrations
{
    /// <inheritdoc />
    public partial class AddShiftDemandsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShiftDemands",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: true),
                    WorkersNeeded = table.Column<int>(type: "int", nullable: false),
                    WorkingUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftDemands", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShiftDemands_WorkingUnits_WorkingUnitId",
                        column: x => x.WorkingUnitId,
                        principalTable: "WorkingUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShiftDemands_WorkingUnitId",
                table: "ShiftDemands",
                column: "WorkingUnitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShiftDemands");
        }
    }
}
