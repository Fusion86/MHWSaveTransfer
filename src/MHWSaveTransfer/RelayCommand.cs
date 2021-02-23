using System;
using System.Diagnostics;
using System.Windows.Input;

namespace MHWSaveTransfer
{
    // See https://gist.github.com/schuster-rainer/2648922
    public class RelayCommand<T> : ICommand
    {
        #region Fields

        readonly Action<T> _execute;
        readonly Predicate<T>? _canExecute;

        #endregion // Fields

        #region Constructors

        public RelayCommand(Action<T> execute)
        : this(execute, null)
        {
        }

        public RelayCommand(Action<T> execute, Predicate<T>? canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }
        #endregion // Constructors

        #region ICommand Members

        [DebuggerStepThrough]
        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute((T)parameter);
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object? parameter)
        {
            _execute((T)parameter);
        }

        #endregion // ICommand Members
    }

    public class RelayCommand : RelayCommand<object>
    {
        public RelayCommand(Action execute)
            : this(execute, () => true) { }

        public RelayCommand(Action execute, Func<bool> canExecute)
            : base(param => execute(), param => canExecute()) { }
    }
}
