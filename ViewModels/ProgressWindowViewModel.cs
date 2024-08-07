﻿using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using RevitTranslatorAddin.Utils.DeepL;
using RevitTranslatorAddin.Utils.Revit;

namespace RevitTranslatorAddin.ViewModels;

public class ProgressWindowViewModel : INotifyPropertyChanged
{
    private int _maximum = 0;
    private int _value = 0;
    private int _characterCount = 0;
    private string _statusTextBlock = string.Empty;
    private bool _isProgressBarIndeterminate = false;
    private bool _isStopEnabled = true;
    private bool _isStopRequested = false;
    private bool _translationFinished = false;
    private string _buttonText = string.Empty;
    private double _progressBarOpacity = 1;

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

    public bool IsProgressBarIndeterminate
    {
        get => _isProgressBarIndeterminate;
        set
        {
            _isProgressBarIndeterminate = value;
            OnPropertyChanged(nameof(IsProgressBarIndeterminate));
        }
    }

    public bool IsStopEnabled
    {
        get => _isStopEnabled;
        set
        {
            _isStopEnabled = value;
            OnPropertyChanged(nameof(IsStopEnabled));
        }
    }

    public bool IsStopRequested
    {
        get => _isStopRequested;
        set
        {
            _isStopRequested = value;
            OnPropertyChanged(nameof(IsStopRequested));
        }
    }

    public bool TranslationFinished
    {
        get => _translationFinished;
        set
        {
            _translationFinished = value;
            OnPropertyChanged(nameof(TranslationFinished));
        }
    }

    public string ButtonText
    {
        get => _buttonText;
        set
        {
            _buttonText = value;
            OnPropertyChanged(nameof(ButtonText));
        }
    }

    public double ProgressBarOpacity
    {
        get => _progressBarOpacity;
        set
        {
            _progressBarOpacity = value;
            OnPropertyChanged(nameof(ProgressBarOpacity));
        }
    }

    public ICommand StopCommand
    {
        get;
    }

    public ICommand CloseWindowCommand
    {
        get;
    }

    public ProgressWindowViewModel()
    {
        StopCommand = new RelayCommand(Stop);
        IsStopEnabled = true;
        ButtonText = "Stop translation";
    }

    internal void TranslationsFinished()
    {
        if (IsStopEnabled)
        {
            IsStopEnabled = false;
        }

        if (IsStopRequested)
        {
            ButtonText = "Translation was interrupted";
        }
        else
        {
            ButtonText = "Translation finished!";
        }
    }

    private void Stop()
    {
        Cts.Cancel();
        ButtonText = "Stopping...";
        IsStopEnabled = false;
        IsStopRequested = true;
        ProgressBarOpacity = 0.5;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}