namespace OJS.Services.Administration.Business.ProblemGroups
{
    using OJS.Data.Models.Problems;
    using System.Threading.Tasks;
    using OJS.Services.Administration.Models.ProblemGroups;
    using OJS.Services.Infrastructure.Models;
    using System.Collections.Generic;

    public interface IProblemGroupsBusinessService : IAdministrationOperationService<ProblemGroup, int, ProblemGroupsAdministrationModel>
    {
        Task<ServiceResult<VoidResult>> DeleteById(int id);

        Task<ServiceResult<VoidResult>> CopyAllToContestBySourceAndDestinationContest(
            int sourceContestId,
            int destinationContestId);

        Task GenerateNewProblem(
            Problem problem,
            ProblemGroup currentNewProblemGroup,
            ICollection<Problem> problemsToAdd);

        Task ReevaluateProblemsAndProblemGroupsOrder(int contestId);

        ICollection<ProblemGroupDropdownModel> GetOrderByContestId(int contestId);

        Task<double> GetNewLatestOrderByContest(int contestId);
    }
}