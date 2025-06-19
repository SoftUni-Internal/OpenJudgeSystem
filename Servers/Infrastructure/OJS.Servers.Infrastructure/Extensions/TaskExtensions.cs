namespace OJS.Servers.Infrastructure.Extensions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OJS.Services.Common.Models;
using System.Threading.Tasks;

public static class TaskExtensions
{
    public static async Task<OkObjectResult> ToOkResult<T>(this Task<T> task)
        => new(await task);

    public static async Task<OkResult> ToOkResult(this Task task)
    {
        await task;
        return new OkResult();
    }

    public static async Task<IActionResult> ToActionResult<T>(this Task<ServiceResult<T>> task, ILogger logger)
    {
        var result = await task;
        return result.ToActionResult(logger);
    }
}