﻿namespace OJS.Services.Administration.Business;

using System.Threading.Tasks;
using OJS.Services.Infrastructure;

public interface ITestsCacheService : IService
{
    Task ClearTestsCacheByProblemId(int problemId);
}