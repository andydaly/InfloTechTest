using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace UserManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    Action = table.Column<int>(type: "INTEGER", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    PerformedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Details = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Forename = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Surname = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Password = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "DateOfBirth", "Email", "Forename", "IsActive", "Password", "Surname" },
                values: new object[,]
                {
                    { 1L, new DateTime(1980, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "ploew@example.com", "Peter", true, "mypassword1", "Loew" },
                    { 2L, new DateTime(1974, 7, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "bfgates@example.com", "Benjamin Franklin", true, "mypassword1", "Gates" },
                    { 3L, new DateTime(1971, 11, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "ctroy@example.com", "Castor", false, "mypassword1", "Troy" },
                    { 4L, new DateTime(1976, 5, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "mraines@example.com", "Memphis", true, "mypassword1", "Raines" },
                    { 5L, new DateTime(1972, 2, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), "sgodspeed@example.com", "Stanley", true, "mypassword1", "Goodspeed" },
                    { 6L, new DateTime(1960, 9, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "himcdunnough@example.com", "H.I.", true, "mypassword1", "McDunnough" },
                    { 7L, new DateTime(1970, 12, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "cpoe@example.com", "Cameron", false, "mypassword1", "Poe" },
                    { 8L, new DateTime(1968, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "emalus@example.com", "Edward", false, "mypassword1", "Malus" },
                    { 9L, new DateTime(1965, 3, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "dmacready@example.com", "Damon", false, "mypassword1", "Macready" },
                    { 10L, new DateTime(1984, 10, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "jblaze@example.com", "Johnny", true, "mypassword1", "Blaze" },
                    { 11L, new DateTime(1955, 8, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "rfeld@example.com", "Robin", true, "mypassword1", "Feld" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserLogs");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
