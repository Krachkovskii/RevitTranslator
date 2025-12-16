using RevitTranslator.Common.Contracts;
using RevitTranslator.Common.Extensions;
using RevitTranslator.Common.Models.Views;
using RevitTranslator.Extensions;

namespace RevitTranslator.Services;

public class ViewProvider : IRevitViewProvider
{
    public IReadOnlyCollection<ViewDto> GetAllIterableViews()
    {
        var viewTypes = new List<Type>
        {
            typeof(ViewPlan),
            typeof(View3D),
            typeof(ViewSheet),
            typeof(ViewDrafting),
            typeof(ViewSection)
        };
        var document = Context.ActiveDocument;

        return new FilteredElementCollector(document)
            .WherePasses(new ElementMulticlassFilter(viewTypes))
            .ToElements()
            .Cast<View>()
            .Where(view => !view.IsTemplate)
            .Select(view => new ViewDto(view.Id.ToLong(),
                view.ViewType.ToInternal(),
                view is ViewSheet sheet ? $"{sheet.SheetNumber} - {sheet.Name}" : view.Name))
            .Where(view => view.ViewType != ViewTypeInternal.Undefined)
            .ToArray();
    }

    public IReadOnlyCollection<ViewDto> GetAllSheets()
    {
        var document = Context.ActiveDocument;
        if (document is null) return [];

        return document.EnumerateInstances<ViewSheet>()
            .Select(sheet => new ViewDto(
                Id: sheet.Id.ToLong(),
                ViewType: sheet.ViewType.ToInternal(),
                Name: $"{sheet.SheetNumber} - {sheet.Name}"))
            .ToArray();
    }

#if NET8_0_OR_GREATER
    public IReadOnlyCollection<ViewGroupDto> GetAllSheetCollections()
    {
        var document = Context.ActiveDocument;
        if (document is null) return [];

        return document.EnumerateInstances<ViewSheet>()
            .GroupBy(sheet => sheet.SheetCollectionId)
            .Select(collection => new ViewGroupDto
            {
                Name = collection.First()
                           .SheetCollectionId
                           .ToElement<SheetCollection>(document)?
                           .Name
                       ?? "<Unnamed sheet collection>",
                Views = collection.Select(sheet => new ViewDto(
                        Id: sheet.Id.ToLong(),
                        ViewType: sheet.ViewType.ToInternal(),
                        Name: $"{sheet.SheetNumber} - {sheet.Name}"))
                    .ToArray()
            })
            .ToArray();
    }
#endif
}