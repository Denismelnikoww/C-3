[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class JsonNameAttribute : Attribute
{
    public string Name { get; }

    public JsonNameAttribute(string name)
    {
        Name = name;
    }
}