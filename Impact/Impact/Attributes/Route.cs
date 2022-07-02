using Impact.Attributes.HttpMethods;

namespace Impact.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class Route : Attribute
{
    public string? Routing = null;
    public Route(string? Name = null)
    {
        this.Routing = Name;
    }
}