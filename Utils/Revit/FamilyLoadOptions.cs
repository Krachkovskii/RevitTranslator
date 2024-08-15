using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitTranslatorAddin.Utils.Revit;
internal class FamilyLoadOptions : IFamilyLoadOptions
{
    bool IFamilyLoadOptions.OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
    {
        overwriteParameterValues = true;
        return true;
    }
    bool IFamilyLoadOptions.OnSharedFamilyFound(Family sharedFamily, bool familyInUse, out FamilySource source, out bool overwriteParameterValues) 
    {
        source = FamilySource.Family;
        overwriteParameterValues = true;
        return true;
    }
}
