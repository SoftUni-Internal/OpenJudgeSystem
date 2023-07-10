﻿namespace OJS.Services.Ui.Models.Submissions
{
    using AutoMapper;
    using OJS.Data.Models.Tests;
    using SoftUni.AutoMapper.Infrastructure.Models;

    public class TestRunDetailsServiceModel : IMapExplicitly
    {
        public int Id { get; set; }

        public int TimeUsed { get; set; }

        public long MemoryUsed { get; set; }

        public int SubmissionId { get; set; }

        public string? ExecutionComment { get; set; }

        public string? CheckerComment { get; set; }

        public string ResultType { get; set; } = null!;

        public string? ExpectedOutputFragment { get; set; }

        public string? UserOutputFragment { get; set; }

        public bool? IsTrialTest { get; set; }

        public string? Input { get; set; }

        public double OrderBy { get; set; }

        public bool ShowInput { get; set; } = true;

        public void RegisterMappings(IProfileExpression configuration)
            => configuration.CreateMap<TestRun, TestRunDetailsServiceModel>()
                .ForMember(d => d.ResultType, opt => opt.MapFrom(s => s.ResultType.ToString()))
                .ForMember(d => d.IsTrialTest, opt => opt.MapFrom(s => s.Test.IsTrialTest))
                .ForMember(d => d.Input, opt => opt.MapFrom(s => s.Test.InputDataAsString))
                .ForMember(d => d.OrderBy, opt => opt.MapFrom(s => s.Test.OrderBy))
                .ForAllOtherMembers(x => x.Ignore());
    }
}