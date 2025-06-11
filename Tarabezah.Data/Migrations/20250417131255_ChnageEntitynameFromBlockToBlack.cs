using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tarabezah.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChnageEntitynameFromBlockToBlack : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BlockingClients_Clients_ClientId",
                table: "BlockingClients");

            migrationBuilder.DropForeignKey(
                name: "FK_BlockingClients_Restaurants_RestaurantId",
                table: "BlockingClients");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BlockingClients",
                table: "BlockingClients");

            migrationBuilder.RenameTable(
                name: "BlockingClients",
                newName: "BlackList");

            migrationBuilder.RenameIndex(
                name: "IX_BlockingClients_RestaurantId",
                table: "BlackList",
                newName: "IX_BlackList_RestaurantId");

            migrationBuilder.RenameIndex(
                name: "IX_BlockingClients_Guid",
                table: "BlackList",
                newName: "IX_BlackList_Guid");

            migrationBuilder.RenameIndex(
                name: "IX_BlockingClients_ClientId_RestaurantId",
                table: "BlackList",
                newName: "IX_BlackList_ClientId_RestaurantId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BlackList",
                table: "BlackList",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BlackList_Clients_ClientId",
                table: "BlackList",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BlackList_Restaurants_RestaurantId",
                table: "BlackList",
                column: "RestaurantId",
                principalTable: "Restaurants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BlackList_Clients_ClientId",
                table: "BlackList");

            migrationBuilder.DropForeignKey(
                name: "FK_BlackList_Restaurants_RestaurantId",
                table: "BlackList");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BlackList",
                table: "BlackList");

            migrationBuilder.RenameTable(
                name: "BlackList",
                newName: "BlockingClients");

            migrationBuilder.RenameIndex(
                name: "IX_BlackList_RestaurantId",
                table: "BlockingClients",
                newName: "IX_BlockingClients_RestaurantId");

            migrationBuilder.RenameIndex(
                name: "IX_BlackList_Guid",
                table: "BlockingClients",
                newName: "IX_BlockingClients_Guid");

            migrationBuilder.RenameIndex(
                name: "IX_BlackList_ClientId_RestaurantId",
                table: "BlockingClients",
                newName: "IX_BlockingClients_ClientId_RestaurantId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BlockingClients",
                table: "BlockingClients",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BlockingClients_Clients_ClientId",
                table: "BlockingClients",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BlockingClients_Restaurants_RestaurantId",
                table: "BlockingClients",
                column: "RestaurantId",
                principalTable: "Restaurants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
