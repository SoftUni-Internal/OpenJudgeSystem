namespace OJS.Services.Ui.Models.Participations;

using OJS.Data.Models.Participants;
using OJS.Services.Infrastructure.Models.Mapping;

public class ParticipationForProblemMaxScoreServiceModel : IMapFrom<ParticipantScore>
{
    public int ProblemId { get; set; }

    public int? Points { get; set; }
}