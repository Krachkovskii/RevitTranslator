namespace RevitTranslator.Ui.Library.Extensions;

public static class VisualExtensions
{
    public static T? FindVisualParent<T>(this FrameworkElement element) where T : FrameworkElement
    {
        var parentElement = (FrameworkElement?)VisualTreeHelper.GetParent(element);
        while (parentElement is not null)
        {
            if (parentElement is T parent)
                return parent;

            parentElement = (FrameworkElement?)VisualTreeHelper.GetParent(parentElement);
        }

        return null;
    }

    public static T? FindVisualParent<T>(this FrameworkElement element, string name) where T : FrameworkElement
    {
        var parentElement = (FrameworkElement?)VisualTreeHelper.GetParent(element);
        while (parentElement is not null)
        {
            if (parentElement is T parent)
                if (parentElement.Name == name)
                    return parent;

            parentElement = (FrameworkElement?)VisualTreeHelper.GetParent(parentElement);
        }

        return null;
    }

    public static T? FindVisualChild<T>(this FrameworkElement element) where T : Visual
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
        {
            var childElement = (FrameworkElement?)VisualTreeHelper.GetChild(element, i);
            if (childElement is null) return null;

            if (childElement is T child)
                return child;

            var descendent = FindVisualChild<T>(childElement);
            if (descendent is not null) return descendent;
        }

        return null;
    }

    public static IEnumerable<T> FindChildren<T>(this DependencyObject parent) where T : DependencyObject
    {
        var children = new List<T>();
        if (parent == null) return children;

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T typedChild)
            {
                children.Add(typedChild);
            }

            // Recursively search the visual tree.
            children.AddRange(FindChildren<T>(child));
        }

        return children;
    }

    public static T? FindVisualChild<T>(this FrameworkElement element, string name) where T : Visual
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
        {
            if (VisualTreeHelper.GetChild(element, i) is not FrameworkElement childElement) return null;

            if (childElement is T child)
                if (childElement.Name == name)
                    return child;

            var descendent = FindVisualChild<T>(childElement, name);
            if (descendent is not null) return descendent;
        }

        return null;
    }

    public static T? FindParent<T>(this DependencyObject? child) where T : DependencyObject
    {
        if (child is null) return null;

        var parentObject = VisualTreeHelper.GetParent(child);

        if (parentObject is null && child is FrameworkElement currentParent)
        {
            parentObject = LogicalTreeHelper.GetParent(currentParent);
        }

        while (parentObject is not null)
        {
            if (parentObject is T parent) return parent;

            parentObject = VisualTreeHelper.GetParent(parentObject);

            if (parentObject is null && child is FrameworkElement parentElement)
            {
                parentObject = LogicalTreeHelper.GetParent(parentElement);
            }
        }

        return null;
    }

    public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject? dependencyObject)
        where T : DependencyObject
    {
        if (dependencyObject == null) yield break;

        var count = VisualTreeHelper.GetChildrenCount(dependencyObject);
        for (var i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(dependencyObject, i);
            if (child is T t)
                yield return t;

            foreach (var descendant in FindVisualChildren<T>(child))
                yield return descendant;
        }
    }
}