using Avalonia.Input;
using System;
using System.Windows.Input;

namespace SkyLauncher.ViewModels;

/// <summary>
/// 无参数版本
/// </summary>
public class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add { }
        remove { }
    }

    public bool CanExecute(object? parameter) => _canExecute == null || _canExecute();
    public void Execute(object? parameter) => _execute();
}

/// <summary>
/// 泛型版本 — 接受一个参数
/// </summary>
public class RelayCommand<T> : ICommand
{
    private readonly Action<T?> _execute;
    private readonly Func<T?, bool>? _canExecute;

    public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add { }
        remove { }
    }

    public bool CanExecute(object? parameter)
    {
        if (_canExecute == null) return true;
        if (parameter is T t) return _canExecute(t);
        return _canExecute(default);
    }

    public void Execute(object? parameter)
    {
        if (parameter is T t)
            _execute(t);
        else
            _execute(default);
    }
}

