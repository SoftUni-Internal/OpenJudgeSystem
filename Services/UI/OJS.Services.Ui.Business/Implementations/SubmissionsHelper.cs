namespace OJS.Services.Ui.Business.Implementations;

using System.Linq;
using OJS.Services.Ui.Models.Submissions;

public class SubmissionsHelper : ISubmissionsHelper
{
    private readonly ISubmissionsForProcessingBusinessService submissionsForProcessingBusinessService;

    public SubmissionsHelper(ISubmissionsForProcessingBusinessService submissionsForProcessingBusinessService)
        => this.submissionsForProcessingBusinessService = submissionsForProcessingBusinessService;
    public bool IsEligibleForRetest(SubmissionDetailsServiceModel detailsModel)
        => detailsModel.Tests.Any() && !detailsModel.TestRuns.Any() &&
           detailsModel is { IsProcessed: true, IsCompiledSuccessfully: true } &&
           !this.submissionsForProcessingBusinessService.IsSubmissionProcessing(detailsModel.Id);
}