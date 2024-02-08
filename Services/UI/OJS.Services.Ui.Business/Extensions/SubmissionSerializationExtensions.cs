namespace OJS.Services.Ui.Business.Extensions;

using FluentExtensions.Extensions;
using OJS.Services.Common.Models.Submissions.ExecutionContext;

public static class SubmissionSerializationExtensions
{
    public static string ToSerializedDetails(this SubmissionServiceModel model)
    {
        var modelCopy = model.DeepCopy();

        // These properties hold possibly big amount of information that we don't want copied in multiple db tables
        modelCopy.FileContent = null;
        modelCopy.Code = null;

        modelCopy.TestsExecutionDetails!.Tests = modelCopy
            .TestsExecutionDetails!
            .Tests
            .Mutate(t =>
            {
                t.Input = null;
                t.Output = null;
            });

        return modelCopy.ToJson();
    }
}