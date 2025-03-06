namespace OJS.PubSub.Worker.Models.Submissions;

using AutoMapper;
using OJS.Services.Common.Models.Submissions;
using OJS.Workers.Common.Models;
using OJS.Services.Infrastructure.Models.Mapping;

public class ProcessedSubmissionPubSubModel : IMapExplicitly
{
    // Used for AutoMapper
    public ProcessedSubmissionPubSubModel()
    {
    }

    public ProcessedSubmissionPubSubModel(int submissionId) => this.Id = submissionId;

    public int Id { get; set; }

    public ExceptionModel? Exception { get; set; }

    public ExecutionResultServiceModel? ExecutionResult { get; set; }

    public DateTime? StartedExecutionOn { get; set; }

    public DateTime? CompletedExecutionOn { get; set; }

    public string? WorkerName { get; set; }

    public void SetExecutionResult(ExecutionResultServiceModel executionResultModel)
    {
        this.ExecutionResult = executionResultModel;
        this.Exception = null;
    }

    public void SetException(Exception exception, bool includeStackTrace, ExceptionType? exceptionType = null)
    {
        this.Exception = new ExceptionModel(exception, includeStackTrace, exceptionType);
        this.ExecutionResult = null;
    }

    public void SetStartedAndCompletedExecutionOn(DateTime startedExecutionOn, DateTime completedExecutionOn)
    {
        this.StartedExecutionOn = startedExecutionOn;
        this.CompletedExecutionOn = completedExecutionOn;
    }

    public void RegisterMappings(IProfileExpression configuration)
        => configuration.CreateMap<ProcessedSubmissionPubSubModel, SubmissionExecutionResult>()
            .ForMember(
                d => d.ExecutionResult,
                opt => opt.MapFrom(s => s.ExecutionResult))
            .ForMember(
                d => d.SubmissionId,
                opt => opt.MapFrom(s => s.Id))
            .ReverseMap();
}