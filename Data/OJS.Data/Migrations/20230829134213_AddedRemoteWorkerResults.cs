﻿#nullable disable

namespace OJS.Data.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class AddedRemoteWorkerResults : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SerializedException",
                table: "SubmissionsForProcessing",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SerializedExecutionDetails",
                table: "SubmissionsForProcessing",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SerializedExecutionResult",
                table: "SubmissionsForProcessing",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SerializedException",
                table: "SubmissionsForProcessing");

            migrationBuilder.DropColumn(
                name: "SerializedExecutionDetails",
                table: "SubmissionsForProcessing");

            migrationBuilder.DropColumn(
                name: "SerializedExecutionResult",
                table: "SubmissionsForProcessing");
        }
    }
}
