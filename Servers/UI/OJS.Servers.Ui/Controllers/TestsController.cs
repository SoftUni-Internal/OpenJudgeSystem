namespace OJS.Servers.Ui.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OJS.Servers.Infrastructure.Controllers;
using OJS.Servers.Infrastructure.Extensions;
using OJS.Servers.Ui.Models.Submissions.Details;
using OJS.Services.Infrastructure.Extensions;
using OJS.Services.Ui.Business;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Http.StatusCodes;

[Authorize]
public class TestsController(ITestsBusinessService testsBusiness) : BaseApiController
{
    /// <summary>
    /// Gets the test details for a submission's test run for the provided test id.
    /// </summary>
    /// <param name="id">The ID of the test.</param>
    /// <param name="submissionId">The ID of the submission.</param>
    /// <returns>Details model.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TestDetailsResponseModel), Status200OK)]
    public async Task<IActionResult> GetDetailsForSubmission(int id, int submissionId)
        => await testsBusiness
            .GetTestDetails(id, submissionId)
            .Map<TestDetailsResponseModel>()
            .ToOkResult();
}