using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Shared.Core.Helpers;

public static class LogHelper
{
    public static void LogInfo<T>(
        ILogger<T> logger,
        string message,
        object[]? args = null,
        [CallerMemberName] string methodName = "")
    {
        var tag = $"({typeof(T).Name}_{methodName})";
        logger.LogInformation(tag + " " + message, args ?? []);
    }

    public static void LogWarning<T>(
        ILogger<T> logger,
        string message,
        object[]? args = null,
        [CallerMemberName] string methodName = "")
    {
        var tag = $"({typeof(T).Name}_{methodName})";
        logger.LogWarning(tag + " " + message, args ?? []);
    }

    public static void LogError<T>(
        ILogger<T> logger,
        string message,
        object[]? args = null,
        [CallerMemberName] string methodName = "")
    {
        var tag = $"({typeof(T).Name}_{methodName})";
        logger.LogError(tag + " " + message, args ?? []);
    }
}