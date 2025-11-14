using System.Globalization;
using System.Reflection;

public class MiniJsonSerializerSettings
{
    public BindingFlags PropertyBindingFlags { get; set; } = BindingFlags.Public | BindingFlags.Instance;
    public BindingFlags FieldBindingFlags { get; set; } = BindingFlags.Public | BindingFlags.Instance;
    public CultureInfo Culture { get; set; } = CultureInfo.CurrentCulture;
    public bool FormatOutput { get; set; } = true;
    public int IndentSize { get; set; } = 2;
    public bool IgnoreNullValues { get; set; } = false;
}
