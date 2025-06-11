using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Tarabezah.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Birthday = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Source = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Tags = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Elements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TableType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Elements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Restaurants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Restaurants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Shifts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shifts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Floorplans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RestaurantId = table.Column<int>(type: "int", nullable: false),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Floorplans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Floorplans_Restaurants_RestaurantId",
                        column: x => x.RestaurantId,
                        principalTable: "Restaurants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RestaurantShifts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RestaurantId = table.Column<int>(type: "int", nullable: false),
                    ShiftId = table.Column<int>(type: "int", nullable: false),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantShifts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RestaurantShifts_Restaurants_RestaurantId",
                        column: x => x.RestaurantId,
                        principalTable: "Restaurants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RestaurantShifts_Shifts_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "Shifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CombinedTables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FloorplanId = table.Column<int>(type: "int", nullable: false),
                    GroupName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MinCapacity = table.Column<int>(type: "int", nullable: true),
                    MaxCapacity = table.Column<int>(type: "int", nullable: true),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CombinedTables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CombinedTables_Floorplans_FloorplanId",
                        column: x => x.FloorplanId,
                        principalTable: "Floorplans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FloorplanElements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FloorplanId = table.Column<int>(type: "int", nullable: false),
                    ElementId = table.Column<int>(type: "int", nullable: false),
                    TableId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MinCapacity = table.Column<int>(type: "int", nullable: false),
                    MaxCapacity = table.Column<int>(type: "int", nullable: false),
                    X = table.Column<int>(type: "int", nullable: false),
                    Y = table.Column<int>(type: "int", nullable: false),
                    Rotation = table.Column<int>(type: "int", nullable: false),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloorplanElements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FloorplanElements_Elements_ElementId",
                        column: x => x.ElementId,
                        principalTable: "Elements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FloorplanElements_Floorplans_FloorplanId",
                        column: x => x.FloorplanId,
                        principalTable: "Floorplans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CombinedTableMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CombinedTableId = table.Column<int>(type: "int", nullable: false),
                    FloorplanElementInstanceId = table.Column<int>(type: "int", nullable: false),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CombinedTableMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CombinedTableMembers_CombinedTables_CombinedTableId",
                        column: x => x.CombinedTableId,
                        principalTable: "CombinedTables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CombinedTableMembers_FloorplanElements_FloorplanElementInstanceId",
                        column: x => x.FloorplanElementInstanceId,
                        principalTable: "FloorplanElements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<int>(type: "int", nullable: true),
                    ShiftId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Time = table.Column<TimeSpan>(type: "time", nullable: false),
                    PartySize = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Tags = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ReservedElementId = table.Column<int>(type: "int", nullable: true),
                    CombinedTableMemberId = table.Column<int>(type: "int", nullable: true),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservations_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reservations_CombinedTableMembers_CombinedTableMemberId",
                        column: x => x.CombinedTableMemberId,
                        principalTable: "CombinedTableMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Reservations_FloorplanElements_ReservedElementId",
                        column: x => x.ReservedElementId,
                        principalTable: "FloorplanElements",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Reservations_Shifts_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "Shifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Clients",
                columns: new[] { "Id", "Birthday", "CreatedDate", "Email", "Guid", "ModifiedDate", "Name", "Notes", "PhoneNumber", "Source", "Tags" },
                values: new object[,]
                {
                    { 1, new DateTime(1985, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "john.smith@example.com", new Guid("a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d"), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "John Smith", "Prefers window seating", "+1-555-123-4567", "Website", "vip,wine lover" },
                    { 2, new DateTime(1990, 3, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "sara.johnson@example.com", new Guid("b2c3d4e5-f6a7-8b9c-0d1e-2f3a4b5c6d7e"), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sara Johnson", "Allergic to nuts", "+1-555-987-6543", "Instagram", "vegetarian,birthday" },
                    { 3, new DateTime(1978, 9, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "michael.chen@example.com", new Guid("c3d4e5f6-a7b8-9c0d-1e2f-3a4b5c6d7e8f"), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Michael Chen", "Celebrates anniversary on September 22", "+1-555-456-7890", "Facebook", "bbq lover,regular" },
                    { 4, new DateTime(1992, 12, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "jennifer.garcia@example.com", new Guid("d4e5f6a7-b8c9-0d1e-2f3a-4b5c6d7e8f9a"), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Jennifer Garcia", "Prefers quiet corner tables", "+1-555-789-0123", "Referral", "pescatarian,quiet table" },
                    { 5, null, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "robert.williams@example.com", new Guid("e5f6a7b8-c9d0-1e2f-3a4b-5c6d7e8f9a0b"), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Robert Williams", "Frequently books for business meetings", "+1-555-234-5678", "WalkIn", "business,wine lover" }
                });

            migrationBuilder.InsertData(
                table: "Elements",
                columns: new[] { "Id", "CreatedDate", "Guid", "ImageUrl", "ModifiedDate", "Name", "Purpose", "TableType" },
                values: new object[,]
                {
                    { 1, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("f1e32900-5e22-4824-8adb-e1c50e976a23"), "/images/elements/round-table.png", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Round Table", "Reservable", "Round" },
                    { 2, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("28be68e2-a4d1-4a33-b0d6-f2d603fa0b41"), "/images/elements/square-table.png", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Square Table", "Reservable", "Square" },
                    { 3, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("a45c7f08-8b04-4d3e-8d23-f9e274a7c546"), "/images/elements/chair.png", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Chair", "Decorative", "Custom" },
                    { 4, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("e28b7af4-6f77-4a42-b4f6-29e7fd6ad0a9"), "/images/elements/wall.png", new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Wall", "Decorative", "Custom" }
                });

            migrationBuilder.InsertData(
                table: "Restaurants",
                columns: new[] { "Id", "CreatedDate", "Guid", "ModifiedDate", "Name" },
                values: new object[,]
                {
                    { 1, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("a7fa1095-d8c5-4d00-8a44-7ba684eae835"), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Italian Bistro" },
                    { 2, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("b2e7c6f0-d98c-4e5d-9a83-bc9429ab4187"), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Seaside Grill" },
                    { 3, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c3d8e9f2-a04b-4c1e-8a75-1d0e7f35b281"), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Downtown Cafe" }
                });

            migrationBuilder.InsertData(
                table: "Shifts",
                columns: new[] { "Id", "CreatedDate", "EndTime", "Guid", "ModifiedDate", "Name", "StartTime" },
                values: new object[,]
                {
                    { 1, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new TimeSpan(0, 11, 0, 0, 0), new Guid("d1e23f45-6789-4a0b-b1c2-d3e4f5a6b7c8"), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Breakfast", new TimeSpan(0, 7, 0, 0, 0) },
                    { 2, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new TimeSpan(0, 15, 0, 0, 0), new Guid("e2f34a56-789b-4c0d-e1f2-a3b4c5d6e7f8"), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Lunch", new TimeSpan(0, 11, 30, 0, 0) },
                    { 3, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new TimeSpan(0, 23, 0, 0, 0), new Guid("f3a45b67-8c9d-4e0f-a1b2-c3d4e5f6a7b8"), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Dinner", new TimeSpan(0, 17, 0, 0, 0) }
                });

            migrationBuilder.InsertData(
                table: "Floorplans",
                columns: new[] { "Id", "CreatedDate", "Guid", "ModifiedDate", "Name", "RestaurantId" },
                values: new object[,]
                {
                    { 1, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("d4f9a1b2-c03e-4f5a-8b67-9a0e7f2c3d45"), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Main Floor", 1 },
                    { 2, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("e5b0c1d2-a03f-4e5b-9c78-0b1f2d3e4a56"), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Patio", 1 },
                    { 3, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("f6c1d2e3-b04f-5e6c-0d89-1c2f3e4d5a67"), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Dining Room", 2 },
                    { 4, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d"), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bar Area", 3 }
                });

            migrationBuilder.InsertData(
                table: "Reservations",
                columns: new[] { "Id", "ClientId", "CombinedTableMemberId", "CreatedDate", "Date", "Guid", "ModifiedDate", "Notes", "PartySize", "ReservedElementId", "ShiftId", "Status", "Tags", "Time", "Type" },
                values: new object[,]
                {
                    { 1, 1, null, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2023, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("f6a7b8c9-d0e1-2f3a-4b5c-6d7e8f9a0b1c"), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Celebrating wedding anniversary", 2, null, 3, null, "anniversary,window seat", new TimeSpan(0, 19, 0, 0, 0), 0 },
                    { 2, 2, null, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2023, 1, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("a7b8c9d0-e1f2-3a4b-5c6d-7e8f9a0b1c2d"), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Birthday celebration - bringing own cake", 4, null, 2, null, "birthday,cake", new TimeSpan(0, 12, 30, 0, 0), 0 },
                    { 3, 3, null, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2023, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b8c9d0e1-f2a3-4b5c-6d7e-8f9a0b1c2d3e"), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Family gathering, requests BBQ specials", 6, null, 3, null, "family,bbq", new TimeSpan(0, 20, 0, 0, 0), 0 },
                    { 4, 4, null, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2023, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("c9d0e1f2-a3b4-5c6d-7e8f-9a0b1c2d3e4f"), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Pescatarian menu options requested", 2, null, 2, null, "quiet corner,pescatarian", new TimeSpan(0, 13, 0, 0, 0), 0 },
                    { 5, 5, null, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2023, 1, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d0e1f2a3-b4c5-6d7e-8f9a-0b1c2d3e4f5a"), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Business dinner, wine pairing recommended", 8, null, 3, null, "business,wine pairing", new TimeSpan(0, 18, 30, 0, 0), 0 },
                    { 6, 1, null, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2023, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("e1f2a3b4-c5d6-7e8f-9a0b-1c2d3e4f5a6b"), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Business lunch, time-constrained", 4, null, 2, null, "business,quick service", new TimeSpan(0, 12, 0, 0, 0), 0 }
                });

            migrationBuilder.InsertData(
                table: "RestaurantShifts",
                columns: new[] { "Id", "CreatedDate", "Guid", "ModifiedDate", "RestaurantId", "ShiftId" },
                values: new object[,]
                {
                    { 1, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d"), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 1 },
                    { 2, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("b2c3d4e5-f6a7-8b9c-0d1e-2f3a4b5c6d7e"), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 2 },
                    { 3, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c3d4e5f6-a7b8-9c0d-1e2f-3a4b5c6d7e8f"), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 3 },
                    { 4, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("d4e5f6a7-b8c9-0d1e-2f3a-4b5c6d7e8f9a"), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 2 },
                    { 5, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("e5f6a7b8-c9d0-1e2f-3a4b-5c6d7e8f9a0b"), new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 3 }
                });

            migrationBuilder.InsertData(
                table: "FloorplanElements",
                columns: new[] { "Id", "CreatedDate", "ElementId", "FloorplanId", "Guid", "MaxCapacity", "MinCapacity", "ModifiedDate", "Rotation", "TableId", "X", "Y" },
                values: new object[,]
                {
                    { 1, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 1, new Guid("b7c8d9e0-f1a2-3b4c-5d6e-7f8a9b0c1d2e"), 4, 2, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, "T1", 100, 150 },
                    { 2, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 1, new Guid("c8d9e0f1-a2b3-4c5d-6e7f-8a9b0c1d2e3f"), 6, 4, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, "T2", 250, 150 },
                    { 3, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 1, new Guid("d9e0f1a2-b3c4-5d6e-7f8a-9b0c1d2e3f4a"), 0, 0, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 90, "W1", 50, 50 },
                    { 4, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 2, new Guid("e0f1a2b3-c4d5-6e7f-8a9b-0c1d2e3f4a5b"), 4, 2, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, "P1", 100, 100 },
                    { 5, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 3, new Guid("f1a2b3c4-d5e6-7f8a-9b0c-1d2e3f4a5b6c"), 8, 4, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 45, "D1", 150, 200 },
                    { 6, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 4, new Guid("a2b3c4d5-e6f7-8a9b-0c1d-2e3f4a5b6c7d"), 2, 2, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, "B1", 75, 125 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Guid",
                table: "Clients",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CombinedTableMembers_CombinedTableId_FloorplanElementInstanceId",
                table: "CombinedTableMembers",
                columns: new[] { "CombinedTableId", "FloorplanElementInstanceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CombinedTableMembers_FloorplanElementInstanceId",
                table: "CombinedTableMembers",
                column: "FloorplanElementInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_CombinedTableMembers_Guid",
                table: "CombinedTableMembers",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CombinedTables_FloorplanId",
                table: "CombinedTables",
                column: "FloorplanId");

            migrationBuilder.CreateIndex(
                name: "IX_CombinedTables_Guid",
                table: "CombinedTables",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Elements_Guid",
                table: "Elements",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FloorplanElements_ElementId",
                table: "FloorplanElements",
                column: "ElementId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorplanElements_FloorplanId_TableId",
                table: "FloorplanElements",
                columns: new[] { "FloorplanId", "TableId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FloorplanElements_Guid",
                table: "FloorplanElements",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Floorplans_Guid",
                table: "Floorplans",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Floorplans_RestaurantId",
                table: "Floorplans",
                column: "RestaurantId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ClientId",
                table: "Reservations",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_CombinedTableMemberId",
                table: "Reservations",
                column: "CombinedTableMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_Date",
                table: "Reservations",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_Guid",
                table: "Reservations",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ReservedElementId",
                table: "Reservations",
                column: "ReservedElementId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ShiftId",
                table: "Reservations",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_Guid",
                table: "Restaurants",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantShifts_Guid",
                table: "RestaurantShifts",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantShifts_RestaurantId_ShiftId",
                table: "RestaurantShifts",
                columns: new[] { "RestaurantId", "ShiftId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantShifts_ShiftId",
                table: "RestaurantShifts",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_Guid",
                table: "Shifts",
                column: "Guid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reservations");

            migrationBuilder.DropTable(
                name: "RestaurantShifts");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropTable(
                name: "CombinedTableMembers");

            migrationBuilder.DropTable(
                name: "Shifts");

            migrationBuilder.DropTable(
                name: "CombinedTables");

            migrationBuilder.DropTable(
                name: "FloorplanElements");

            migrationBuilder.DropTable(
                name: "Elements");

            migrationBuilder.DropTable(
                name: "Floorplans");

            migrationBuilder.DropTable(
                name: "Restaurants");
        }
    }
}
