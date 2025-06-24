namespace OJS.Services.Ui.Models.Problems;

using OJS.Data.Models.Problems;
using OJS.Services.Infrastructure.Models.Mapping;
using System.Collections.Generic;

public class ContestDetailsProblemServiceModel : IMapFrom<Problem>
{
    public string Name { get; set; } = string.Empty;

    public ICollection<ResourceServiceModel> Resources { get; set; } = new HashSet<ResourceServiceModel>();
}