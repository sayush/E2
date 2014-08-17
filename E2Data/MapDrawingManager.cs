using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using m=Microsoft.Maps.MapControl.WPF;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;

namespace E2.Graph
{
    public class MapDrawingManager:IDrawingManager
    {
        private m.Map _map;
        private Dictionary<string, object> _layoutObjects;
        private DataTable _dt;
 
        private LatLngColor[] _latLngColorTable;
        private Dictionary<string, object> _meta;
        private StoryType _storyType;

        private bool refresh;

       
        public MapDrawingManager(Dictionary<string, object> layoutObjects, DataTable dataTable, StoryType st)
        {
            this._storyType = st;
            this._layoutObjects = layoutObjects;
            this._dt = dataTable;
            this._meta = new Dictionary<string, object>();
            refresh = RefreshMeta();
        }

        private bool RefreshDrawingData()
        {
            StoryToolkit stk = new StoryToolkit(_dt, _meta, -1, -1);
            _latLngColorTable = stk.GetLatLngColorTable();
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

                if (!_meta.ContainsKey("maxDate")) _meta.Add("maxDate", null);
                if (!_meta.ContainsKey("minDate")) _meta.Add("minDate", null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }

        private void InitMap()
        {
            if (!RefreshMeta()) return;
            _map = ((m.Map)_layoutObjects["canvasMap"]);
            _map.SetView(new m.LocationRect(new m.Location((double)_meta["minLat"], (double)_meta["minLng"]),
                new m.Location((double)_meta["maxLat"], (double)_meta["maxLng"])));
            RefreshDrawingData();
        }

        private UIElement CreateMarker(string color)
        {
            String s = null;
            if ("" != color) s = "#" + color.Substring(0, 6);
            else s = "#FF0000";
            SolidColorBrush scb = new BrushConverter().ConvertFromString(s) as SolidColorBrush;
            return new Ellipse()
            {
                Height = 4,
                Width = 4,
                //Stroke = Brushes.White,
                Fill = scb
            };
            
            //return new Rectangle()
            //{
            //    Height = 3,
            //    Width = 3,
            //    Stroke = scb,
            //    Fill = scb
            //};
        }

        private void DrawMap()
        {
            m.MapLayer layer = new m.MapLayer();
            for (int i = 0; i < _latLngColorTable.GetLength(0); i++)
            {
                layer.AddChild(CreateMarker(_latLngColorTable[i].Color), new m.Location(_latLngColorTable[i].Latitude, _latLngColorTable[i].Longitude));
            }
            _map.Children.Add(layer);
        }

        public void Draw()
        {
            InitMap();
            DrawMap();
        }
    }
}
