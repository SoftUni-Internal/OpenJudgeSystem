namespace OJS.Services.Infrastructure.Extensions;

using OJS.Common.Constants;
using OJS.Services.Infrastructure.Exceptions;
using OJS.Services.Infrastructure.Models;
using System.Threading.Tasks;

public static class ValidationResultExtensions
{
    public static async Task VerifyResult(this Task<ValidationResult> task)
        => (await task)
            .VerifyResult();

    public static void VerifyResult(this ValidationResult validationResult, string? explicitPropertyName = null)
    {
        if (!validationResult.IsValid)
        {
            var exception = validationResult.PropertyName == null && explicitPropertyName == null
                ? new BusinessServiceException(validationResult.Message)
                : new BusinessServiceException(
                    validationResult.Message,
                    explicitPropertyName ?? validationResult.PropertyName);

            throw exception;
        }
    }

    public static ServiceResult<T> ToServiceResult<T>(this ValidationResult validationResult, T? data = default)
        => validationResult.IsValid
            ? ServiceResult.Success(data!)
            : ServiceResult.Failure<T>(
                validationResult.ErrorCode ?? ServiceConstants.ErrorCodes.BusinessRuleViolation,
                validationResult.Message,
                validationResult.ResourceType,
                validationResult.PropertyName,
                validationResult.ErrorContext,
                validationResult.Errors);

    public static async Task<ServiceResult<T>> ToServiceResult<T>(this Task<ValidationResult> validationResultTask, T? data = default)
    {
        var validationResult = await validationResultTask;
        return validationResult.ToServiceResult(data);
    }
}