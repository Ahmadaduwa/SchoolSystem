using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTeacher2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Profiles_AspNetUsers_UserId",
                table: "Profiles");

            migrationBuilder.DropForeignKey(
                name: "FK_Profiles_Teachers_TeacherId",
                table: "Profiles");

            migrationBuilder.DropForeignKey(
                name: "FK_Teachers_AspNetUsers_UserId",
                table: "Teachers");

            migrationBuilder.DropIndex(
                name: "IX_Profiles_TeacherId",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "TeacherId",
                table: "Profiles");

            migrationBuilder.AddForeignKey(
                name: "FK_Profiles_AspNetUsers_UserId",
                table: "Profiles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Teachers_AspNetUsers_UserId",
                table: "Teachers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Profiles_AspNetUsers_UserId",
                table: "Profiles");

            migrationBuilder.DropForeignKey(
                name: "FK_Teachers_AspNetUsers_UserId",
                table: "Teachers");

            migrationBuilder.AddColumn<int>(
                name: "TeacherId",
                table: "Profiles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_TeacherId",
                table: "Profiles",
                column: "TeacherId",
                unique: true,
                filter: "[TeacherId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Profiles_AspNetUsers_UserId",
                table: "Profiles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Profiles_Teachers_TeacherId",
                table: "Profiles",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "TeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_Teachers_AspNetUsers_UserId",
                table: "Teachers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
