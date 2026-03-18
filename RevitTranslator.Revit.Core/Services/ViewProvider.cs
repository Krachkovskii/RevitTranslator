using RevitTranslator.Revit.Core.Contracts;
using RevitTranslator.Revit.Core.Models;
using RevitTranslator.Revit.Core.Extensions;
using RevitTranslator.Revit.Core.Utils;

namespace RevitTranslator.Revit.Core.Services;

public class ViewProvider(EventHandlers revitHandler) : IRevitViewProvider
{
    public async Task<IReadOnlyCollection<ViewDto>> GetAllIterableViewsAsync()
    {
        ViewDto[] views = [];

        await revitHandler.AsyncHandler.RaiseAsync(uiapp =>
        {
            var viewTypes = new List<Type>
            {
                typeof(ViewPlan),
                typeof(View3D),
                typeof(ViewSheet),
                typeof(ViewDrafting),
                typeof(ViewSection)
            };
            var document = uiapp.ActiveUIDocument.Document;

            views = new FilteredElementCollector(document)
                .WherePasses(new ElementMulticlassFilter(viewTypes))
                .ToElements()
                .Cast<View>()
                .Where(view => !view.IsTemplate)
                .Select(view => new ViewDto(
                    view.Id.ToLong(),
                    view.ViewType.ToInternal(),
                    view is ViewSheet sheet ? $"{sheet.SheetNumber} - {sheet.Name}" : view.Name,
                    new FilteredElementCollector(document, view.Id).GetElementCount()))
                .Where(view => view.ViewType != ViewTypeInternal.Undefined)
                .ToArray();
        });

        return views;
    }

    public async Task<IReadOnlyCollection<ViewDto>> GetAllSheetsAsync()
    {
        ViewDto[] views = [];

        await revitHandler.AsyncHandler.RaiseAsync((uiapp) =>
        {
            var document = uiapp.ActiveUIDocument.Document;
            views = document.EnumerateInstances<ViewSheet>()
                .Select(sheet => new ViewDto(
                    Id: sheet.Id.ToLong(),
                    ViewType: sheet.ViewType.ToInternal(),
                    Name: $"{sheet.SheetNumber} - {sheet.Name}",
                    ElementCount: new FilteredElementCollector(document, sheet.Id).GetElementCount()))
                .ToArray();
        });

        return views;
    }

    public async Task<IReadOnlyCollection<ViewGroupDto>> GetAllSheetCollectionsAsync()
    {
        ViewGroupDto[] groups = [];

#if NET8_0_OR_GREATER
        await revitHandler.AsyncHandler.RaiseAsync(uiapp =>
        {
            var document = uiapp.ActiveUIDocument.Document;

            groups = document.EnumerateInstances<ViewSheet>()
                .GroupBy(sheet => sheet.SheetCollectionId)
                .Select(collection => new ViewGroupDto
                {
                    Name = collection.First()
                               .SheetCollectionId
                               .ToElement<SheetCollection>(document)?
                               .Name
                           ?? "Ungrouped sheets",
                    Views = collection.Select(sheet => new ViewDto(
                            Id: sheet.Id.ToLong(),
                            ViewType: sheet.ViewType.ToInternal(),
                            Name: $"{sheet.SheetNumber} - {sheet.Name}",
                            ElementCount: new FilteredElementCollector(document, sheet.Id).GetElementCount()))
                        .ToArray()
                })
                .ToArray();
        });
#else
        await Task.CompletedTask;
#endif

        return groups;
    }
}
