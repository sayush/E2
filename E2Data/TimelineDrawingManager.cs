using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Direct2D;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;
using System.Globalization;
using System.Threading.Tasks;
using E2.Graph.Legend;

namespace E2.Graph
{
    public class TimelineDrawingManager: IDrawingManager
    {
        private Scene _dsd;
        private Dictionary<string, object> _layoutObjects;
        private DataTable _dt;
 
        private TimeFreq _storyTable;
        private Dictionary<string, object> _meta;
        private StoryType _storyType;

        private double _drawingHeight, _drawingWidth;

        private bool refresh;
        private ForceParameters _forceParameters;
        

        public TimelineDrawingManager(Dictionary<string, object> layoutObjects, DataTable dataTable, StoryType st)
        {
            this._storyType = st;
            this._layoutObjects = layoutObjects;
            this._dt = dataTable;
            this._meta = new Dictionary<string, object>();
            refresh = RefreshMeta();
        }

        private bool RefreshDrawingData()
        {
            if (_drawingHeight < 1 || _drawingWidth < 1) return false;
            StoryToolkit stk = new StoryToolkit(_dt, _meta, _drawingWidth, _drawingHeight);
            _storyTable = stk.GetTimeFrequencyTable();
            return true;
        }

        private bool RefreshMeta()
        {
            try
            {
                if (!_meta.ContainsKey("maxLat")) _meta.Add("maxLat", null); 
                if (!_meta.ContainsKey("minLat")) _meta.Add("minLat", null); 

                if (!_meta.ContainsKey("maxLng")) _meta.Add("maxLng", null); 
                if (!_meta.ContainsKey("minLng")) _meta.Add("minLng", null); 

                var vx = _dt.AsEnumerable().Select(al => al.Field<DateTime>(Properties.Settings.Default.StorygraphDateName)).Distinct().ToList();
                if (!_meta.ContainsKey("minDate")) _meta.Add("minDate", vx.Min()); else _meta["minDate"] = vx.Min();
                if (!_meta.ContainsKey("maxDate")) _meta.Add("maxDate", vx.Max()); else _meta["maxDate"] = vx.Max();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }
        
        private UIElement GraphText(double x, double y, string text, int textsize, HorizontalAlignment textalign)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = text;
            textBlock.HorizontalAlignment = textalign;
            textBlock.Foreground = Brushes.Black;
            textBlock.FontSize = textsize;
            Canvas.SetLeft(textBlock, x);
            Canvas.SetTop(textBlock, y);
            return textBlock;
        }

        private void DrawTimeAxis(Canvas c)
        {
            int HTICKNUM = Properties.Settings.Default.StorygraphHorizontalTicks;
            c.Children.Clear();
            double increment = c.ActualWidth / HTICKNUM;
            DateTime maxd = (DateTime)_meta["maxDate"];
            DateTime mind = (DateTime)_meta["minDate"];
            double tincrement = (maxd.Subtract(mind)).TotalDays / HTICKNUM;

            GeometryGroup grp = new GeometryGroup();
            double j = 0.0;
            DateTime k = (DateTime)_meta["minDate"];
            while (j <= c.ActualWidth + 1)
            {
                grp.Children.Add(new LineGeometry(new Point(j, 0), new Point(j, Properties.Settings.Default.StorygraphTickLength)));
                //c.Children.Add(GraphText(j - 12, 8.0, k.ToString("HH:mm"), Properties.Settings.Default.StorygraphFontSize, HorizontalAlignment.Center));
                c.Children.Add(GraphText(j - 12, 8.0, k.ToString("MM/yy"), Properties.Settings.Default.StorygraphFontSize, HorizontalAlignment.Center));
                j += increment;
                k = k.AddDays(tincrement);
            }

            grp.Freeze();
            Path tickPath = new Path()
            {
                Stroke = Brushes.Black,
                StrokeThickness = 1.0
            };
            tickPath.Data = grp;
            c.Children.Add(tickPath);
        }

        private void DrawFrequencyAxis(Canvas c)
        {
            int VTICKNUM = Properties.Settings.Default.TimelineVerticalTicks;
            c.Children.Clear();
            double x = c.ActualWidth - Properties.Settings.Default.StorygraphTickLength;
            double increment = _drawingHeight / VTICKNUM;

            double tincrement = (_storyTable.MaxFrequency - _storyTable.MinFrequency) / VTICKNUM;

            GeometryGroup grp = new GeometryGroup();
            double j = c.ActualHeight;
            double k = _storyTable.MinFrequency;
            while ((c.ActualHeight - j) < c.ActualHeight)
            {
                grp.Children.Add(new LineGeometry(new Point(x, j), new Point(x + Properties.Settings.Default.StorygraphTickLength, j)));
                c.Children.Add(GraphText(x - 38, j - 8, String.Format("{0:0.0000}", k), Properties.Settings.Default.StorygraphFontSize, HorizontalAlignment.Right));
                j -= increment;
                k += tincrement;
            }

            grp.Freeze();
            Path tickPath = new Path()
            {
                Stroke = Brushes.Black,
                StrokeThickness = 1.0
            };
            tickPath.Data = grp;
            c.Children.Add(tickPath);
        }

        public bool DrawLayout()
        {
            _drawingHeight = ((Direct2DControl)_layoutObjects["canvasGraph"]).ActualHeight;
            _drawingWidth = ((Direct2DControl)_layoutObjects["canvasGraph"]).ActualWidth;
            if (_drawingHeight < 1 || _drawingWidth < 1) return false;

            RefreshDrawingData();

            DrawFrequencyAxis((Canvas)_layoutObjects["canvasLatitude"]);
            DrawTimeAxis((Canvas)_layoutObjects["canvasDate"]);
            
            return true;
        }

        public void DrawGraph()
        {
            if (refresh)
            {
                try
                {
                    _dsd = new D2DTimelineScene(_storyTable, true);
                    ((Direct2DControl)_layoutObjects["canvasGraph"]).Scene = _dsd;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        public void Draw()
        {
            if (refresh)
            {
                DrawLayout();
                DrawGraph();
            }
        }
    }
}
