namespace OJS.Services.Administration.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using OJS.Data.Models.Contests;
    using OJS.Services.Common.Data;

    public interface IContestsDataService : IDataService<Contest>
    {
        Task<Contest?> GetByIdWithProblems(int id);

        Task<Contest?> GetByIdWithProblemsAndParticipants(int id);

        Task<Contest?> GetByIdWithParticipantsScoresAndSubmissions(int id);

        IQueryable<Contest> GetAllVisible();

        IQueryable<Contest> GetAllByLecturer(string? lecturerId);

        Task<int> GetMaxPointsForExportById(int id);

        Task<string?> GetNameById(int id);

        Task<bool> IsActiveById(int id);

        Task<bool> IsWithRandomTasksById(int id);

        Task<bool> IsUserLecturerInContestByContestAndUser(int id, string? userId);

        Task<IEnumerable<string>> GetProblemNamesById(int id);
    }
}