using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tarabezah.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTableIdUniqueConstraintFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FloorplanElements_FloorplanId_TableId",
                table: "FloorplanElements");

            migrationBuilder.CreateIndex(
                name: "IX_FloorplanElements_FloorplanId",
                table: "FloorplanElements",
                column: "FloorplanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FloorplanElements_FloorplanId",
                table: "FloorplanElements");

            migrationBuilder.CreateIndex(
                name: "IX_FloorplanElements_FloorplanId_TableId",
                table: "FloorplanElements",
                columns: new[] { "FloorplanId", "TableId" });
        }
    }
}
