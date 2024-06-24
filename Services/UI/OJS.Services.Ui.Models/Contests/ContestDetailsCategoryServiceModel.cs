namespace OJS.Services.Ui.Models.Contests;

using OJS.Data.Models.Contests;
using OJS.Services.Infrastructure.Models.Mapping;

public class ContestDetailsCategoryServiceModel : IMapFrom<ContestCategory>
{
    public bool IsVisible { get; set; }

    public bool IsDeleted { get; set; }
}
