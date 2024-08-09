namespace OJS.Services.Common.Models.Users;

using AutoMapper;
using OJS.Common.Extensions;
using OJS.Services.Infrastructure.Models.Mapping;
using System.Security.Claims;

public class UserInfoModel : IMapExplicitly
{
    public string Id { get; set; } = null!;

    public string? Username { get; set; }

    public bool IsAuthenticated => this.Id != null!;

    public bool IsAdmin { get; set; }

    public bool IsLecturer { get; set; }

    public bool IsDeveloper { get; set; }

    public bool IsAdminOrLecturer => this.IsAdmin || this.IsLecturer;

    public void RegisterMappings(IProfileExpression configuration)
        => configuration
            .CreateMap<ClaimsPrincipal, UserInfoModel>()
            .ForMember(
                m => m.Id,
                opt => opt.MapFrom(src => src.GetId()))
            .ForMember(
                m => m.Username,
                opt => opt.MapFrom(src => src.GetUsername()))
            .ForMember(
                m => m.IsAdmin,
                opt => opt.MapFrom(src => src.IsAdmin()))
            .ForMember(
                m => m.IsDeveloper,
                opt => opt.MapFrom(src => src.IsDeveloper()))
            .ForMember(
                m => m.IsLecturer,
                opt => opt.MapFrom(src => src.IsLecturer()));
}