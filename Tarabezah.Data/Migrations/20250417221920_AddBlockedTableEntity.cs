using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tarabezah.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBlockedTableEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlockedTables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FloorplanElementInstanceId = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockedTables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlockedTables_FloorplanElements_FloorplanElementInstanceId",
                        column: x => x.FloorplanElementInstanceId,
                        principalTable: "FloorplanElements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlockedTables_FloorplanElementInstanceId",
                table: "BlockedTables",
                column: "FloorplanElementInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_BlockedTables_Guid",
                table: "BlockedTables",
                column: "Guid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlockedTables");
        }
    }
}
