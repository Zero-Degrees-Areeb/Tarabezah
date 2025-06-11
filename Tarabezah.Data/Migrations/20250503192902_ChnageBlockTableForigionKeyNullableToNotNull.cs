using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tarabezah.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChnageBlockTableForigionKeyNullableToNotNull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BlockTables_FloorplanElements_FloorplanElementInstanceId",
                table: "BlockTables");

            migrationBuilder.AddForeignKey(
                name: "FK_BlockTables_FloorplanElements_FloorplanElementInstanceId",
                table: "BlockTables",
                column: "FloorplanElementInstanceId",
                principalTable: "FloorplanElements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BlockTables_FloorplanElements_FloorplanElementInstanceId",
                table: "BlockTables");

            migrationBuilder.AddForeignKey(
                name: "FK_BlockTables_FloorplanElements_FloorplanElementInstanceId",
                table: "BlockTables",
                column: "FloorplanElementInstanceId",
                principalTable: "FloorplanElements",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
