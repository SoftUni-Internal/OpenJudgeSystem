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
            // Step 1: Rename old table temporarily
            migrationBuilder.Sql("EXEC sp_rename 'ProblemResources', 'ProblemResources_Old'");

            // Step 2: Create new Resource table (same as before)
            migrationBuilder.CreateTable(
                name: "Resources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResourceType = table.Column<string>(type: "nvarchar(21)", maxLength: 21, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    File = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    FileExtension = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    Link = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrderBy = table.Column<double>(type: "float", nullable: false),
                    ContestId = table.Column<int>(type: "int", nullable: true),
                    ProblemId = table.Column<int>(type: "int", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Resources_Contests_ContestId",
                        column: x => x.ContestId,
                        principalTable: "Contests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Resources_Problems_ProblemId",
                        column: x => x.ProblemId,
                        principalTable: "Problems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Resources_ContestId",
                table: "Resources",
                column: "ContestId");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_ProblemId",
                table: "Resources",
                column: "ProblemId");

            // Step 3: Migrate old data into new table
            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT Resources ON;

                INSERT INTO Resources (
                    Id,
                    ResourceType,
                    Name,
                    Type,
                    [File],
                    FileExtension,
                    Link,
                    OrderBy,
                    ContestId,
                    ProblemId,
                    CreatedOn,
                    ModifiedOn,
                    IsDeleted,
                    DeletedOn
                )
                SELECT
                    Id,
                    'ProblemResource',
                    Name,
                    Type,
                    [File],
                    FileExtension,
                    Link,
                    OrderBy,
                    NULL,
                    ProblemId,
                    CreatedOn,
                    ModifiedOn,
                    IsDeleted,
                    DeletedOn
                FROM ProblemResources_Old;

                SET IDENTITY_INSERT Resources OFF;
            ");

            // Step 4: Drop old table
            migrationBuilder.DropTable(name: "ProblemResources_Old");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Resources");

            migrationBuilder.CreateTable(
                name: "ProblemResources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProblemId = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    File = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    FileExtension = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Link = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OrderBy = table.Column<double>(type: "float", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProblemResources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProblemResources_Problems_ProblemId",
                        column: x => x.ProblemId,
                        principalTable: "Problems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProblemResources_ProblemId",
                table: "ProblemResources",
                column: "ProblemId");
        }
    }
}
