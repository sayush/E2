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
    /// Interaction logic for Storygraph.xaml
    /// </summary>
    public partial class Storylines : Window, IGraph
    {
        public IViewModel svm { get; set; }

        public Storylines(DataTable data, bool isUncertaintySelected)
        {
            InitializeComponent();
            svm = new StoryViewModel(this, data, isUncertaintySelected?StoryType.STORYLINESWU:StoryType.STORYLINES);
            this.DataContext = svm;
        }

        public Dictionary<string, object> GetLayout()
        {
            Dictionary<string, object> d = new Dictionary<string, object>();
            d.Add("canvasLatitude", (Object)this.canvasLat);
            d.Add("canvasLongitude", (Object)this.canvasLng);
            d.Add("canvasDate", (Object)this.canvasDate);
            d.Add("canvasGraph", (Object)this.d2DControl);
            return d;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            d2DControl.Scene.Dispose();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //svm.Draw();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            svm.Draw();
        }
    }
}
