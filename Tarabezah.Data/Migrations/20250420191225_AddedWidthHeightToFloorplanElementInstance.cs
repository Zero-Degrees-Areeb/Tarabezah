using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tarabezah.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedWidthHeightToFloorplanElementInstance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Height",
                table: "FloorplanElements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Width",
                table: "FloorplanElements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "FloorplanElements",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Height", "Width" },
                values: new object[] { 0, 0 });

            migrationBuilder.UpdateData(
                table: "FloorplanElements",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Height", "Width" },
                values: new object[] { 0, 0 });

            migrationBuilder.UpdateData(
                table: "FloorplanElements",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Height", "Width" },
                values: new object[] { 0, 0 });

            migrationBuilder.UpdateData(
                table: "FloorplanElements",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Height", "Width" },
                values: new object[] { 0, 0 });

            migrationBuilder.UpdateData(
                table: "FloorplanElements",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Height", "Width" },
                values: new object[] { 0, 0 });

            migrationBuilder.UpdateData(
                table: "FloorplanElements",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Height", "Width" },
                values: new object[] { 0, 0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Height",
                table: "FloorplanElements");

            migrationBuilder.DropColumn(
                name: "Width",
                table: "FloorplanElements");
        }
    }
}
