namespace OJS.Services.Mentor.Business;

using OJS.Services.Infrastructure;
using OJS.Services.Mentor.Models;

public interface IMentorBusinessService : IService
{
    Task<ConversationResponseModel> StartConversation(ConversationRequestModel model);

    Task<ConversationMessageModel> GetSystemMessage(ConversationRequestModel model);
}