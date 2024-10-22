namespace OJS.Services.Administration.Business.ProblemGroups
{
    using OJS.Data.Models.Problems;
    using OJS.Services.Common.Models;
    using System.Threading.Tasks;
    using OJS.Services.Administration.Models.ProblemGroups;
    using System.Collections.Generic;

    public interface IProblemGroupsBusinessService : IAdministrationOperationService<ProblemGroup, int, ProblemGroupsAdministrationModel>
    {
        Task<ServiceResult> DeleteById(int id);

        Task<ServiceResult> CopyAllToContestBySourceAndDestinationContest(
            int sourceContestId,
            int destinationContestId);

        Task GenerateNewProblem(
            Problem problem,
            ProblemGroup currentNewProblemGroup,
            ICollection<Problem> problemsToAdd);

        Task ReevaluateProblemsAndProblemGroupsOrder(int contestId);

        ICollection<ProblemGroupDropdownModel> GetOrderByContestId(int contestId);
    }
}