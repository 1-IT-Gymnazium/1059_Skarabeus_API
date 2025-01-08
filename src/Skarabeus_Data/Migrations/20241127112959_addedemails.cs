using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Skarabeus_Data.Migrations
{
    /// <inheritdoc />
    public partial class addedemails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ApsNetUser",
                table: "ApsNetUser");

            migrationBuilder.RenameTable(
                name: "ApsNetUser",
                newName: "AspNetUser");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUser",
                table: "AspNetUser",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Emails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    Subject = table.Column<string>(type: "text", nullable: false),
                    Receiver = table.Column<string>(type: "text", nullable: false),
                    Sender = table.Column<string>(type: "text", nullable: false),
                    ScheduledAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    SentAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    ModifiedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: false),
                    DeletedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Emails", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "AspNetUser",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Email", "EmailConfirmed", "FullName", "LockoutEnabled", "LockoutEnd", "ModifiedAt", "ModifiedBy", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { new Guid("ceab6921-dfed-4b4d-b661-dc36b8749067"), 0, "ba46c7df-e2cf-469d-a17d-b653c50a0147", NodaTime.Instant.FromUnixTimeTicks(-3776735808000000000L), "System", null, null, "user@example.com", true, null, true, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), NodaTime.Instant.FromUnixTimeTicks(-3776735808000000000L), "System", "USER@EXAMPLE.COM", "USER@EXAMPLE.COM", "AQAAAAEAACcQAAAAELKQmdGcfZbjxaz1GeqZ62mF7gEO9d49ofpdaQ+Mq0904MEIWvUnaMMfx9gJ27NmdQ==", "123456798", true, "2MLDENGLJTQEITJVCJMIJJQOKXOUNSD6", false, "user@example.com" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Emails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUser",
                table: "AspNetUser");

            migrationBuilder.DeleteData(
                table: "AspNetUser",
                keyColumn: "Id",
                keyValue: new Guid("ceab6921-dfed-4b4d-b661-dc36b8749067"));

            migrationBuilder.RenameTable(
                name: "AspNetUser",
                newName: "ApsNetUser");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApsNetUser",
                table: "ApsNetUser",
                column: "Id");
        }
    }
}
