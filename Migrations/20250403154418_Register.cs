using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolSystem.Migrations
{
    /// <inheritdoc />
    public partial class Register : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RegisteredCourse_Course_CourseId",
                table: "RegisteredCourse");

            migrationBuilder.DropForeignKey(
                name: "FK_RegisteredCourse_Semesters_SemesterId",
                table: "RegisteredCourse");

            migrationBuilder.DropForeignKey(
                name: "FK_RegisteredCourse_Students_StudentId",
                table: "RegisteredCourse");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RegisteredCourse",
                table: "RegisteredCourse");

            migrationBuilder.RenameTable(
                name: "RegisteredCourse",
                newName: "RegisteredCourses");

            migrationBuilder.RenameIndex(
                name: "IX_RegisteredCourse_StudentId",
                table: "RegisteredCourses",
                newName: "IX_RegisteredCourses_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_RegisteredCourse_SemesterId",
                table: "RegisteredCourses",
                newName: "IX_RegisteredCourses_SemesterId");

            migrationBuilder.RenameIndex(
                name: "IX_RegisteredCourse_CourseId",
                table: "RegisteredCourses",
                newName: "IX_RegisteredCourses_CourseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RegisteredCourses",
                table: "RegisteredCourses",
                column: "RC_id");

            migrationBuilder.AddForeignKey(
                name: "FK_RegisteredCourses_Course_CourseId",
                table: "RegisteredCourses",
                column: "CourseId",
                principalTable: "Course",
                principalColumn: "CourseId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RegisteredCourses_Semesters_SemesterId",
                table: "RegisteredCourses",
                column: "SemesterId",
                principalTable: "Semesters",
                principalColumn: "SemesterID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RegisteredCourses_Students_StudentId",
                table: "RegisteredCourses",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RegisteredCourses_Course_CourseId",
                table: "RegisteredCourses");

            migrationBuilder.DropForeignKey(
                name: "FK_RegisteredCourses_Semesters_SemesterId",
                table: "RegisteredCourses");

            migrationBuilder.DropForeignKey(
                name: "FK_RegisteredCourses_Students_StudentId",
                table: "RegisteredCourses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RegisteredCourses",
                table: "RegisteredCourses");

            migrationBuilder.RenameTable(
                name: "RegisteredCourses",
                newName: "RegisteredCourse");

            migrationBuilder.RenameIndex(
                name: "IX_RegisteredCourses_StudentId",
                table: "RegisteredCourse",
                newName: "IX_RegisteredCourse_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_RegisteredCourses_SemesterId",
                table: "RegisteredCourse",
                newName: "IX_RegisteredCourse_SemesterId");

            migrationBuilder.RenameIndex(
                name: "IX_RegisteredCourses_CourseId",
                table: "RegisteredCourse",
                newName: "IX_RegisteredCourse_CourseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RegisteredCourse",
                table: "RegisteredCourse",
                column: "RC_id");

            migrationBuilder.AddForeignKey(
                name: "FK_RegisteredCourse_Course_CourseId",
                table: "RegisteredCourse",
                column: "CourseId",
                principalTable: "Course",
                principalColumn: "CourseId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RegisteredCourse_Semesters_SemesterId",
                table: "RegisteredCourse",
                column: "SemesterId",
                principalTable: "Semesters",
                principalColumn: "SemesterID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RegisteredCourse_Students_StudentId",
                table: "RegisteredCourse",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
