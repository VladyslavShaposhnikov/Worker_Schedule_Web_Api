using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Worker_Schedule_Web_Api.Migrations
{
    /// <inheritdoc />
    public partial class FixScheduleForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_Workers_WorkingUnitId",
                table: "Schedules");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "Date",
                table: "ShiftDemands",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1),
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_WorkerId",
                table: "Schedules",
                column: "WorkerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_Workers_WorkerId",
                table: "Schedules",
                column: "WorkerId",
                principalTable: "Workers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_Workers_WorkerId",
                table: "Schedules");

            migrationBuilder.DropIndex(
                name: "IX_Schedules_WorkerId",
                table: "Schedules");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "Date",
                table: "ShiftDemands",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_Workers_WorkingUnitId",
                table: "Schedules",
                column: "WorkingUnitId",
                principalTable: "Workers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
