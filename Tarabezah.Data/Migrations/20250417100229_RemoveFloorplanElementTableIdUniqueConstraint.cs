using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tarabezah.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFloorplanElementTableIdUniqueConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the existing unique index
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT *
                    FROM sys.indexes
                    WHERE name = 'IX_FloorplanElements_FloorplanId_TableId'
                    AND object_id = OBJECT_ID('FloorplanElements')
                )
                BEGIN
                    DROP INDEX IX_FloorplanElements_FloorplanId_TableId ON FloorplanElements;
                END");

            // Create a new non-unique index
            migrationBuilder.CreateIndex(
                name: "IX_FloorplanElements_FloorplanId",
                table: "FloorplanElements",
                column: "FloorplanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the non-unique index
            migrationBuilder.DropIndex(
                name: "IX_FloorplanElements_FloorplanId",
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
