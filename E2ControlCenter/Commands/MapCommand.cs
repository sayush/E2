using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using E2Charts;

namespace E2ControlCenter.Commands
{
    class MapCommand: ICommand
    {
        private IControlCenterViewModel _ivm;

        public MapCommand(IControlCenterViewModel ivm)
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
            _ivm.LoadGraph(StoryType.MAP);
        }
    }
}
