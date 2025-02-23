using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Skarabeus_Data.Migrations
{
    /// <inheritdoc />
    public partial class addedPersonStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Persons",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Persons");
        }
    }
}
