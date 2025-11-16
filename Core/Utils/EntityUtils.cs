public static class EntityUtils
{
    public static T CloneEntity<T>(T entity) where T : class, new()
    {
        var clone = new T();
        foreach (var prop in typeof(T).GetProperties())
        {
            if (prop.CanRead && prop.CanWrite)
            {
                prop.SetValue(clone, prop.GetValue(entity));
            }
        }
        return clone;
    }
}