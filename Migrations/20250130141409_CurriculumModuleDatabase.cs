using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolSystem.Migrations
{
    /// <inheritdoc />
    public partial class CurriculumModuleDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CourseCategorys",
                columns: table => new
                {
                    CourseCategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseCategorys", x => x.CourseCategoryId);
                });

            migrationBuilder.CreateTable(
                name: "GradeLevels",
                columns: table => new
                {
                    GradeLevelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradeLevels", x => x.GradeLevelId);
                });

            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    CourseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Course_Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CourseName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Unit = table.Column<int>(type: "int", nullable: false),
                    CourseCategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.CourseId);
                    table.ForeignKey(
                        name: "FK_Courses_CourseCategorys_CourseCategoryId",
                        column: x => x.CourseCategoryId,
                        principalTable: "CourseCategorys",
                        principalColumn: "CourseCategoryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompulsoryCourses",
                columns: table => new
                {
                    CC_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GradeLevelId = table.Column<int>(type: "int", nullable: false),
                    CurriculumId = table.Column<int>(type: "int", nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompulsoryCourses", x => x.CC_Id);
                    table.ForeignKey(
                        name: "FK_CompulsoryCourses_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompulsoryCourses_Curriculum_CurriculumId",
                        column: x => x.CurriculumId,
                        principalTable: "Curriculum",
                        principalColumn: "CurriculumId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompulsoryCourses_GradeLevels_GradeLevelId",
                        column: x => x.GradeLevelId,
                        principalTable: "GradeLevels",
                        principalColumn: "GradeLevelId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompulsoryElectiveCourses",
                columns: table => new
                {
                    CEC_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GradeLevelId = table.Column<int>(type: "int", nullable: false),
                    CurriculumId = table.Column<int>(type: "int", nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompulsoryElectiveCourses", x => x.CEC_Id);
                    table.ForeignKey(
                        name: "FK_CompulsoryElectiveCourses_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompulsoryElectiveCourses_Curriculum_CurriculumId",
                        column: x => x.CurriculumId,
                        principalTable: "Curriculum",
                        principalColumn: "CurriculumId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompulsoryElectiveCourses_GradeLevels_GradeLevelId",
                        column: x => x.GradeLevelId,
                        principalTable: "GradeLevels",
                        principalColumn: "GradeLevelId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ElectiveCourses",
                columns: table => new
                {
                    ES_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GradeLevelId = table.Column<int>(type: "int", nullable: false),
                    CurriculumId = table.Column<int>(type: "int", nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElectiveCourses", x => x.ES_Id);
                    table.ForeignKey(
                        name: "FK_ElectiveCourses_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ElectiveCourses_Curriculum_CurriculumId",
                        column: x => x.CurriculumId,
                        principalTable: "Curriculum",
                        principalColumn: "CurriculumId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ElectiveCourses_GradeLevels_GradeLevelId",
                        column: x => x.GradeLevelId,
                        principalTable: "GradeLevels",
                        principalColumn: "GradeLevelId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompulsoryCourses_CourseId",
                table: "CompulsoryCourses",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CompulsoryCourses_CurriculumId",
                table: "CompulsoryCourses",
                column: "CurriculumId");

            migrationBuilder.CreateIndex(
                name: "IX_CompulsoryCourses_GradeLevelId",
                table: "CompulsoryCourses",
                column: "GradeLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_CompulsoryElectiveCourses_CourseId",
                table: "CompulsoryElectiveCourses",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CompulsoryElectiveCourses_CurriculumId",
                table: "CompulsoryElectiveCourses",
                column: "CurriculumId");

            migrationBuilder.CreateIndex(
                name: "IX_CompulsoryElectiveCourses_GradeLevelId",
                table: "CompulsoryElectiveCourses",
                column: "GradeLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_CourseCategoryId",
                table: "Courses",
                column: "CourseCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ElectiveCourses_CourseId",
                table: "ElectiveCourses",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_ElectiveCourses_CurriculumId",
                table: "ElectiveCourses",
                column: "CurriculumId");

            migrationBuilder.CreateIndex(
                name: "IX_ElectiveCourses_GradeLevelId",
                table: "ElectiveCourses",
                column: "GradeLevelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompulsoryCourses");

            migrationBuilder.DropTable(
                name: "CompulsoryElectiveCourses");

            migrationBuilder.DropTable(
                name: "ElectiveCourses");

            migrationBuilder.DropTable(
                name: "Courses");

            migrationBuilder.DropTable(
                name: "GradeLevels");

            migrationBuilder.DropTable(
                name: "CourseCategorys");
        }
    }
}
