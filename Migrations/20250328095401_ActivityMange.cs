using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolSystem.Migrations
{
    /// <inheritdoc />
    public partial class ActivityMange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdateAt",
                table: "ActivityManagement");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ActivityManagement",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "ActivityManagement",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ActivityManagement",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ActivityManagement");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ActivityManagement");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ActivityManagement");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateAt",
                table: "ActivityManagement",
                type: "datetime2",
                nullable: true);
        }
    }
}
