using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using E2ControlCenter.ColorPicker;

namespace E2ControlCenter.Commands
{
    class ColorPickerCommand: ICommand
    {
        private IControlCenterViewModel _ivm;

        public ColorPickerCommand(IControlCenterViewModel ivm)
        {
            this._ivm = ivm;
        }

        public bool CanExecute(object parameter)
        {
            //return _ivm.GetDataState();
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _ivm.LoadOptions(OptionWindows.COLORPICKER);
        }
    }
}
