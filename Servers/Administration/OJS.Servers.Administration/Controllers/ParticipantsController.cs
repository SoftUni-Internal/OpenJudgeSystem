﻿namespace OJS.Servers.Administration.Controllers;

using Microsoft.AspNetCore.Mvc;
using OJS.Data.Models.Participants;
using OJS.Servers.Administration.Attributes;
using OJS.Services.Administration.Business.Contests.Permissions;
using OJS.Services.Administration.Business.Participants;
using OJS.Services.Administration.Business.Participants.GridData;
using OJS.Services.Administration.Business.Participants.Validators;
using OJS.Services.Administration.Models.Contests.Participants;
using OJS.Services.Administration.Models.Participants;
using OJS.Services.Common.Models.Pagination;
using System.Threading.Tasks;

public class ParticipantsController : BaseAdminApiController<Participant, int, ParticipantInListViewModel, ParticipantAdministrationModel>
{
    private readonly IParticipantsGridDataService participantsGridDataService;

    public ParticipantsController(
        IParticipantsGridDataService participantsGridDataService,
        IParticipantsBusinessService participantsBusinessService,
        ParticipantAdministrationModelValidator validator)
        : base(
            participantsGridDataService,
            participantsBusinessService,
            validator)
        => this.participantsGridDataService = participantsGridDataService;

    [HttpGet("{contestId:int}")]
    [ProtectedEntityAction("contestId", typeof(ContestIdPermissionsService))]
    public async Task<IActionResult> GetByContestId([FromQuery] PaginationRequestModel model, [FromRoute] int contestId)
    {
        if (contestId < 1)
        {
            return this.BadRequest("Invalid contest id.");
        }

        return this.Ok(
            await this.participantsGridDataService
                .GetAll<ParticipantInListViewModel>(model, participant => participant.ContestId == contestId));
    }
}