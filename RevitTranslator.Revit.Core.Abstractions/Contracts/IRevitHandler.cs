namespace RevitTranslator.Revit.Abstractions.Contracts;

public interface IRevitHandler
{
    Task RaiseAsync(Action handler);
}