namespace RevitTranslator.Models;

public class CategoryDescriptor
{
#if REVIT2025_OR_GREATER
    public long Id { get; set; }
#else
    public int Id { get; set; }
#endif
    public string Name { get; set; }
    public string Type { get; set; }
    public bool IsChecked { get; set; }
    public bool IsBuiltInCategory { get; set; }
}