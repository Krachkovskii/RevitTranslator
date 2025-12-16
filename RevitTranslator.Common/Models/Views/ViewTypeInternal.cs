using System.ComponentModel;

namespace RevitTranslator.Common.Models.Views;

public enum ViewTypeInternal
{
    [Description("All Types")]
    All,

    [Description("Sheets")]
    Sheet = 6,

    [Description("Floor Plans")]
    FloorPlan = 1,
    
    [Description("Ceiling Plans")]
    CeilingPlan = 2,

    [Description("Elevations")]
    Elevation = 3,

    [Description("Sections")]
    Section = 117,

    [Description("3D Views")]
    ThreeD = 4,

    [Description("Schedules")]
    Schedule = 5,

    [Description("Legends")]
    Legend = 11,

    Undefined
}