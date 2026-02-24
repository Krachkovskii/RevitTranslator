using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using RevitTranslator.Common.Extensions;

namespace RevitTranslator.Common.Services;

public sealed class ScopedWindowService(IServiceProvider rootProvider)
{
    private IServiceScope? _scope;

    private Window InitializeWindow<T>(Window? parentWindow) where T : Window
    {
        if (_scope is not null)
        {
            var opened = _scope.ServiceProvider.GetRequiredService<T>();
            ActivateWindow(opened);
            return opened;
        }

        _scope = rootProvider.CreateScope();
        var serviceProvider = _scope.ServiceProvider;

        var window = serviceProvider.GetRequiredService<T>();
        window.Owner = parentWindow;
        window.Closed += WindowClosed;
        return window;
    }

    public void Show<T>(Window? parentWindow) where T : Window
    {
        var window = InitializeWindow<T>(parentWindow);
        window.Show();
    }

    public bool? ShowDialog<T>(Window? parentWindow) where T : Window
    {
        var window = InitializeWindow<T>(parentWindow);
        return window.ShowDialog(parentWindow);
    }

    public void Show<T>(Window? parentWindow, Action<IServiceProvider>? afterShowAction) where T : Window
    {
        var hadScope = _scope is not null;
        var window = InitializeWindow<T>(parentWindow);

        if (hadScope)
        {
            afterShowAction?.Invoke(_scope!.ServiceProvider);
            return;
        }

        window.Show();
        afterShowAction?.Invoke(_scope!.ServiceProvider);
    }
    
    private static void ActivateWindow(Window window)
    {
        if (window.WindowState == WindowState.Minimized) window.WindowState = WindowState.Normal;

        window.Activate();
    }

    private void WindowClosed(object? sender, EventArgs e)
    {
        if (sender is not Window window) return;

        window.Closed -= WindowClosed;
        window.Owner?.Activate();
        window.Owner = null;
        _scope?.Dispose();
        _scope = null;
    }
}