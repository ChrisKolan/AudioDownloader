using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ViewModel.Helper
{
    public class ActionCommand : ICommand
    {
        #region Fields

        private Action _action;

        #endregion

        #region Constructors

        public ActionCommand(Action command)
        {
            _action = command;
        }

        #endregion

        #region Properties

        private bool _isEnabled = true;

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;

                if (CanExecuteChanged != null)
                {
                    CanExecuteChanged(this, EventArgs.Empty);
                }
            }
        }

        #endregion

        #region ICommand implementation

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return IsEnabled;
        }

        public void Execute(object parameter)
        {
            if (_action != null)
            {
                _action();
            }
        }

        #endregion
    }
}
