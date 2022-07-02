namespace Impact.Helpers;

public static class NullHelper
{
    public static void IfNull<T>(this T? obj, Action action)
    {
        if (obj is null)
            action();
    }
    
    public static void IfNotNull<T>(this T? obj, Action action)
    {
        if (obj is not null)
            action();
    }
}