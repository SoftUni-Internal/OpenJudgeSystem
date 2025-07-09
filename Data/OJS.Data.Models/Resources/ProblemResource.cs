namespace OJS.Data.Models.Resources;

using OJS.Data.Models.Problems;

public class ProblemResource : Resource
{
    public ProblemResource()
        => this.ResourceType = nameof(ProblemResource);

    public int? ProblemId { get; set; }

    public virtual Problem Problem { get; set; } = null!;
}