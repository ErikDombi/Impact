namespace Impact.Attributes.HttpMethods;

[AttributeUsage(AttributeTargets.Method)]
public class HttpAttribute : Attribute
{
    public string? Routing = null;

    public HttpAttribute(string? Name)
    {
        this.Routing = Name;
    }
}