namespace OJS.Services.Administration.Models.UsersMentors;

using System;
using OJS.Data.Models.Mentor;
using OJS.Services.Infrastructure.Models.Mapping;

public class UserMentorInListModel : IMapFrom<UserMentor>
{
    public string Id { get; set; } = default!;

    public string UserUserName { get; set; } = default!;

    public DateTimeOffset QuotaResetTime { get; set; }

    public int RequestsMade { get; set; }

    public int? QuotaLimit { get; set; }

    public int TotalRequestsMade { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }
}