namespace OJS.Servers.Ui.Controllers;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OJS.Servers.Infrastructure.Controllers;
using OJS.Servers.Infrastructure.Extensions;
using OJS.Services.Mentor.Business;
using OJS.Services.Mentor.Models;
using static OJS.Common.GlobalConstants.Roles;
using static Microsoft.AspNetCore.Http.StatusCodes;

[Authorize]
public class MentorController(IMentorBusinessService mentorBusiness) : BaseApiController
{
    /// <summary>
    /// Starts a new conversation with the mentor.
    /// </summary>
    /// <param name="model">A request model containing the details, necessary for starting a conversation.</param>
    /// <returns>A response model, containing the conversation's data.</returns>
    [ProducesResponseType(typeof(ConversationResponseModel), Status200OK)]
    [HttpPost]
    public async Task<IActionResult> StartConversation(ConversationRequestModel model)
        => await mentorBusiness.StartConversation(model)
            .ToOkResult();

    /// <summary>
    /// Gets the system message that will be sent when a user asks a question for the specific Problem.
    /// </summary>
    /// <param name="model">The request model with problem details.</param>
    /// <returns>A response model containing the message content.</returns>
    [Authorize(Roles = AdministratorOrLecturer)]
    [HttpGet]
    [ProducesResponseType(typeof(ConversationMessageModel), Status200OK)]
    public async Task<IActionResult> GetSystemMessage([FromQuery] ConversationRequestModel model)
        => await mentorBusiness.GetSystemMessage(model)
            .ToOkResult();
}