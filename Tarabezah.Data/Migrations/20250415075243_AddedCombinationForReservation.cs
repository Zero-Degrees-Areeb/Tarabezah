using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tarabezah.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedCombinationForReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // CombinedTableMemberId column is already added in the initial migration
            // Only create the index and foreign key
            migrationBuilder.CreateIndex(
                name: "IX_Reservations_CombinedTableMemberId",
                table: "Reservations",
                column: "CombinedTableMemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_CombinedTableMembers_CombinedTableMemberId",
                table: "Reservations",
                column: "CombinedTableMemberId",
                principalTable: "CombinedTableMembers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_CombinedTableMembers_CombinedTableMemberId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_CombinedTableMemberId",
                table: "Reservations");
        }
    }
}
