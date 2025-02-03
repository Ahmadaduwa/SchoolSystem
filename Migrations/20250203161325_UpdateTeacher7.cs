using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTeacher7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Teachers_ProfileId",
                table: "Teachers");

            migrationBuilder.AlterColumn<int>(
                name: "ProfileId",
                table: "Teachers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_ProfileId",
                table: "Teachers",
                column: "ProfileId",
                unique: true,
                filter: "[ProfileId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Teachers_ProfileId",
                table: "Teachers");

            migrationBuilder.AlterColumn<int>(
                name: "ProfileId",
                table: "Teachers",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_ProfileId",
                table: "Teachers",
                column: "ProfileId",
                unique: true);
        }
    }
}
