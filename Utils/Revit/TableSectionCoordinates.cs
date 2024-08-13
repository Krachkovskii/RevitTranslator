using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitTranslatorAddin.Utils.Revit;
/// <summary>
/// Represents grid coordinates (row and column) of a Schedule cell
/// </summary>
public class ScheduleCellCoordinates
{
    /// <summary>
    /// The row of the cell
    /// </summary>
    public int Row
    {
        get; set;
    }

    /// <summary>
    /// The column of the cell
    /// </summary>
    public int Column
    {
        get; set; 
    }
}
