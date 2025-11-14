using MiniJsonSerializerLibrary.Attributes;
using System.Collections;
using System.Reflection;
using System.Text;

public class MiniJsonSerializer
{
    private int _countSpaces;
    private readonly MiniJsonSerializerSettings _settings;
    private readonly HashSet<object> _serializedObjects = new HashSet<object>();

    public MiniJsonSerializer(MiniJsonSerializerSettings settings)
    {
        _settings = settings ?? new MiniJsonSerializerSettings();
    }

    public string Serialize(object? obj)
    {
        _countSpaces = 0;
        _serializedObjects.Clear();
        var sb = new StringBuilder();
        SerializeValue(obj, sb);
        return sb.ToString();
    }

    private void SerializeValue(object? value, StringBuilder sb)
    {
        if (value == null)
        {
            sb.Append("null");
            return;
        }

        Type type = value.GetType();

        if (!type.IsValueType && type != typeof(string))
        {
            if (_serializedObjects.Contains(value))
            {
                sb.Append("null");
                return;
            }
            _serializedObjects.Add(value);
        }

        try
        {
            if (type == typeof(string))
            {
                SerializeString((string)value, sb);
            }
            else if (type == typeof(bool))
            {
                sb.Append(value.ToString().ToLower());
            }
            else if (type == typeof(DateTime))
            {
                SerializeDateTime((DateTime)value, sb);
            }
            else if (type.IsEnum)
            {
                SerializeEnum(value, sb);
            }
            else if (IsNumericType(type))
            {
                SerializeNumber(value, type, sb);
            }
            else if (IsDictionary(type))
            {
                SerializeDictionary((IDictionary)value, sb);
            }
            else if (IsEnumerable(type))
            {
                SerializeEnumerable((IEnumerable)value, sb);
            }
            else
            {
                SerializeObject(value, sb);
            }
        }
        finally
        {
            if (!type.IsValueType && type != typeof(string))
            {
                _serializedObjects.Remove(value);
            }
        }
    }

    private void SerializeString(string value, StringBuilder sb)
    {
        sb.Append('"');
        foreach (char c in value)
        {
            switch (c)
            {
                case '"': sb.Append("\\\""); break;
                case '\t': sb.Append("\\t"); break;
                case '\n': sb.Append("\\n"); break;
                case '\\': sb.Append("\\\\"); break;
                default: sb.Append(c); break;
            }
        }
        sb.Append('"');
    }

    private void SerializeDateTime(DateTime dateTime, StringBuilder sb)
    {
        sb.Append('"');
        sb.Append(dateTime.ToString(_settings.Culture));
        sb.Append('"');
    }

    private void SerializeEnum(object enumValue, StringBuilder sb)
    {
        sb.Append('"');
        sb.Append(enumValue.ToString());
        sb.Append('"');
    }

    private void SerializeNumber(object value, Type type, StringBuilder sb)
    {
        if (type == typeof(decimal))
        {
            sb.Append(((decimal)value).ToString(_settings.Culture));
        }
        else if (type == typeof(double) || type == typeof(float))
        {
            sb.Append(Convert.ToDouble(value).ToString(_settings.Culture));
        }
        else
        {
            sb.Append(Convert.ToString(value, _settings.Culture) ?? "null");
        }
    }

    private void SerializeDictionary(IDictionary dictionary, StringBuilder sb)
    {
        if (dictionary.Count == 0)
        {
            sb.Append("{}");
            return;
        }

        if (_settings.FormatOutput)
        {
            sb.AppendLine("{");
            _countSpaces++;

            bool first = true;
            foreach (DictionaryEntry entry in dictionary)
            {
                if (!first)
                {
                    sb.AppendLine(",");
                }
                first = false;

                AppendSpace(sb);
                SerializeString(entry.Key.ToString()!, sb);
                sb.Append(": ");
                SerializeValue(entry.Value, sb);
            }

            sb.AppendLine();
            _countSpaces--;
            AppendSpace(sb);
            sb.Append('}');
        }
        else
        {
            sb.Append('{');
            bool first = true;
            foreach (DictionaryEntry entry in dictionary)
            {
                if (!first) sb.Append(',');
                first = false;
                SerializeString(entry.Key.ToString()!, sb);
                sb.Append(':');
                SerializeValue(entry.Value, sb);
            }
            sb.Append('}');
        }
    }

    private void SerializeEnumerable(IEnumerable enumerable, StringBuilder sb)
    {
        var enumerator = enumerable.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            sb.Append("[]");
            return;
        }

        if (_settings.FormatOutput)
        {
            sb.AppendLine("[");
            _countSpaces++;

            AppendSpace(sb);
            SerializeValue(enumerator.Current, sb);

            while (enumerator.MoveNext())
            {
                sb.AppendLine(",");
                AppendSpace(sb);
                SerializeValue(enumerator.Current, sb);
            }

            sb.AppendLine();
            _countSpaces--;
            AppendSpace(sb);
            sb.Append(']');
        }
        else
        {
            sb.Append('[');
            sb.Append(SerializeValueCompact(enumerator.Current));
            while (enumerator.MoveNext())
            {
                sb.Append(',');
                sb.Append(SerializeValueCompact(enumerator.Current));
            }
            sb.Append(']');
        }
    }

    private string SerializeValueCompact(object? value)
    {
        if (value == null) return "null";

        var sb = new StringBuilder();
        SerializeValue(value, sb);
        return sb.ToString();
    }

    private void SerializeObject(object obj, StringBuilder sb)
    {
        Type type = obj.GetType();
        var members = GetSerializableMembers(type);

        if (_settings.IgnoreNullValues)
        {
            members = members.Where(m => GetMemberValue(m, obj) != null).ToList();
        }

        if (members.Count == 0)
        {
            sb.Append("{}");
            return;
        }

        if (_settings.FormatOutput)
        {
            sb.AppendLine("{");
            _countSpaces++;

            bool first = true;
            foreach (var member in members)
            {
                var memberValue = GetMemberValue(member, obj);
                if (_settings.IgnoreNullValues && memberValue == null)
                    continue;

                if (!first)
                {
                    sb.AppendLine(",");
                }
                first = false;

                AppendSpace(sb);
                string jsonName = GetJsonName(member);
                SerializeString(jsonName, sb);
                sb.Append(": ");

                SerializeValue(memberValue, sb);
            }

            sb.AppendLine();
            _countSpaces--;
            AppendSpace(sb);
            sb.Append('}');
        }
        else
        {
            sb.Append('{');
            bool first = true;
            foreach (var member in members)
            {
                var memberValue = GetMemberValue(member, obj);
                if (_settings.IgnoreNullValues && memberValue == null)
                    continue;

                if (!first) sb.Append(',');
                first = false;

                string jsonName = GetJsonName(member);
                SerializeString(jsonName, sb);
                sb.Append(':');
                SerializeValue(memberValue, sb);
            }
            sb.Append('}');
        }
    }

    private List<MemberInfo> GetSerializableMembers(Type type)
    {
        var members = new List<MemberInfo>();

        var properties = type.GetProperties(_settings.PropertyBindingFlags);
        foreach (var property in properties)
        {
            if (property.CanRead &&
                property.GetCustomAttribute<JsonIgnoreAttribute>() == null &&
                property.GetIndexParameters().Length == 0)
            {
                members.Add(property);
            }
        }

        var fields = type.GetFields(_settings.FieldBindingFlags);
        foreach (var field in fields)
        {
            if (field.GetCustomAttribute<JsonIgnoreAttribute>() == null)
            {
                members.Add(field);
            }
        }

        return members;
    }

    private object? GetMemberValue(MemberInfo member, object obj)
    {
        if (member is PropertyInfo property)
        {
            return property.GetValue(obj);
        }
        else if (member is FieldInfo field)
        {
            return field.GetValue(obj);
        }
        return null;
    }

    private string GetJsonName(MemberInfo member)
    {
        var jsonNameAttr = member.GetCustomAttribute<JsonNameAttribute>();
        return jsonNameAttr?.Name ?? member.Name;
    }

    private void AppendSpace(StringBuilder sb)
    {
        if (_settings.FormatOutput)
        {
            sb.Append(' ', _countSpaces * _settings.IndentSize);
        }
    }

    private bool IsNumericType(Type type)
    {
        return type == typeof(int) ||
            type == typeof(long) ||
            type == typeof(double) ||
            type == typeof(float) ||
            type == typeof(decimal) ||
            type == typeof(short) ||
            type == typeof(byte) ||
            type == typeof(sbyte) ||
            type == typeof(ushort) ||
            type == typeof(uint) ||
            type == typeof(ulong);
    }

    private bool IsDictionary(Type type)
    {
        return typeof(IDictionary).IsAssignableFrom(type);
    }

    private bool IsEnumerable(Type type)
    {
        return typeof(IEnumerable).IsAssignableFrom(type) && !typeof(string).IsAssignableFrom(type);
    }
}