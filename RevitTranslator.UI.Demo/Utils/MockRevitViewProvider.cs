using Bogus;
using JetBrains.Annotations;
using RevitTranslator.Revit.Core.Contracts;
using RevitTranslator.Revit.Core.Models;

namespace RevitTranslator.UI.Demo.Utils;

[UsedImplicitly]
public class MockRevitViewProvider : IRevitViewProvider
{
    private static readonly ViewTypeInternal[] IterableViewTypes =
    [
        ViewTypeInternal.FloorPlan,
        ViewTypeInternal.CeilingPlan,
        ViewTypeInternal.Elevation,
        ViewTypeInternal.Section,
        ViewTypeInternal.ThreeD,
        ViewTypeInternal.Legend,
    ];

    private readonly IReadOnlyCollection<ViewDto> _allSheets;
    private readonly IReadOnlyCollection<ViewGroupDto> _sheetCollections;
    private readonly IReadOnlyCollection<ViewDto> _allIterableViews;

    public MockRevitViewProvider()
    {
        var faker = new Faker();
        _allSheets = GenerateSheets(faker, faker.Random.Int(10, 40));
        _sheetCollections = GenerateSheetCollections(faker);
        _allIterableViews = GenerateIterableViews(faker);
    }

    public Task<IReadOnlyCollection<ViewDto>> GetAllSheetsAsync() => Task.FromResult(_allSheets);
    public Task<IReadOnlyCollection<ViewGroupDto>> GetAllSheetCollectionsAsync() => Task.FromResult(_sheetCollections);
    public Task<IReadOnlyCollection<ViewDto>> GetAllIterableViewsAsync() => Task.FromResult(_allIterableViews);

    private static List<ViewDto> GenerateSheets(Faker faker, int count) =>
        Enumerable.Range(1, count)
            .Select(i => new ViewDto(
                faker.Random.Long(1, 999_999),
                ViewTypeInternal.Sheet,
                $"A{i:D3} - {faker.Commerce.ProductName()}",
                faker.Random.Int(5, 200)))
            .ToList();

    private static List<ViewGroupDto> GenerateSheetCollections(Faker faker) =>
        Enumerable.Range(0, faker.Random.Int(2, 5))
            .Select(_ => new ViewGroupDto
            {
                Name = faker.Commerce.Department(),
                Views = GenerateSheets(faker, faker.Random.Int(3, 15))
            })
            .ToList();

    private static List<ViewDto> GenerateIterableViews(Faker faker) =>
        IterableViewTypes
            .SelectMany(type => Enumerable.Range(0, faker.Random.Int(2, 8))
                .Select(_ => new ViewDto(
                    faker.Random.Long(1, 999_999),
                    type,
                    faker.Commerce.ProductName(),
                    faker.Random.Int(5, 200))))
            .ToList();
}
