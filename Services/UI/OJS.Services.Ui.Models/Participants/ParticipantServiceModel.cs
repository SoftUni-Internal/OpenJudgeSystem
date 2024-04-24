namespace OJS.Services.Ui.Models.Participants;

using System;
using SoftUni.AutoMapper.Infrastructure.Models;
using OJS.Data.Models.Participants;

public class ParticipantServiceModel : IMapFrom<Participant>
{
    public int Id { get; set; }

    public DateTime? ParticipationEndTime { get; set; }
}