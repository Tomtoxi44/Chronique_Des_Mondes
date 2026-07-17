// -----------------------------------------------------------------------
// <copyright file="ValidationFilter.cs" company="ANGIBAUD Tommy">
// Copyright (c) ANGIBAUD Tommy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Cdm.ApiService.Filters;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Endpoint filter that validates a request body of type <typeparamref name="T"/>
/// using its DataAnnotations attributes. Minimal APIs do not run DataAnnotations
/// validation automatically, so this filter enforces it (audit fix #5).
/// </summary>
/// <typeparam name="T">The request type to validate.</typeparam>
public class ValidationFilter<T> : IEndpointFilter
    where T : class
{
    /// <inheritdoc />
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var argument = context.Arguments.OfType<T>().FirstOrDefault();

        if (argument is not null)
        {
            var validationContext = new ValidationContext(argument);
            var results = new List<ValidationResult>();

            if (!Validator.TryValidateObject(argument, validationContext, results, validateAllProperties: true))
            {
                var errors = results
                    .SelectMany(r => r.MemberNames.DefaultIfEmpty(string.Empty), (r, member) => (Member: member, r.ErrorMessage))
                    .GroupBy(x => x.Member)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(x => x.ErrorMessage ?? "Invalid value").ToArray());

                return Results.ValidationProblem(errors);
            }
        }

        return await next(context);
    }
}
