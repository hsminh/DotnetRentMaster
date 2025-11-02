namespace RentMaster.Core.Middleware;

public static class HttpContextExtensions
{
    public static T? GetCurrentUser<T>(this HttpContext context) where T : class
        => context.Items["user"] as T;

    public static Guid? GetCurrentUserUid(this HttpContext context)
        => context.Items["uid"] is Guid uid ? uid : null;

    public static string? GetCurrentUserRole(this HttpContext context)
        => context.Items["role"] as string;
}