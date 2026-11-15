using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Api.Validation;

public sealed class ValidationFilter<T> : IEndpointFilter
    where T : class
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        T? request = context.Arguments
            .OfType<T>()
            .FirstOrDefault();

        if (request is null)
            return await next(context);

        var validator = context.HttpContext.RequestServices
            .GetRequiredService<IValidator<T>>();

        var result = await validator.ValidateAsync(
            request,
            context.HttpContext.RequestAborted);

        if (!result.IsValid)
        {
            var errors = result.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .Select(error => error.ErrorMessage)
                        .ToArray());

            return Results.ValidationProblem(errors);
        }

        return await next(context);
    }
}
