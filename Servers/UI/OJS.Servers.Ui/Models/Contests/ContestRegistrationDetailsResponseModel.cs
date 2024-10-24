namespace OJS.Servers.Ui.Models.Contests;

using OJS.Services.Infrastructure.Models.Mapping;
using OJS.Services.Ui.Models.Contests;
using System;

public class ContestRegistrationDetailsResponseModel : IMapFrom<ContestRegistrationDetailsServiceModel>
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool RequirePassword { get; set; }

    public bool ShouldConfirmParticipation { get; set; }

    public bool IsRegisteredSuccessfully { get; set; }

    public TimeSpan? Duration { get; set; }

    public int NumberOfProblems { get; set; }

    public int? ParticipantId { get; set; }

    public bool IsOnlineExam { get; set; }

    public int? CategoryId { get; set; }
}