namespace OJS.Servers.Administration.Extensions;

using FluentExtensions.Extensions;
using Microsoft.Extensions.DependencyInjection;
using OJS.Common.Enumerations;
using OJS.Data.Models;
using OJS.Data.Models.Mentor;
using OJS.Services.Administration.Models;
using OJS.Services.Common.Data;
using OJS.Services.Common.Models.MentorPromptTemplates;
using OJS.Services.Common.Models.Settings;
using OJS.Services.Infrastructure.Extensions;
using System;
using System.Threading.Tasks;
using static OJS.Common.GlobalConstants.Settings;

public static class ServiceProviderExtensions
{
    public static async Task SeedSettings(this IServiceProvider serviceProvider)
    {
        var contestLimitBetweenSubmissionsAdjustmentSettings = new ContestLimitBetweenSubmissionsAdjustSettings
        {
            MaxLimitBetweenSubmissionsInSeconds = 300,
            RatioMultiplier = 1.5,
            RatioModerateThreshold = 0.3,
            RatioCriticalThreshold = 0.7,
            QueueMaxFactor = 4,
            QueueCriticalThresholdMultiplier = 3,
        };

        SettingServiceModel[] settings =
        [
            new() { Name = MaxWorkersWorkingTimeInSeconds, Value = "300", Type = SettingType.Numeric },
            new() { Name = MaxSubmissionsCountAllowedForBatchRetest, Value = "100", Type = SettingType.Numeric },
            new() { Name = nameof(MentorMessagesSentCount), Value = "6", Type = SettingType.Numeric },
            new() { Name = nameof(MentorMaxInputTokenCount), Value = "1600", Type = SettingType.Numeric },
            new() { Name = nameof(PercentageOfMentorMaxInputTokenCountUsedByUser), Value = "50", Type = SettingType.Numeric },
            new() { Name = nameof(MentorMaxOutputTokenCount), Value = "800", Type = SettingType.Numeric },
            new() { Name = nameof(MentorQuotaLimit), Value = "10", Type = SettingType.Numeric },
            new() { Name = nameof(MentorQuotaResetTimeInMinutes), Value = "60", Type = SettingType.Numeric },
            new() { Name = MentorModel, Value = "Gpt4oMini", Type = SettingType.ShortString },
            new()
            {
                Name = ContestLimitBetweenSubmissionsAdjustmentSettings,
                Value = contestLimitBetweenSubmissionsAdjustmentSettings.ToJson(),
                Type = SettingType.Json,
            },
        ];

        var dataService = serviceProvider.GetRequiredService<ISeederDataService<Setting>>();

        foreach (var setting in settings)
        {
            await dataService.CreateIfNotExists(
                setting.Map<Setting>(),
                setting,
                name => entity => entity.Name == name,
                entity => entity.Name
            );
        }
    }

    public static async Task SeedMentorPromptTemplates(this IServiceProvider serviceProvider)
    {
        var dataService = serviceProvider.GetRequiredService<ISeederDataService<MentorPromptTemplate>>();
        MentorPromptTemplateServiceModel[] promptTemplates =
        [
            new()
            {
                Title = "Default Template",
                Template = "Act as a teacher and guide the student to the final answer without providing the solution, even if asked. Respond exclusively in grammatically correct Bulgarian, ensuring all communication adheres to standard Bulgarian language rules and conventions. Review shared code, point out mistakes, and give hints or explanations to help the student solve the problem independently. Analyze and validate suggested approaches. The problem details are: Name: \"{0}\", Description: \"{1}\", Contest: \"{2}\", Category: \"{3}\", Programming Language: \"{4}\", Provider: SoftUni. If the problem's name, description or programming language is missing, request them explicitly. Do not discuss unrelated topics, assume missing details, or deviate from these instructions. Keep responses concise and relevant."            }
        ];

        foreach (var promptTemplate in promptTemplates)
        {
            await dataService.CreateIfNotExists(
                promptTemplate.Map<MentorPromptTemplate>(),
                promptTemplate,
                title => entity => entity.Title == title,
                entity => entity.Title);
        }
    }
}