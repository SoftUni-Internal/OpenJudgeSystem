namespace OJS.Services.Infrastructure.Configurations;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class SvnConfig : BaseConfig
{
    public override string SectionName => "Svn";

    [Required]
    public string BaseUrl { get; set; } = default!;

    [Required]
    public string Username { get; set; } = default!;

    [Required]
    public string Password { get; set; } = default!;

    public IEnumerable<string>? AlternativeBaseUrls { get; set; }
}