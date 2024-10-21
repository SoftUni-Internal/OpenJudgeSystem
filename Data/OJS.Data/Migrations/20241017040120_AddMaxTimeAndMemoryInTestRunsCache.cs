using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OJS.Data.Migrations
{
    using System;

    /// <inheritdoc />
    public partial class AddMaxTimeAndMemoryInTestRunsCache : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE s
                SET s.TestRunsCache = s.TestRunsCache 
                    + '|' 
                    + CAST(tr.MaxTimeUsed AS VARCHAR(50)) 
                    + ',' 
                    + CAST(tr.MaxMemoryUsed AS VARCHAR(50))
                FROM Submissions s
                INNER JOIN (
                    SELECT 
                        tr.SubmissionId,
                        MAX(tr.TimeUsed) AS MaxTimeUsed, 
                        MAX(tr.MemoryUsed) AS MaxMemoryUsed
                    FROM TestRuns tr
                    GROUP BY tr.SubmissionId
                ) tr ON tr.SubmissionId = s.Id
                WHERE s.TestRunsCache IS NOT NULL
                  AND tr.MaxTimeUsed IS NOT NULL
                  AND tr.MaxMemoryUsed IS NOT NULL;
                ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            /*
             * No implementation is provided for the 'Down' method because the 'Up' migration
             * performs an irreversible update. Specifically, it add the max time and max
             * memory used for a given submission to the 'TestRunsCache', which cannot be
             * undone without losing data integrity. This is a deliberate choice to ensure
             * that the database does not contain any unnecessary or misleading data, such
             * as empty file records.
             */

            throw new NotSupportedException("This migration cannot be rolled back.");
        }
    }
}
