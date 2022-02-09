using System.Diagnostics.CodeAnalysis;

namespace EventinatR;

public record JsonDataType(string Name, string? Assembly = null)
{
    public static JsonDataType For<T>(T value)
        => For(value?.GetType() ?? typeof(T));

    public static JsonDataType For(Type type)
    {
        _ = type ?? throw new ArgumentNullException(nameof(type));

        if (type.Assembly.IsDynamic)
        {
            return new JsonDataType(type.FullName ?? type.Name);
        }

        return new JsonDataType(type.FullName ?? type.Name, type.Assembly.GetName().FullName);
    }

    public bool TryToType([MaybeNullWhen(false)] out Type type)
    {
        type = null;

        if (!string.IsNullOrEmpty(Assembly))
        {
            type = Type.GetType($"{Name}, {Assembly}", false, true);
        }

        if (type is null)
        {
            type = Type.GetType(Name, false, true);
        }

        return type is not null;
    }
}
