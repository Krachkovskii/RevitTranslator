namespace RevitTranslator.Extensions;

public static class ElementIdExtensions
{
    public static long ToLong(this ElementId elementId)
    {
#if REVIT2024_OR_GREATER
        return elementId.Value;
#else
        return elementId.IntegerValue;
#endif
    }

    public static ElementId ToElementId(this long longValue)
    {
#if REVIT2024_OR_GREATER
        return new ElementId(longValue);
#else
        return new ElementId((int)longValue);
#endif
    }

    public static Element ToElement(this ElementId elementId)
    {
        return Context.ActiveDocument?.GetElement(elementId);
    }
}