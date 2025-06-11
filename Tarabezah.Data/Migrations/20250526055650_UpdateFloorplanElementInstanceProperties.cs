using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tarabezah.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFloorplanElementInstanceProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Y",
                table: "FloorplanElements",
                type: "float",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<double>(
                name: "X",
                table: "FloorplanElements",
                type: "float",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<double>(
                name: "Width",
                table: "FloorplanElements",
                type: "float",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<double>(
                name: "Rotation",
                table: "FloorplanElements",
                type: "float",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<double>(
                name: "Height",
                table: "FloorplanElements",
                type: "float",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.UpdateData(
                table: "FloorplanElements",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Height", "Rotation", "Width", "X", "Y" },
                values: new object[] { 0.0, 0.0, 0.0, 100.0, 150.0 });

            migrationBuilder.UpdateData(
                table: "FloorplanElements",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Height", "Rotation", "Width", "X", "Y" },
                values: new object[] { 0.0, 0.0, 0.0, 250.0, 150.0 });

            migrationBuilder.UpdateData(
                table: "FloorplanElements",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Height", "Rotation", "Width", "X", "Y" },
                values: new object[] { 0.0, 90.0, 0.0, 50.0, 50.0 });

            migrationBuilder.UpdateData(
                table: "FloorplanElements",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Height", "Rotation", "Width", "X", "Y" },
                values: new object[] { 0.0, 0.0, 0.0, 100.0, 100.0 });

            migrationBuilder.UpdateData(
                table: "FloorplanElements",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Height", "Rotation", "Width", "X", "Y" },
                values: new object[] { 0.0, 45.0, 0.0, 150.0, 200.0 });

            migrationBuilder.UpdateData(
                table: "FloorplanElements",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Height", "Rotation", "Width", "X", "Y" },
                values: new object[] { 0.0, 0.0, 0.0, 75.0, 125.0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Y",
                table: "FloorplanElements",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<int>(
                name: "X",
                table: "FloorplanElements",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<int>(
                name: "Width",
                table: "FloorplanElements",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<int>(
                name: "Rotation",
                table: "FloorplanElements",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<int>(
                name: "Height",
                table: "FloorplanElements",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.UpdateData(
                table: "FloorplanElements",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Height", "Rotation", "Width", "X", "Y" },
                values: new object[] { 0, 0, 0, 100, 150 });

            migrationBuilder.UpdateData(
                table: "FloorplanElements",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Height", "Rotation", "Width", "X", "Y" },
                values: new object[] { 0, 0, 0, 250, 150 });

            migrationBuilder.UpdateData(
                table: "FloorplanElements",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Height", "Rotation", "Width", "X", "Y" },
                values: new object[] { 0, 90, 0, 50, 50 });

            migrationBuilder.UpdateData(
                table: "FloorplanElements",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Height", "Rotation", "Width", "X", "Y" },
                values: new object[] { 0, 0, 0, 100, 100 });

            migrationBuilder.UpdateData(
                table: "FloorplanElements",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Height", "Rotation", "Width", "X", "Y" },
                values: new object[] { 0, 45, 0, 150, 200 });

            migrationBuilder.UpdateData(
                table: "FloorplanElements",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Height", "Rotation", "Width", "X", "Y" },
                values: new object[] { 0, 0, 0, 75, 125 });
        }
    }
}
