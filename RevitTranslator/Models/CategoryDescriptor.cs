namespace RevitTranslator.Models;

public class CategoryDescriptor
{
    //TODO: Fix conversion to Long
    public long Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public bool IsChecked { get; set; }
    public bool IsBuiltInCategory { get; set; }
}