﻿namespace OJS.Services.Administration.Business;

using System.Security.Claims;
using System.Linq.Expressions;
using System;
using OJS.Common.Extensions;

public abstract class BasePermissionService<TEntity, TModel>
    where TModel : class
{
    public virtual bool HasReadPermission() => true;

    public virtual bool HasCreatePermission() => true;

    public virtual bool HasUpdatePermission(TModel model) => true;

    public virtual bool HasDeletePermission(int id) => true;

    public virtual bool HasFullAccess(ClaimsPrincipal user) => user.IsAdmin();

    public virtual Expression<Func<TEntity, bool>>? GeneratePermittedRecordsExpression() => null;
}