namespace OJS.Web.Areas.Administration.ViewModels.Contest
{
    using System.Linq;

    public class PreviousContestLoadData
    {
        public PreviousContestLoadData(OJS.Data.Models.Contest contest)
        {
            this.Id = contest.Id;
            this.Name = contest.Name;
        }

        public int Id { get; set; }

        public string Name { get; set; }
    }
}