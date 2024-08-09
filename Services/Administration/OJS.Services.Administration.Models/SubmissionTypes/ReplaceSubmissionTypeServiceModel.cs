namespace OJS.Services.Administration.Models.SubmissionTypes;

public class ReplaceSubmissionTypeServiceModel
{
    public int SubmissionTypeToReplace { get; set; }

    public int? SubmissionTypeToReplaceWith { get; set; }
}