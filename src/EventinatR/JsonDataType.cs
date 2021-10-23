namespace EventinatR;

public record JsonDataType(string Name, string Assembly)
{
    public static JsonDataType For<T>(T value)
        => For(value?.GetType() ?? typeof(T));

    public static JsonDataType For(Type type)
    {
        _ = type ?? throw new ArgumentNullException(nameof(type));

        return new JsonDataType(type.FullName ?? type.Name, type.Assembly.GetName().FullName);
    }
}
