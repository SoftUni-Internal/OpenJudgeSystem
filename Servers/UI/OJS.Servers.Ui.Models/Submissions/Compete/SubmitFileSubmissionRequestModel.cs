﻿namespace OJS.Servers.Ui.Models.Submissions.Compete;

using System;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using OJS.Common.Extensions;
using OJS.Services.Ui.Models.Submissions;
using SoftUni.AutoMapper.Infrastructure.Models;

public class SubmitFileSubmissionRequestModel : IMapExplicitly
{
    public int ProblemId { get; set; }

    public int SubmissionTypeId { get; set; }

    public IFormFile? Content { get; set; } = null;

    public bool Official { get; set; }

    public void RegisterMappings(IProfileExpression configuration)
        => configuration.CreateMap<SubmitFileSubmissionRequestModel, SubmitFileSubmissionServiceModel>()
            .ForMember(
                d => d.Content,
                opt => opt.MapFrom(s =>
                    s.Content == null
                        ? null
                        : s.Content.GetBytes()))
            .ForMember(
                        d => d.FileExtension,
                        opt => opt.MapFrom(s =>
                            s.Content == null
                                ? string.Empty
                                : s.Content.FileName.Split(".", StringSplitOptions.None)[1]));
}