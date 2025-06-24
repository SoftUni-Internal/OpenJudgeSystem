using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OJS.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTablePerHierarchyForResources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename the table from ProblemResources to Resources
            migrationBuilder.RenameTable(
                name: "ProblemResources",
                newName: "Resources");

            // Add the discriminator column for TPH (Table Per Hierarchy)
            migrationBuilder.AddColumn<string>(
                name: "ResourceType",
                table: "Resources",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "ProblemResource");

            // Add ContestId column for ContestResource entities (nullable)
            migrationBuilder.AddColumn<int>(
                name: "ContestId",
                table: "Resources",
                type: "int",
                nullable: true);

            // Make ProblemId nullable (if it wasn't already)
            migrationBuilder.AlterColumn<int>(
                name: "ProblemId",
                table: "Resources",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            // Add check constraint to ensure data integrity
            migrationBuilder.AddCheckConstraint(
                name: "CK_Resources_ResourceType_Integrity",
                table: "Resources",
                sql: @"
                    (ResourceType = 'ProblemResource' AND ProblemId IS NOT NULL AND ContestId IS NULL) OR
                    (ResourceType = 'ContestResource' AND ContestId IS NOT NULL AND ProblemId IS NULL)
                ");

            // Add indexes for foreign keys
            migrationBuilder.CreateIndex(
                name: "IX_Resources_ContestId",
                table: "Resources",
                column: "ContestId");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_ProblemId",
                table: "Resources",
                column: "ProblemId");

            // Add foreign key constraints
            migrationBuilder.AddForeignKey(
                name: "FK_Resources_Contests_ContestId",
                table: "Resources",
                column: "ContestId",
                principalTable: "Contests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Resources_Problems_ProblemId",
                table: "Resources",
                column: "ProblemId",
                principalTable: "Problems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop foreign key constraints
            migrationBuilder.DropForeignKey(
                name: "FK_Resources_Contests_ContestId",
                table: "Resources");

            migrationBuilder.DropForeignKey(
                name: "FK_Resources_Problems_ProblemId",
                table: "Resources");

            // Drop indexes
            migrationBuilder.DropIndex(
                name: "IX_Resources_ContestId",
                table: "Resources");

            migrationBuilder.DropIndex(
                name: "IX_Resources_ProblemId",
                table: "Resources");

            // Drop check constraint
            migrationBuilder.DropCheckConstraint(
                name: "CK_Resources_ResourceType_Integrity",
                table: "Resources");

            // Remove added columns
            migrationBuilder.DropColumn(
                name: "ContestId",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "ResourceType",
                table: "Resources");

            // Restore ProblemId as non-nullable (if it was before)
            migrationBuilder.AlterColumn<int>(
                name: "ProblemId",
                table: "Resources",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            // Rename table back
            migrationBuilder.RenameTable(
                name: "Resources",
                newName: "ProblemResources");
        }
    }
}
