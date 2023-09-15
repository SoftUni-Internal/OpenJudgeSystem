namespace OJS.Web.Areas.Contests.ViewModels.Results
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using OJS.Data.Models;

    public class ParticipantResultViewModel
    {
        public string ParticipantUsername { get; set; }

        public string ParticipantFirstName { get; set; }

        public string ParticipantLastName { get; set; }

        public IEnumerable<ProblemResultPairViewModel> ProblemResults { get; set; }

        public string ParticipantFullName => $"{this.ParticipantFirstName?.Trim()} {this.ParticipantLastName?.Trim()}";

        public int Total => this.ProblemResults
            .Where(pr => pr.ShowResult)
            .Sum(pr => pr.BestSubmission.Points);

        public int AdminTotal => this.ProblemResults
            .Sum(pr => pr.BestSubmission.Points);

        public int ExportTotal => this.ProblemResults
            .Where(pr => pr.ShowResult && !pr.IsExcludedFromHomework)
            .Sum(pr => pr.BestSubmission.Points);

        public IEnumerable<int> ParticipantProblemIds { get; set; }
    }
}