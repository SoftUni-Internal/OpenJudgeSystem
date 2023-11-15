namespace OJS.Services.Common.Models.Configurations;

using System.ComponentModel.DataAnnotations;

public class ApplicationConfig : BaseConfig
{
    public override string SectionName => "ApplicationSettings";

    [Required]
    public string LoggerFilesFolderPath { get; set; } = string.Empty;

    [Required]
    public string SharedAuthCookieDomain { get; set; } = string.Empty;
}