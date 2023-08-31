﻿namespace OJS.Services.Ui.Models.Submissions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoMapper;
    using OJS.Data.Models.Submissions;
    using OJS.Services.Ui.Models.Users;
    using SoftUni.AutoMapper.Infrastructure.Models;

    public class SubmissionDetailsServiceModel : IMapExplicitly
    {
        public int Id { get; set; }

        public ProblemServiceModel Problem { get; set; } = null!;

        public int Points { get; set; }

        public string? Content { get; set; }

        public IEnumerable<TestRunDetailsServiceModel> TestRuns { get; set; } =
            Enumerable.Empty<TestRunDetailsServiceModel>();

        public UserProfileServiceModel User { get; set; } = null!;

        public double MaxUsedTime { get; set; }

        public double MaxUsedMemory { get; set; }

        public SubmissionTypeForSubmissionDetailsServiceModel SubmissionType { get; set; } = null!;

        public bool IsOfficial { get; set; }

        public bool IsCompiledSuccessfully { get; set; }

        public string CompilerComment { get; set; } = null!;

        public DateTime CreatedOn { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public byte[]? ByteContent { get; set; }

        public string? FileExtension { get; set; }

        public DateTime? StartedExecutionOn { get; set; }

        public string? ProcessingComment { get; set; }

        public int TotalTests { get; set; }

        public void RegisterMappings(IProfileExpression configuration)
            => configuration.CreateMap<Submission, SubmissionDetailsServiceModel>()
                .ForMember(s => s.User, opt => opt.MapFrom(s => s.Participant!.User))
                .ForMember(d => d.MaxUsedMemory, opt => opt.MapFrom(source =>
                    source.TestRuns.Any()
                        ? source.TestRuns.Max(tr => tr.MemoryUsed)
                        : 0.0))
                .ForMember(d => d.MaxUsedTime, opt => opt.MapFrom(source =>
                    source.TestRuns.Any()
                        ? source.TestRuns.Max(tr => tr.TimeUsed)
                        : 0.0))
                .ForMember(d => d.Content, opt => opt.MapFrom(s =>
                    s.IsBinaryFile
                        ? null
                        : s.ContentAsString))
                .ForMember(d => d.IsOfficial, opt => opt.MapFrom(s =>
                    s.Participant!.IsOfficial))
                .ForMember(d => d.ByteContent, opt => opt.MapFrom(s =>
                    s.Content))
                .ForMember(d => d.TotalTests, opt => opt.MapFrom(s =>
                    s.Problem != null
                        ? s.Problem.Tests.Count
                        : 0));
    }
}