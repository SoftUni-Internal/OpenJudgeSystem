﻿namespace OJS.Services.Common.Models.Mappings;

using AutoMapper;
using OJS.Common.Constants;
using OJS.Services.Common.Models.Submissions.ExecutionDetails;
using OJS.Workers.Checkers;
using OJS.Workers.ExecutionStrategies.Models;
using System.Collections.Generic;

public class CheckerTypeNameValueResolver : IValueResolver<TestsExecutionDetailsServiceModel, TestsInputModel, string>
{
    private readonly IDictionary<string, string> nameToValueMap = new Dictionary<string, string>
    {
        { ServiceConstants.CheckerTypes.Trim, CheckerConstants.TypeNames.Trim },
        { ServiceConstants.CheckerTypes.TrimEnd, CheckerConstants.TypeNames.TrimEnd },
        { ServiceConstants.CheckerTypes.Precision, CheckerConstants.TypeNames.Precision },
        { ServiceConstants.CheckerTypes.CaseInsensitive, CheckerConstants.TypeNames.CaseInsensitive },
        { ServiceConstants.CheckerTypes.Sort, CheckerConstants.TypeNames.Sort },
        { ServiceConstants.CheckerTypes.ExactMatch, CheckerConstants.TypeNames.ExactMatch },
        { ServiceConstants.CheckerTypes.CSharpCode, CheckerConstants.TypeNames.CSharpCoreCode },
        { ServiceConstants.CheckerTypes.CSharpCoreCode, CheckerConstants.TypeNames.CSharpCode },
    };

    private readonly string defaultValue = string.Empty;

    public string Resolve(
        TestsExecutionDetailsServiceModel source,
        TestsInputModel destination,
        string destMember,
        ResolutionContext context) => !string.IsNullOrEmpty(source.CheckerType) && this.nameToValueMap.ContainsKey(source.CheckerType)
        ? this.nameToValueMap[source.CheckerType]
        : this.defaultValue;
}