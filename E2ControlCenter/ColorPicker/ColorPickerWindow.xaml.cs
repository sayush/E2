using System;
using System.Windows;
using System.Data;
using System.Collections;

namespace E2ControlCenter.ColorPicker
{
    /// <summary>
    /// Interaction logic for ColorPickerWindow.xaml
    /// </summary>
    public partial class ColorPickerWindow : Window
    {
        private DataTable _dt;

        //public DataView ColorList { get { return _dt.DefaultView; } set; }

        public DataView ColorList
        {
            get
            {
                String[] columns = { "Label", "Color" };
                return _dt.DefaultView.ToTable(true, columns).DefaultView;
            }
        }

        public IEnumerable Stories { get; set; }

        public ColorPickerWindow(DataTable dt)
        {
            InitializeComponent();
            DataContext = this;
            this._dt = dt;
        }
    }
}
