using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using E2Charts;
using System.Windows.Input;

namespace E2ControlCenter.Commands
{
    class StorygraphCommand : ICommand
    {
        private IControlCenterViewModel _ivm;

        public StorygraphCommand(IControlCenterViewModel ivm)
        {
            this._ivm = ivm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _ivm.LoadGraph(StoryType.STORYGRAPH);
        }
    }
}
