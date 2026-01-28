using FluentValidation.Results;
using ItSupportServer.src.Shared.Exceptions;

namespace ItSupportServer.src.Shared.Extensions
{
    public static class FluentValidationExtensions
    {
        /// <summary>
        /// Throws custom ValidationException if FluentValidation result is invalid
        /// </summary>
        public static void ThrowIfInvalid(this ValidationResult validationResult)
        {
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );
                
                throw new ValidationException(errors);
            }
        }
    }
}