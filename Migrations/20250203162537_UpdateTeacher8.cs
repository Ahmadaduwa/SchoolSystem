using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTeacher8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Profiles_Teachers_TeacherId",
                table: "Profiles");

            migrationBuilder.DropIndex(
                name: "IX_Profiles_TeacherId",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "TeacherId",
                table: "Profiles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TeacherId",
                table: "Profiles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_TeacherId",
                table: "Profiles",
                column: "TeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_Profiles_Teachers_TeacherId",
                table: "Profiles",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "TeacherId");
        }
    }
}
