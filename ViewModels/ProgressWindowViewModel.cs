using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RevitTranslatorAddin.ViewModels;
public class ProgressWindowViewModel : INotifyPropertyChanged
{
    internal static CancellationTokenSource Cts = null;

    private int _progressBarValue = 0;
    public int ProgressBarValue
    {

        get => _progressBarValue;
        set
        {
            if (_progressBarValue != value)
            {
                _progressBarValue = value;
                OnPropertyChanged(nameof(ProgressBarValue));
            }
        }
    }

    private int _progressBarMaximum = 0;
    public int ProgressBarMaximum
    {

        get => _progressBarMaximum;
        set
        {
            if (_progressBarMaximum != value)
            {
                _progressBarMaximum = value;
                OnPropertyChanged(nameof(ProgressBarMaximum));
            }
        }
    }

    private bool _isProgressBarIndeterminate = false;
    public bool IsProgressBarIndeterminate
    {

        get => _isProgressBarIndeterminate;
        set
        {
            if (_isProgressBarIndeterminate != value)
            {
                _isProgressBarIndeterminate = value;
                OnPropertyChanged(nameof(IsProgressBarIndeterminate));
            }
        }
    }

    private int _progressWindowCharacters = 0;
    public int ProgressWindowCharacters
    {

        get => _progressWindowCharacters;
        set
        {
            if (_progressWindowCharacters != value)
            {
                _progressWindowCharacters = value;
                OnPropertyChanged(nameof(ProgressWindowCharacters));
            }
        }
    }

    public ICommand CancelCommand { get; }

    public ProgressWindowViewModel()
    {
        CancelCommand = new RelayCommand(CancelTranslation);
    }

    private void CancelTranslation()
    {
        Cts?.Cancel();
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
