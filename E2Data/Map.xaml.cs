using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data;

namespace E2.Graph
{
    /// <summary>
    /// Interaction logic for Map.xaml
    /// </summary>
    public partial class Map : Window, IGraph
    {
        public IViewModel svm { get; set; }

        public Map(DataTable data)
        {
            InitializeComponent();
            svm = new MapViewModel(this, data, StoryType.MAP);
            this.DataContext = svm;
        }

        public Dictionary<string, object> GetLayout()
        {
            Dictionary<string, object> d = new Dictionary<string, object>();
            d.Add("canvasMap", (Object)this.mapControl);
            return d;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            svm.Draw();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            svm.Draw();
        }
    }
}
