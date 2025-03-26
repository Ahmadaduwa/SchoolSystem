using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolSystem.Migrations
{
    /// <inheritdoc />
    public partial class Init3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AcitivityAttendanceSummary_ActivityManagement_AM_id",
                table: "AcitivityAttendanceSummary");

            migrationBuilder.DropForeignKey(
                name: "FK_AcitivityAttendanceSummary_Students_StudentId",
                table: "AcitivityAttendanceSummary");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AcitivityAttendanceSummary",
                table: "AcitivityAttendanceSummary");

            migrationBuilder.RenameTable(
                name: "AcitivityAttendanceSummary",
                newName: "ActivityAttendanceSummary");

            migrationBuilder.RenameIndex(
                name: "IX_AcitivityAttendanceSummary_StudentId",
                table: "ActivityAttendanceSummary",
                newName: "IX_ActivityAttendanceSummary_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_AcitivityAttendanceSummary_AM_id",
                table: "ActivityAttendanceSummary",
                newName: "IX_ActivityAttendanceSummary_AM_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ActivityAttendanceSummary",
                table: "ActivityAttendanceSummary",
                column: "AAM_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ActivityAttendanceSummary_ActivityManagement_AM_id",
                table: "ActivityAttendanceSummary",
                column: "AM_id",
                principalTable: "ActivityManagement",
                principalColumn: "AM_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ActivityAttendanceSummary_Students_StudentId",
                table: "ActivityAttendanceSummary",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActivityAttendanceSummary_ActivityManagement_AM_id",
                table: "ActivityAttendanceSummary");

            migrationBuilder.DropForeignKey(
                name: "FK_ActivityAttendanceSummary_Students_StudentId",
                table: "ActivityAttendanceSummary");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ActivityAttendanceSummary",
                table: "ActivityAttendanceSummary");

            migrationBuilder.RenameTable(
                name: "ActivityAttendanceSummary",
                newName: "AcitivityAttendanceSummary");

            migrationBuilder.RenameIndex(
                name: "IX_ActivityAttendanceSummary_StudentId",
                table: "AcitivityAttendanceSummary",
                newName: "IX_AcitivityAttendanceSummary_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_ActivityAttendanceSummary_AM_id",
                table: "AcitivityAttendanceSummary",
                newName: "IX_AcitivityAttendanceSummary_AM_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AcitivityAttendanceSummary",
                table: "AcitivityAttendanceSummary",
                column: "AAM_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AcitivityAttendanceSummary_ActivityManagement_AM_id",
                table: "AcitivityAttendanceSummary",
                column: "AM_id",
                principalTable: "ActivityManagement",
                principalColumn: "AM_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AcitivityAttendanceSummary_Students_StudentId",
                table: "AcitivityAttendanceSummary",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
