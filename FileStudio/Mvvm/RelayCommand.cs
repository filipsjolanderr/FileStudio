// Mvvm/RelayCommand.cs
using System;
using System.Windows.Input;

namespace FileStudio.Mvvm
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        public event EventHandler? CanExecuteChanged;

        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
            : this(_ => execute(), canExecute != null ? _ => canExecute() : null)
        {
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object? parameter)
        {
            _execute(parameter);
        }

        public void NotifyCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T?> _execute;
        private readonly Func<T?, bool>? _canExecute;

        public event EventHandler? CanExecuteChanged;

        public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            // Handle null parameter for non-nullable T if necessary, or adjust as needed
            if (parameter == null && typeof(T).IsValueType && Nullable.GetUnderlyingType(typeof(T)) == null)
            {
                // Cannot pass null to a non-nullable value type method
                // Decide how to handle this: throw, return false, etc.
                // Returning false is often safest.
                return false; 
            }
            return _canExecute == null || _canExecute((T?)parameter);
        }

        public void Execute(object? parameter)
        {
             if (parameter == null && typeof(T).IsValueType && Nullable.GetUnderlyingType(typeof(T)) == null)
            {
                // Handle execution attempt with null for non-nullable value type if needed
                // Often, CanExecute should prevent this, but as a safeguard:
                // throw new ArgumentNullException(nameof(parameter), "Cannot execute command with null parameter for non-nullable type.");
                // Or simply return if CanExecute should have caught it.
                return; 
            }
            _execute((T?)parameter);
        }

        public void NotifyCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
