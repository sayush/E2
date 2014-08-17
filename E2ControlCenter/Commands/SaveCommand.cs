using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace E2ControlCenter.Commands
{
    public class SaveCommand : ICommand
    {
        private IControlCenterViewModel _ivm;
        public SaveCommand(IControlCenterViewModel ivm)
        {
            this._ivm = ivm;
        }

        public bool CanExecute(object parameter)
        {
            return _ivm.GetSaveState();
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {

        }
    }
}
