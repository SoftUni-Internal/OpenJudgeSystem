namespace OJS.Services.Administration.Models.SubmissionsForProcessing;

using AutoMapper;
using OJS.Common.Enumerations;
using OJS.Data.Models.Submissions;
using OJS.Services.Common.Models;
using OJS.Services.Infrastructure.Models.Mapping;
using System;

public class SubmissionsForProcessingAdministrationServiceModel : BaseAdministrationModel<int>, IMapExplicitly
{
    public int SubmissionId { get; set; }

    public SubmissionProcessingState State { get; set; } = SubmissionProcessingState.Invalid;

    public DateTimeOffset? EnqueuedAt { get; set; }

    public DateTimeOffset? ProcessingStartedAt { get; set; }

    public DateTimeOffset? ProcessedAt { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public void RegisterMappings(IProfileExpression configuration)
        => configuration.CreateMap<SubmissionForProcessing, SubmissionsForProcessingAdministrationServiceModel>()
            .ForMember(sfpam => sfpam.OperationType, opt
                => opt.Ignore());
}