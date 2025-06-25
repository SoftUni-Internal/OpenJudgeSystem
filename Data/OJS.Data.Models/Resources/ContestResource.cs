namespace OJS.Data.Models.Resources;

using OJS.Data.Models.Contests;

public class ContestResource : Resource
{
    public ContestResource()
        => this.ResourceType = nameof(ContestResource);

    public int? ContestId { get; set; }

    public Contest? Contest { get; set; }
}