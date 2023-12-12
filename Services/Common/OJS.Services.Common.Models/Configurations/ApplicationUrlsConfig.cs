namespace OJS.Services.Common.Models.Configurations;

using System.ComponentModel.DataAnnotations;

public class ApplicationUrlsConfig : BaseConfig
{
    public override string SectionName => "ApplicationUrls";

    [Required]
    public string UiUrl { get; set; } = string.Empty;

    [Required]
    public string AdministrationUrl { get; set; } = string.Empty;

    [Required]
    public string SulsPlatformBaseUrl { get; set; } = string.Empty;

    [Required]
    public string SulsPlatformApiKey { get; set; } = string.Empty;

    [Required]
    public string FrontEndUrl { get; set; } = string.Empty;
}