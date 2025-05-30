using Microsoft.EntityFrameworkCore.Migrations;

namespace OJS.Data.Migrations;

public partial class AddNullableResourceForeignKeys : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Add new nullable columns
        migrationBuilder.AddColumn<int>(
            name: "ContestId",
            table: "Resources",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "ProblemId",
            table: "Resources",
            type: "int",
            nullable: true);

        // Migrate existing ProblemId data
        migrationBuilder.Sql(@"
            UPDATE Resources 
            SET ProblemId = ParentId 
            WHERE ParentId IS NOT NULL");

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

        // Add check constraint to ensure exactly one foreign key is set
        migrationBuilder.Sql(@"
            ALTER TABLE Resources
            ADD CONSTRAINT CK_Resources_ExactlyOneParent
            CHECK (
                (ContestId IS NOT NULL AND ProblemId IS NULL) OR
                (ContestId IS NULL AND ProblemId IS NOT NULL)
            )");

        // Drop the old ParentId column
        migrationBuilder.DropColumn(
            name: "ParentId",
            table: "Resources");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Add back the ParentId column
        migrationBuilder.AddColumn<int>(
            name: "ParentId",
            table: "Resources",
            type: "int",
            nullable: true);

        // Migrate data back
        migrationBuilder.Sql(@"
            UPDATE Resources 
            SET ParentId = ProblemId 
            WHERE ProblemId IS NOT NULL");

        // Drop the check constraint
        migrationBuilder.Sql("ALTER TABLE Resources DROP CONSTRAINT CK_Resources_ExactlyOneParent");

        // Drop the foreign key constraints
        migrationBuilder.DropForeignKey(
            name: "FK_Resources_Contests_ContestId",
            table: "Resources");

        migrationBuilder.DropForeignKey(
            name: "FK_Resources_Problems_ProblemId",
            table: "Resources");

        // Drop the new columns
        migrationBuilder.DropColumn(
            name: "ContestId",
            table: "Resources");

        migrationBuilder.DropColumn(
            name: "ProblemId",
            table: "Resources");
    }
} 