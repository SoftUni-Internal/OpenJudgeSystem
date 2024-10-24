namespace OJS.Servers.Ui.Models.Users;

using OJS.Services.Infrastructure.Models.Mapping;
using OJS.Services.Ui.Models.Users;

public class UserResponseModel : IMapFrom<UserServiceModel>
{
    public string? Username { get; set; }
}