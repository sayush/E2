using System.Windows;

namespace E2ControlCenter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class StoryControlCenter : Window
    {
        private IControlCenterViewModel _ivm;

        public StoryControlCenter()
        {
            InitializeComponent();
            _ivm = new ControlCenterViewModel(this);
            this.DataContext = _ivm;
            
        }
    }
}
