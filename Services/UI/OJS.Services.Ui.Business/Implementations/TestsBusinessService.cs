namespace OJS.Services.Ui.Business.Implementations;

using Microsoft.EntityFrameworkCore;
using OJS.Services.Common;
using OJS.Services.Infrastructure.Exceptions;
using OJS.Services.Ui.Business.Cache;
using OJS.Services.Ui.Data;
using OJS.Services.Ui.Models.Submissions;
using System.Linq;
using System.Threading.Tasks;

public class TestsBusinessService(
    ILecturersInContestsCacheService lecturersInContestsCache,
    ISubmissionsDataService submissionsData,
    IUserProviderService userProvider)
    : ITestsBusinessService
{
    public async Task<TestSeparateDetailsServiceModel> GetTestDetails(int testId, int submissionId)
    {
        var currentUser = userProvider.GetCurrentUser();
        var submission = await submissionsData.GetByIdQuery(submissionId)
            .Select(s => new
            {
                s.Participant.UserId,
                s.Participant.ContestId,
                s.Participant.Contest.CategoryId,
                Test = s.TestRuns
                    .Select(tr => new TestSeparateDetailsServiceModel
                    {
                        Id = tr.Test.Id,
                        IsTrialTest = tr.Test.IsTrialTest,
                        IsOpenTest = tr.Test.IsOpenTest,
                        HideInput = tr.Test.HideInput,
                        ProblemShowDetailedFeedback = tr.Test.Problem.ShowDetailedFeedback,
                        InputData = tr.Test.InputData,
                    })
                    .FirstOrDefault(t => t.Id == testId),
            })
            .FirstOrDefaultAsync() ?? throw new BusinessServiceException("Submission not found.");

        var test = submission.Test ?? throw new BusinessServiceException("Test not found.");

        var isCurrentUserAdminOrLecturerInContest = await lecturersInContestsCache
            .IsUserAdminOrLecturerInContest(submission.ContestId, submission.CategoryId, currentUser);

        if (!isCurrentUserAdminOrLecturerInContest && submission.UserId != currentUser.Id)
        {
            throw new BusinessServiceException("You are not authorized to view this test.");
        }

        var canViewTest = isCurrentUserAdminOrLecturerInContest ||
            (!test.HideInput &&
                (test.IsTrialTest || test.IsOpenTest || test.ProblemShowDetailedFeedback));

        return canViewTest
            ? test
            : throw new BusinessServiceException("You are not authorized to view this test.");
    }
}