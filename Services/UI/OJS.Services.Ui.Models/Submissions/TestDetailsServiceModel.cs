namespace OJS.Services.Ui.Models.Submissions;

using OJS.Data.Models.Tests;
using OJS.Services.Infrastructure.Models.Mapping;

public class TestDetailsServiceModel : IMapFrom<Test>
{
    public int Id { get; set; }

    public bool IsTrialTest { get; set; }

    public bool IsOpenTest { get; set; }

    public bool HideInput { get; set; }

    public string InputDataAsString { get; set; } = string.Empty;

    public double OrderBy { get; set; }
}