namespace OJS.Data;

using Microsoft.EntityFrameworkCore;
using OJS.Data.Models.Submissions;

public class ArchivesDbContext(DbContextOptions<ArchivesDbContext> options) : DbContext(options)
{
    public DbSet<ArchivedSubmission> Submissions { get; set; } = null!;
}