namespace OJS.Services.Ui.Models.Submissions;

using AutoMapper;
using OJS.Data.Models.Submissions;
using OJS.Services.Infrastructure.Models.Mapping;

public class SubmitSubmissionServiceModel : IMapExplicitly
{
    public int ProblemId { get; set; }

    public int ContestId { get; set; }

    public bool IsOnlineExam { get; set; }

    public int SubmissionTypeId { get; set; }

    public bool Official { get; set; }

    public byte[]? ByteContent { get; set; }

    public string? StringContent { get; set; }

    public string? FileExtension { get; set; }

    public bool Verbosely { get; set; }

    public void RegisterMappings(IProfileExpression configuration)
        => configuration.CreateMap<SubmitSubmissionServiceModel, Submission>()
            .ForMember(
                d => d.Content,
                opt => opt.MapFrom(s =>
                    s.ByteContent == null
                        ? null
                        : s.ByteContent))
            .ForMember(
                d => d.ProblemId,
                opt => opt.MapFrom(s => s.ProblemId))
            .ForMember(
                d => d.SubmissionTypeId,
                opt => opt.MapFrom(s => s.SubmissionTypeId))
            .ForMember(
                d => d.FileExtension,
                opt => opt.MapFrom(s =>
                    s.FileExtension == null
                        ? null
                        : s.FileExtension))
            .ForMember(m => m.ParticipantId, opt => opt.Ignore())
            .ForMember(m => m.Participant, opt => opt.Ignore())
            .ForMember(m => m.Problem, opt => opt.Ignore())
            .ForMember(m => m.SubmissionType, opt => opt.Ignore())
            .ForMember(m => m.SolutionSkeleton, opt => opt.Ignore())
            .ForMember(m => m.StartedExecutionOn, opt => opt.Ignore())
            .ForMember(m => m.CompletedExecutionOn, opt => opt.Ignore())
            .ForMember(m => m.IpAddress, opt => opt.Ignore())
            .ForMember(m => m.WorkerName, opt => opt.Ignore())
            .ForMember(m => m.ContentAsString, opt => opt.Ignore())
            .ForMember(m => m.IsCompiledSuccessfully, opt => opt.Ignore())
            .ForMember(m => m.CompilerComment, opt => opt.Ignore())
            .ForMember(m => m.IsPublic, opt => opt.Ignore())
            .ForMember(m => m.TestRuns, opt => opt.Ignore())
            .ForMember(m => m.TestRunsCache, opt => opt.Ignore())
            .ForMember(m => m.Processed, opt => opt.Ignore())
            .ForMember(m => m.ProcessingComment, opt => opt.Ignore())
            .ForMember(m => m.Points, opt => opt.Ignore())
            .ForMember(m => m.IsDeleted, opt => opt.Ignore())
            .ForMember(m => m.DeletedOn, opt => opt.Ignore())
            .ForMember(m => m.CreatedOn, opt => opt.Ignore())
            .ForMember(m => m.ModifiedOn, opt => opt.Ignore())
            .ForMember(m => m.Id, opt => opt.Ignore());
}