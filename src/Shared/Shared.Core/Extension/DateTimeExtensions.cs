namespace Shared.Core.Extension;

public static class DateTimeExtensions
{
    public static TimeSpan ToTimeSpanUntil(this DateTime futureTime)
    {
        
        var duration = futureTime - DateTime.UtcNow;
        return duration > TimeSpan.Zero ? duration : TimeSpan.Zero;
    }

    public static TimeSpan ToExpirationTimeSpan(this DateTime expiresAt)
    {
        return expiresAt.ToTimeSpanUntil();
    }
}
