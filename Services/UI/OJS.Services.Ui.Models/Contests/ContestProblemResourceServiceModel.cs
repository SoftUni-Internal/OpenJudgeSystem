namespace OJS.Services.Ui.Models.Contests
{
    using AutoMapper;
    using OJS.Common.Enumerations;
    using OJS.Data.Models.Problems;
    using OJS.Data.Models.Resources;
    using OJS.Services.Infrastructure.Models.Mapping;

    public class ContestProblemResourceServiceModel : IMapExplicitly
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string Link { get; set; } = null!;

        public ProblemResourceType Type { get; set; }

        public void RegisterMappings(IProfileExpression configuration)
            => configuration
                .CreateMap<ProblemResource, ContestProblemResourceServiceModel>()
                .ReverseMap();
    }
}