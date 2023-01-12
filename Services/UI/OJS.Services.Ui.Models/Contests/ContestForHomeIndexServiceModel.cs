namespace OJS.Services.Ui.Models.Contests
{
    using System;
    using AutoMapper;
    using OJS.Data.Models.Contests;
    using SoftUni.AutoMapper.Infrastructure.Models;

    public class ContestForHomeIndexServiceModel : IMapExplicitly
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public DateTime? PracticeStartTime { get; set; }

        public DateTime? PracticeEndTime { get; set; }

        public bool CanBeCompeted { get; set; }

        public bool CanBePracticed { get; set; }

        public bool HasContestPassword { get; set; }

        public bool HasPracticePassword { get; set; }

        public string Category { get; set; } = string.Empty;

        public void RegisterMappings(IProfileExpression configuration)
            => configuration.CreateMap<Contest, ContestForHomeIndexServiceModel>()
                .ForMember(
                    dest => dest.Category,
                    opt => opt.MapFrom(src => src.Category!.Name));
    }
}