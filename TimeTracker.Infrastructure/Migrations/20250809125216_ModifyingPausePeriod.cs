using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ModifyingPausePeriod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimeEntries_AspNetUsers_UserId",
                table: "TimeEntries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PausePeriods",
                table: "PausePeriods");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "PausePeriods",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PausePeriods",
                table: "PausePeriods",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_PausePeriods_TimeEntryId",
                table: "PausePeriods",
                column: "TimeEntryId");

            migrationBuilder.AddForeignKey(
                name: "FK_TimeEntries_AspNetUsers_UserId",
                table: "TimeEntries",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimeEntries_AspNetUsers_UserId",
                table: "TimeEntries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PausePeriods",
                table: "PausePeriods");

            migrationBuilder.DropIndex(
                name: "IX_PausePeriods_TimeEntryId",
                table: "PausePeriods");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "PausePeriods");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PausePeriods",
                table: "PausePeriods",
                columns: new[] { "TimeEntryId", "Start" });

            migrationBuilder.AddForeignKey(
                name: "FK_TimeEntries_AspNetUsers_UserId",
                table: "TimeEntries",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
