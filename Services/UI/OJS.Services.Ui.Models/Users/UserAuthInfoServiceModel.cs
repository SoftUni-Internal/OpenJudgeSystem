namespace OJS.Services.Ui.Models.Users;

using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using OJS.Data.Models.Users;
using OJS.Services.Infrastructure.Models.Mapping;
using OJS.Common;

public class UserAuthInfoServiceModel : IMapExplicitly
{
    public string Id { get; set; } = default!;

    public string UserName { get; set; } = default!;

    public string Email { get; set; } = default!;

    public bool IsAdmin { get; set; }

    public bool IsLecturer { get; set; }

    public bool IsDeveloper { get; set; }

    public bool CanAccessAdministration { get; set; }
    public IEnumerable<RoleServiceModel> Roles { get; set; } = Enumerable.Empty<RoleServiceModel>();

    public void RegisterMappings(IProfileExpression configuration) => configuration
        .CreateMap<UserProfile, UserAuthInfoServiceModel>()
        .ForMember(
            d => d.Roles,
            opts => opts
                .MapFrom(s =>
                    s
                    .UsersInRoles
                    .Select(uir => uir.Role)))
        .ForMember(d => d.CanAccessAdministration, opts
            => opts.MapFrom(s
                => s.UsersInRoles.Any(r => r.Role.Name == GlobalConstants.Roles.Administrator || r.Role.Name == GlobalConstants.Roles.Lecturer)))
        .ForMember(d => d.IsAdmin, opts
            => opts.MapFrom(s
                => s.UsersInRoles.Any(r => r.Role.Name == GlobalConstants.Roles.Administrator)))
        .ForMember(d => d.IsLecturer, opts
            => opts.MapFrom(s
                => s.UsersInRoles.Any(r => r.Role.Name == GlobalConstants.Roles.Lecturer)))
        .ForMember(d => d.IsDeveloper, opts
            => opts.MapFrom(s
                => s.UsersInRoles.Any(r => r.Role.Name == GlobalConstants.Roles.Developer)));
}