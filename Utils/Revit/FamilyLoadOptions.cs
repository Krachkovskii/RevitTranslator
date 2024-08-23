namespace RevitTranslatorAddin.Utils.Revit;
/// <summary>
/// Implementation of IFamilyLoadOptions for this project. Overrides parameter values on load.
/// </summary>
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
