namespace OJS.Services.Administration.Business.Extensions;

using AutoCrudAdmin.Models;
using AutoCrudAdmin.ViewModels;
using FluentExtensions.Extensions;
using Microsoft.AspNetCore.Http;
using OJS.Common.Enumerations;
using OJS.Services.Administration.Models;
using System;
using System.Collections.Generic;
using System.Linq;

public static class AdminActionContextExtensions
{
    public static string? GetFormValue(this AdminActionContext actionContext, AdditionalFormFields field)
        => actionContext.EntityDict.ContainsKey(field.ToString()) ? actionContext.EntityDict[field.ToString()] : null;

    public static IFormFile? GetFormFile(this AdminActionContext actionContext, AdditionalFormFields field)
        => actionContext.Files.SingleFiles.FirstOrDefault(f => f.Name == field.ToString());

    public static ProblemGroupType? GetProblemGroupType(this AdminActionContext actionContext)
        => actionContext.EntityDict[AdditionalFormFields.ProblemGroupType.ToString()].ToEnum<ProblemGroupType>();

    public static IEnumerable<ExpandableMultiChoiceCheckBoxFormControlViewModel> GetSubmissionTypes(this AdminActionContext actionContext)
        => actionContext.EntityDict[AdditionalFormFields.SubmissionTypes.ToString()]
            .FromJson<IEnumerable<ExpandableMultiChoiceCheckBoxFormControlViewModel>>();

    public static int? GetEntityIdOrDefault<TEntity>(this AdminActionContext actionContext)
        where TEntity : class
        => actionContext.EntityDict.GetEntityIdOrDefault<TEntity>();

    public static byte[] GetByteArrayFromStringInput(this AdminActionContext actionContext, AdditionalFormFields field)
        => actionContext.EntityDict[field.ToString()].Compress();

    public static int? GetEntityIdOrDefault<TEntity>(this IDictionary<string, string> entityDict)
        where TEntity : class
        => int.TryParse(
            entityDict
                .FirstOrDefault(x => x.Key.Equals(typeof(TEntity).Name + "Id", StringComparison.OrdinalIgnoreCase))
                .Value,
            out var id)
            ? id
            : null;
}