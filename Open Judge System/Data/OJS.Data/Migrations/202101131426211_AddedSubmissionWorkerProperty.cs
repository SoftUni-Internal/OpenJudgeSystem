namespace OJS.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedSubmissionWorkerProperty : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Submissions", "Worker", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Submissions", "Worker");
        }
    }
}
