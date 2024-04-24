namespace OJS.Services.Administration.Models.Settings;

using OJS.Common.Enumerations;
using OJS.Data.Models;
using SoftUni.AutoMapper.Infrastructure.Models;

public class SettingInListModel : IMapFrom<Setting>
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Value { get; set; }

    public SettingType Type { get; set; }
}