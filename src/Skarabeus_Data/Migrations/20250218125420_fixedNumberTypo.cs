using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Skarabeus_Data.Migrations
{
    /// <inheritdoc />
    public partial class fixedNumberTypo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PhoneNummberOfMother",
                table: "Persons",
                newName: "PhoneNumberOfMother");

            migrationBuilder.RenameColumn(
                name: "PhoneNummber",
                table: "Persons",
                newName: "PhoneNumberOfFather");

            migrationBuilder.RenameColumn(
                name: "PhoneNUmmberOfFather",
                table: "Persons",
                newName: "PhoneNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PhoneNumberOfMother",
                table: "Persons",
                newName: "PhoneNummberOfMother");

            migrationBuilder.RenameColumn(
                name: "PhoneNumberOfFather",
                table: "Persons",
                newName: "PhoneNummber");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "Persons",
                newName: "PhoneNUmmberOfFather");
        }
    }
}
