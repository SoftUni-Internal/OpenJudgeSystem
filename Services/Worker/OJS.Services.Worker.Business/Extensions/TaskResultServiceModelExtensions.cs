namespace OJS.Services.Worker.Business.Extensions;

using OJS.Services.Common.Models.Submissions;
using System;
using OJS.Workers.Common.Models;
using System.Linq;

public static class TaskResultServiceModelExtensions
{
    public static void CalculatePoints(this TaskResultServiceModel? taskResult, int taskMaxPoints)
    {
        if (taskResult == null)
        {
            return;
        }

        var points = 0;

        var testResults = taskResult
            .TestResults
            .Where(x => !x.IsTrialTest)
            .ToList();

        if (testResults.Count > 0)
        {
            var correctAnswersCount = testResults
                .Count(x =>
                    x.ResultType == (int)TestRunResultType.CorrectAnswer);

            var coefficient = (double)correctAnswersCount / testResults.Count;

            points = (int)(coefficient * taskMaxPoints);
        }

        taskResult.Points = points;
    }
}