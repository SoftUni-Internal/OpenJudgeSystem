﻿namespace OJS.Services.Administration.Data
{
    using OJS.Data.Models.Tests;
    using OJS.Services.Common.Data;
    using System.Linq;
    using System.Threading.Tasks;

    public interface ITestsDataService : IDataService<Test>
    {
        IQueryable<Test> GetByIdQuery(int id);

        IQueryable<Test> GetAllByProblem(int problemId);

        Task DeleteByProblem(int problemId);
    }
}