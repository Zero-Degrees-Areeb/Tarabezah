using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tarabezah.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateNewTableForBlockTableFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlockTables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FloorplanElementInstanceId = table.Column<int>(type: "int", nullable: true),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockTables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlockTables_FloorplanElements_FloorplanElementInstanceId",
                        column: x => x.FloorplanElementInstanceId,
                        principalTable: "FloorplanElements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlockTables_FloorplanElementInstanceId",
                table: "BlockTables",
                column: "FloorplanElementInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_BlockTables_Guid",
                table: "BlockTables",
                column: "Guid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlockTables");
        }
    }
}
