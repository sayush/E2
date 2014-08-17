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
using E2Charts.Legend;

namespace E2Charts
{
    public class StoryDrawingManager: IDrawingManager
    {
        private Scene _dsd;
        private Dictionary<string, object> _layoutObjects;
        private DataTable _dt;
 
        private StoryTable _storyTable;
        private Dictionary<string, object> _meta;
        private StoryType _storyType;

        private double _drawingHeight, _drawingWidth;

        private bool refresh;
        private ForceParameters _forceParameters;
        

        public StoryDrawingManager(Dictionary<string, object> layoutObjects, DataTable dt, StoryType st)
        {
            this._storyType = st;
            this._layoutObjects = layoutObjects;
            this._dt = dt;
            this._meta = new Dictionary<string, object>();
            refresh = RefreshMeta();
        }

        public StoryDrawingManager(Dictionary<string, object> layoutObjects, DataTable dt, StoryType st, ForceParameters f)
        {
            this._storyType = st;
            this._layoutObjects = layoutObjects;
            this._dt = dt;
            this._meta = new Dictionary<string, object>();
            refresh = RefreshMeta();

            this._forceParameters = f;
        }

        private bool RefreshDrawingData()
        {
            if (_drawingHeight < 1 || _drawingWidth < 1) return false;

            StoryToolkit stk = new StoryToolkit(_dt, _meta, _drawingWidth, _drawingHeight);
            _storyTable = stk.GetStoryTable();
            return true;
        }

        private bool RefreshMeta()
        {
            try
            {
                var v = _dt.AsEnumerable().Select(al => al.Field<double>(Properties.Settings.Default.StorygraphLatitudeName)).Distinct().ToList();
                if (!_meta.ContainsKey("maxLat")) _meta.Add("maxLat", v.Max()); else _meta["maxLat"] = v.Max();
                if (!_meta.ContainsKey("minLat")) _meta.Add("minLat", v.Min()); else _meta["minLat"] = v.Min();

                v = _dt.AsEnumerable().Select(al => al.Field<double>(Properties.Settings.Default.StorygraphLongitudeName)).Distinct().ToList();
                if (!_meta.ContainsKey("maxLng")) _meta.Add("maxLng", v.Max()); else _meta["maxLng"] = v.Max();
                if (!_meta.ContainsKey("minLng")) _meta.Add("minLng", v.Min()); else _meta["minLng"] = v.Min();

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
                c.Children.Add(GraphText(j - 12, 8.0, k.ToString("MM/yy\nHH:mm"), Properties.Settings.Default.StorygraphFontSize, HorizontalAlignment.Center));
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

        private void DrawLongitudeAxis(Canvas c)
        {
            int VTICKNUM = Properties.Settings.Default.StorygraphVerticalTicks;
            c.Children.Clear();
            double x = 0;
            double increment = _drawingHeight / VTICKNUM;
            double tincrement = ((double)_meta["maxLng"] - (double)_meta["minLng"]) / VTICKNUM;

            GeometryGroup grp = new GeometryGroup();
            double j = c.ActualHeight;
            double k = (double)_meta["minLng"];
            while ((c.ActualHeight - j) < c.ActualHeight)
            {
                grp.Children.Add(new LineGeometry(new Point(x, j), new Point(x + Properties.Settings.Default.StorygraphTickLength, j)));
                c.Children.Add(GraphText(x+8, j - 8, String.Format("{0:0.0000}", k), Properties.Settings.Default.StorygraphFontSize, HorizontalAlignment.Left));
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

        private void DrawLatitudeAxis(Canvas c)
        {
            int VTICKNUM = Properties.Settings.Default.StorygraphVerticalTicks;
            c.Children.Clear();
            double x = c.ActualWidth - Properties.Settings.Default.StorygraphTickLength;
            double increment = _drawingHeight / VTICKNUM;
            double tincrement = ((double)_meta["maxLat"] - (double)_meta["minLat"]) / VTICKNUM;

            GeometryGroup grp = new GeometryGroup();
            double j = c.ActualHeight;
            double k = (double)_meta["minLat"];
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

            DrawLatitudeAxis((Canvas)_layoutObjects["canvasLatitude"]);
            DrawLongitudeAxis((Canvas)_layoutObjects["canvasLongitude"]);
            DrawTimeAxis((Canvas)_layoutObjects["canvasDate"]);
            return true;
        }

        public void DrawGraph()
        {
            if (refresh)
            {
                try
                {
                    switch (_storyType)
                    {
                        case StoryType.STORYGRAPH:
                            _dsd = new D2DStorygraphScene(_storyTable, true);
                            break;
                        case StoryType.STORYGRAPHWFORCE:
                            //_dsd = new D2DStorygraphFDEBScene(_storyTable, _forceParameters, true);
                            _dsd = new D2DEdgeBundlingScene(_storyTable, _forceParameters, true);
                            break;
                        case StoryType.STORYLINES:
                            _dsd = new D2DStorylinesScene(_storyTable, true);
                            break;
                        case StoryType.STORYLINESWU:
                            _dsd = new D2DStorylinesUScene(_storyTable, true); 
                            break;
                        default:
                            _dsd = new D2DStorygraphScene(_storyTable, true);
                            break;
                    }
                    
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
            if (RefreshMeta())
            {
                DrawLayout();
                RefreshDrawingData();
                DrawGraph();
                if(!_isLegendPresent)
                    DrawLegend();
            }
        }

        
        public bool _isLegendPresent { get; set; }
        public void DrawLegend()
        {
            _isLegendPresent = true;
            LegendContent lc = new LegendContent(_storyTable);
            Window lwnd = new Window() { Title = "Legend", Width=200, WindowStyle=WindowStyle.ToolWindow };
            lwnd.Closed += new EventHandler(lwnd_Closed); 
            lc.Draw();
            lwnd.Content = lc;
            lwnd.Show();
        }

        void lwnd_Closed(object sender, EventArgs e)
        {
            _isLegendPresent = false;
        }
    }
}
