using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTeacher3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teachers_AspNetUsers_UserId",
                table: "Teachers");

            migrationBuilder.DropIndex(
                name: "IX_Teachers_UserId",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Teachers");

            migrationBuilder.AddColumn<int>(
                name: "ProfileId",
                table: "Teachers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TeacherId",
                table: "Profiles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_ProfileId",
                table: "Teachers",
                column: "ProfileId",
                unique: true);

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

            migrationBuilder.AddForeignKey(
                name: "FK_Teachers_Profiles_ProfileId",
                table: "Teachers",
                column: "ProfileId",
                principalTable: "Profiles",
                principalColumn: "ProfileId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Profiles_Teachers_TeacherId",
                table: "Profiles");

            migrationBuilder.DropForeignKey(
                name: "FK_Teachers_Profiles_ProfileId",
                table: "Teachers");

            migrationBuilder.DropIndex(
                name: "IX_Teachers_ProfileId",
                table: "Teachers");

            migrationBuilder.DropIndex(
                name: "IX_Profiles_TeacherId",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "ProfileId",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "TeacherId",
                table: "Profiles");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Teachers",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_UserId",
                table: "Teachers",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Teachers_AspNetUsers_UserId",
                table: "Teachers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
