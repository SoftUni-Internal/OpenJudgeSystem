﻿using AutoMapper;
using OJS.Workers.Common;
using OJS.Workers.Common.Models;
using OJS.Workers.ExecutionStrategies.Models;
using SoftUni.AutoMapper.Infrastructure.Extensions;

namespace OJS.Services.Common.Models.Submissions.ExecutionContext.Mapping;

public class SubmissionInputValueResolver : IValueResolver<SubmissionServiceModel, IOjsSubmission, object>
{
    public object Resolve(
        SubmissionServiceModel source,
        IOjsSubmission destination,
        object destMember,
        ResolutionContext context)
    {
        switch (source.ExecutionType)
        {
            case ExecutionType.SimpleExecution:
                return source.SimpleExecutionDetails!.Map<SimpleInputModel>();
            case ExecutionType.TestsExecution:
                return source.TestsExecutionDetails!.Map<TestsInputModel>();
            default:
                return default!;
        }
    }
}
