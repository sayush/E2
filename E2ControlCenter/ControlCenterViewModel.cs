using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using E2Charts;
using System.Data;
using E2ControlCenter.Commands;
using System.Windows;
using System.ComponentModel;
using E2ControlCenter.ColorPicker;

namespace E2ControlCenter
{
    public class ControlCenterViewModel : IControlCenterViewModel, INotifyPropertyChanged
    {
        private DataTable _dt;
        private bool _isDataImportOk, _isSaveValid, _isForceSelected, _isUncertaintySelected;
        private int _segments = 10, _iterations = 100;

        private StoryControlCenter _scc;
        private ColorPickerWindow _cpw;

        public ICommand ColorPickerCommand { get; set; }
        public ICommand OpenCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand ExitCommand { get; set; }
        public ICommand StorygraphCommand { get; set; }
        public ICommand StorylinesCommand { get; set; }
        public ICommand StorygramCommand { get; set; }
        public ICommand MapCommand { get; set; }
        public ICommand TimelineCommand { get; set; }

        private List<Window> _graphList;

        public ControlCenterViewModel(StoryControlCenter scc)
        {
            this._scc = scc;
            initializeVariables();
            initializeCommands();
        }

        private void initializeVariables()
        {
            this._graphList = new List<Window>();
            this._isForceSelected = this._isDataImportOk = this._isSaveValid = this._isUncertaintySelected = false;
        }

        private void initializeCommands()
        {
            this.OpenCommand = new OpenCommand(this);
            this.ColorPickerCommand = new ColorPickerCommand(this);
            this.StorygraphCommand = new StorygraphCommand(this);
            this.StorylinesCommand = new StorylinesCommand(this);
            this.MapCommand = new MapCommand(this);
            this.TimelineCommand = new TimelineCommand(this);
        }

        public bool GetSaveState()
        {
            return _isSaveValid;
        }

        public bool GetDataState()
        {
            return IsDataImportOk;
        }

        public void FeedData(DataTable dt, bool isDataOk)
        {
            this._dt = dt;
            IsDataImportOk = isDataOk;

            if (IsDataImportOk)
            {
                showDataInfo();
                MessageBox.Show("Data successfully imported.");
            }
            else
                MessageBox.Show("Data Format error: could not import data.");
            //_scc.InvalidateVisual();
        }

        #region Information About Data
        private string _dataInfo = "";
        private void showDataInfo()
        {
            string str = "Rows Imported: " + _dt.Rows.Count.ToString();
            str += "\nHeaders Found: ";
            foreach (DataColumn d in _dt.Columns)
            {
                str += d.ColumnName + ", ";
            }
            str = str.Substring(0, str.Length - 2);
            DataInfo = str;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(String property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        public string DataInfo { get { return _dataInfo; } set { _dataInfo = value; OnPropertyChanged("DataInfo"); } }
        #endregion


        public bool IsDataImportOk { get { return _isDataImportOk; } set { _isDataImportOk = value; OnPropertyChanged("IsDataImportOk"); } }

        public bool IsForceSelected { get { return _isForceSelected; } set { _isForceSelected = value; OnPropertyChanged("IsForceSelected"); } }

        public bool IsUncertaintySelected { get { return _isUncertaintySelected; } set { _isUncertaintySelected = value; OnPropertyChanged("IsForceSelected"); } }

        public int Iterations { get { return _iterations; } set { _iterations = value; OnPropertyChanged("Iterations"); } }
        public int Segments { get { return _segments; } set { _segments = value; OnPropertyChanged("Segments"); } }

        public void LoadGraph(StoryType storyType)
        {
            Window g = null;
            switch (storyType)
            {
                case StoryType.STORYGRAPH:
                    g = new Storygraph(_dt, IsForceSelected, new ForceParameters() { Iterations = this.Iterations, Segments = this.Segments });
                    break;
                case StoryType.STORYLINES:
                    g = new Storylines(_dt, IsUncertaintySelected);
                    break;
                case StoryType.MAP:
                    g = new E2Charts.Map(_dt);
                    _graphList.Add(g);
                    g.Show();
                    break;
                case StoryType.TIMELINE:
                    g = new Timeline(_dt);
                    break;
            }
            _graphList.Add(g);
            g.Show();
        }
        

        public void LoadOptions(OptionWindows w)
        {
            switch (w)
            {
                case OptionWindows.COLORPICKER:
                    _cpw = new ColorPickerWindow(_dt);
                    _cpw.Owner = _scc;
                    _cpw.Show();
                    break;
            }
        }

    }
}
