﻿namespace OJS.Services.Business.Submissions
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface ISubmissionsBusinessService : IService
    {
        IQueryable<Submission> GetAllForArchiving();

        void RecalculatePointsByProblem(int problemId);

        int HardDeleteAllArchived();

        int HardDeleteArchived(int deleteCountLimit = 0);
    }
}