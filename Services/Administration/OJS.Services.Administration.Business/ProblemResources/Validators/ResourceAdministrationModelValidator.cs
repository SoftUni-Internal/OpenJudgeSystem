namespace OJS.Services.Administration.Business.ProblemResources.Validators;

using FluentValidation;

using OJS.Common.Enumerations;
using OJS.Data.Models.Resources;
using OJS.Services.Administration.Models.ProblemResources;
using OJS.Services.Common.Data;
using OJS.Services.Common.Data.Validation;

public class ResourceAdministrationModelValidator : BaseAdministrationModelValidator<ResourceAdministrationModel, int, Resource>
{
    public ResourceAdministrationModelValidator(IDataService<Resource> resourcesDataService)
        : base(resourcesDataService)
    {
        this.RuleFor(model => model.Name)
            .NotNull()
            .NotEmpty()
            .WithMessage("The resource's name is mandatory.")
            .When(x => x is { OperationType: CrudOperationType.Create or CrudOperationType.Update });

        this.RuleFor(model => model.ParentId)
            .GreaterThanOrEqualTo(0)
            .WithMessage("The Problem Id cannot be less than 0.")
            .When(x => x is { OperationType: CrudOperationType.Create or CrudOperationType.Update });

        this.RuleFor(model => model)
            .Must(NotContainBothLinkAndFile)
            .WithMessage("The resource cannot contain both links and files.")
            .When(x => x is { OperationType: CrudOperationType.Create or CrudOperationType.Update });

        this.RuleFor(model => model)
            .Must(ContainEitherLinkOrFile)
            .WithMessage("The resource should contain either a file or a link.")
            .When(x => x is { OperationType: CrudOperationType.Create });
    }

    private static bool NotContainBothLinkAndFile(ResourceAdministrationModel model)
    {
        if (model.File != null && !string.IsNullOrEmpty(model.Link))
        {
            return false;
        }

        return true;
    }

    private static bool ContainEitherLinkOrFile(ResourceAdministrationModel model)
        => !string.IsNullOrEmpty(model.Link) || model.File is { Length: > 0 };
}