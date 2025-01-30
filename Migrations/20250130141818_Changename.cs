using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolSystem.Migrations
{
    /// <inheritdoc />
    public partial class Changename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_CourseCategorys_CourseCategoryId",
                table: "Courses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CourseCategorys",
                table: "CourseCategorys");

            migrationBuilder.RenameTable(
                name: "CourseCategorys",
                newName: "CourseCategories");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CourseCategories",
                table: "CourseCategories",
                column: "CourseCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_CourseCategories_CourseCategoryId",
                table: "Courses",
                column: "CourseCategoryId",
                principalTable: "CourseCategories",
                principalColumn: "CourseCategoryId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_CourseCategories_CourseCategoryId",
                table: "Courses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CourseCategories",
                table: "CourseCategories");

            migrationBuilder.RenameTable(
                name: "CourseCategories",
                newName: "CourseCategorys");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CourseCategorys",
                table: "CourseCategorys",
                column: "CourseCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_CourseCategorys_CourseCategoryId",
                table: "Courses",
                column: "CourseCategoryId",
                principalTable: "CourseCategorys",
                principalColumn: "CourseCategoryId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
