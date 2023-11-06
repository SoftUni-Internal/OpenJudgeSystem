namespace OJS.Services.Business.Contests
{
    using OJS.Data.Models;
    using OJS.Services.Business.Contests.Models;
    using OJS.Services.Common;

    public interface IContestsBusinessService : IService
    {
        bool IsContestIpValidByContestAndIp(int contestId, string ip);

        /// <summary>
        /// Determines if a user can compete in a contest, depending of his role and the contest type
        /// </summary>
        /// <param name="contestId">The id of the contest</param>
        /// <param name="userId">The id of the user</param>
        /// <param name="isAdmin">Is the user administrator in the system</param>
        /// <param name="allowToAdminAlways">If true, and the user is admin he will always be able to compete</param>
        bool CanUserCompeteByContestByUserAndIsAdmin(
            int contestId,
            string userId,
            bool isAdmin,
            bool allowToAdminAlways = false);

        ServiceResult TransferParticipantsToPracticeById(int contestId);

        void DeleteById(int id);

        /// <summary>
        /// Calculates the required workers for a given contest based on different parameters.
        /// </summary>
        /// <param name="model">The model received from the view, that holds all the parameters needed to calculate required workers.</param>
        JudgeLoadResults CalculateLoadForContest(BaseContestBusinessModel model);


        /// <summary>
        /// Calculates the the average runtime in seconds for specific contest
        /// </summary>
        /// <param name="contest">The contest for which the calculation will be done.</param>
        /// <param name="onlyOfficialSubmissions">Determines weather it takes all or only official submissions</param>
        /// /// <param name="maxAlloweSubmissionCompletionTime">If it has value, will take only submission where completion time is lower or equal to this parameter</param>
        int GetContestSubmissionsAverageRunTimeSeconds(
            Contest contest,
            bool onlyOfficialSubmissions,
            int? maxAlloweSubmissionCompletionTime);
    }
}