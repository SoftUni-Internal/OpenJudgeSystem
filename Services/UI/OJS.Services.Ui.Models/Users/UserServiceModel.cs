namespace OJS.Services.Ui.Models.Users;

using OJS.Services.Common.Models.Users;
using OJS.Services.Infrastructure.Models.Mapping;

public class UserServiceModel : IMapFrom<UserInfoModel>
{
    public string Id { get; set; } = string.Empty;

    public string? Username { get; set; }
}