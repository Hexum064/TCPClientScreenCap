using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClientScreenCap
{
    public class SimpleCommand : ICommand
    {
        private Action _commandAction = null;

        public SimpleCommand(Action commandAction)
        {
            _commandAction = commandAction;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter = null)
        {
            _commandAction.Invoke();
        }
    }

    public class SimpleCommand<T> : ICommand
    {
        private Action<T> _commandAction = null;

        public SimpleCommand(Action<T> commandAction)
        {
            _commandAction = commandAction;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _commandAction.Invoke((T)Convert.ChangeType(parameter, typeof(T)));
        }
    }
}
