using RevitTranslator.Ui.Library.Controls;
using Controls_TextBlock = RevitTranslator.Ui.Library.Controls.TextBlock;

namespace RevitTranslator.Ui.Library.Behaviors;

public static class TextHelpers
{
    // The Attached Property definition
    public static readonly DependencyProperty TextWrappingProperty =
        DependencyProperty.RegisterAttached(
            "TextWrapping",
            typeof(TextWrapping),
            typeof(TextHelpers),
            new PropertyMetadata(TextWrapping.NoWrap, OnTextWrappingChanged)); // Default to NoWrap

    // Getter
    public static TextWrapping GetTextWrapping(DependencyObject obj)
    {
        return (TextWrapping)obj.GetValue(TextWrappingProperty);
    }

    // Setter
    public static void SetTextWrapping(DependencyObject obj, TextWrapping value)
    {
        obj.SetValue(TextWrappingProperty, value);
    }

    // Property Changed Callback
    private static void OnTextWrappingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // We only care about DependencyObjects that inherit from FrameworkElement, as they have the Loaded event.
        if (d is FrameworkElement element)
        {
            // Unsubscribe from the old handler (if necessary) to prevent leaks/multiple calls.
            // This is good practice if the property is set multiple times.
            element.Loaded -= Element_Loaded;

            // Check if the TextWrapping is anything other than NoWrap (the default)
            if ((TextWrapping)e.NewValue != TextWrapping.NoWrap)
            {
                // Subscribe to the Loaded event. This ensures the visual tree (including
                // ContentPresenter's content) is fully materialized before we search for TextBlocks.
                element.Loaded += Element_Loaded;
            }
        }
    }

    // Event Handler for the Loaded event
    private static void Element_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            // Get the user-set TextWrapping value from the attached property
            var wrappingValue = GetTextWrapping(element);

            // Use the provided FindChildren extension method to locate all TextBlocks
            // The method is called on the 'element' (which could be the ContentPresenter itself)
            // 'var' style is used as requested.
            var textBlocks = element.FindChildren<TextBlock>();

            foreach (var textBlock in textBlocks)
            {
                // Set the TextWrapping property on the found TextBlock
                textBlock.TextWrapping = wrappingValue;
            }

            // Important: Unsubscribe the handler after the work is done to prevent
            // the method being called again on subsequent Loaded events (e.g., when visibility changes)
            // and to allow the FrameworkElement to be garbage collected.
            element.Loaded -= Element_Loaded;
        }
    }
}