using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tarabezah.Data.Migrations
{
    public partial class DropFloorplanElementTableIdUniqueIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the existing unique index
            migrationBuilder.DropIndex(
                name: "IX_FloorplanElements_FloorplanId_TableId",
                table: "FloorplanElements");

            // Create a new non-unique index
            migrationBuilder.CreateIndex(
                name: "IX_FloorplanElements_FloorplanId_TableId",
                table: "FloorplanElements",
                columns: new[] { "FloorplanId", "TableId" },
                unique: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the non-unique index
            migrationBuilder.DropIndex(
                name: "IX_FloorplanElements_FloorplanId_TableId",
                table: "FloorplanElements");

            // Recreate the unique index
            migrationBuilder.CreateIndex(
                name: "IX_FloorplanElements_FloorplanId_TableId",
                table: "FloorplanElements",
                columns: new[] { "FloorplanId", "TableId" },
                unique: true);
        }
    }
}