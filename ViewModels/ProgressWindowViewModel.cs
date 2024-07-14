using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace RevitTranslatorAddin.ViewModels;

public class ProgressWindowViewModel : INotifyPropertyChanged
{
    private int _maximum = 0;
    private int _value = 0;
    private int _characterCount = 0;
    private string _statusTextBlock = string.Empty;

    internal static CancellationTokenSource Cts = null;

    public int Maximum
    {
        get => _maximum;
        set
        {
            _maximum = value;
            OnPropertyChanged(nameof(Maximum));
        }
    }

    public int Value
    {
        get => _value;
        set
        {
            _value = value;
            OnPropertyChanged(nameof(Value));
        }
    }

    public string StatusTextBlock
    {
        get => _statusTextBlock;
        set
        {
            _statusTextBlock = value;
            OnPropertyChanged(nameof(StatusTextBlock));
        }
    }

    public int CharacterCount
    {
        get => _characterCount;
        set
        {
            _characterCount = value;
            OnPropertyChanged(nameof(CharacterCount));
        }
    }

    public ICommand CancelCommand
    {
        get;
    }

    private void Cancel()
    {
        Cts?.Cancel();
    }

        public ProgressWindowViewModel()
    {
        CancelCommand = new RelayCommand(Cancel);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}