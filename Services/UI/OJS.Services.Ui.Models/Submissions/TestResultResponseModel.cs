namespace OJS.Services.Ui.Models.Submissions;

using OJS.Services.Infrastructure.Models.Mapping;
using OJS.Workers.Common.Models;

public class TestResultResponseModel : IMapFrom<TestRunServiceModel>
{
    public int Id { get; set; }

    public TestRunResultType ResultType { get; set; }

    public int TimeUsed { get; set; }

    public int MemoryUsed { get; set; }
}