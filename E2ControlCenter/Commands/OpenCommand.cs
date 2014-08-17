using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Data;
using E2Data;

namespace E2ControlCenter.Commands
{
    class OpenCommand : ICommand
    {
        private IControlCenterViewModel _ivm;

        public OpenCommand(IControlCenterViewModel ivm)
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
            IDataImporter dataImporter = new CsvImporter();
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "Document";
            dlg.DefaultExt = ".txt";
            dlg.Filter = "CSV documents (.csv)|*.csv|Text documents (.txt)|*.txt|All Files (*.*)|*.*";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                DataTable d;
                try
                {
                    d = dataImporter.Import(dlg.FileName);
                    _ivm.FeedData(d, true);
                }
                catch
                {
                    _ivm.FeedData(null, false);
                }
            }
            else
            {
                _ivm.FeedData(null, false);
            }
        }
    }
}
