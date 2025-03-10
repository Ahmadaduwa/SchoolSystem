using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolSystem.Migrations
{
    /// <inheritdoc />
    public partial class ActivityAttendance6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teachers_Profiles_ProfileId",
                table: "Teachers");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Teachers_Profiles_ProfileId",
                table: "Teachers",
                column: "ProfileId",
                principalTable: "Profiles",
                principalColumn: "ProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teachers_Profiles_ProfileId",
                table: "Teachers");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Teachers_Profiles_ProfileId",
                table: "Teachers",
                column: "ProfileId",
                principalTable: "Profiles",
                principalColumn: "ProfileId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
