namespace OJS.Services.Administration.Models.SubmissionTypes;

using OJS.Data.Models.Submissions;
using OJS.Workers.Common.Models;
using SoftUni.AutoMapper.Infrastructure.Models;

public class SubmissionTypeInListModel : IMapFrom<SubmissionType>
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public ExecutionStrategyType ExecutionStrategyType { get; set; }

    public CompilerType CompilerType { get; set; }

    public bool AllowBinaryFilesUpload { get; set; }

    public string? AllowedFileExtensions { get; set; }
}