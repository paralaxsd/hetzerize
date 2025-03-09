using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Hetzerize.Extensions;

static class NullableExtensions
{
    public static T NotNull<T>(this T? obj, string? message = null,
        // https://learn.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.callerargumentexpressionattribute
        [CallerArgumentExpression("obj")] string? argumentExpression = null)
        where T : class
    {
        if (obj == null)
        {
            var defaultMsg = $"Object of type '{typeof(T)}' should not be null";
            Throw(message, defaultMsg, argumentExpression);
        }
        return obj;
    }

    [DoesNotReturn]
    static void Throw(string? message, string defaultMessage, string? argumentExpression) =>
        throw new ArgumentException(message ?? defaultMessage,
            message == null ? argumentExpression : null);
}