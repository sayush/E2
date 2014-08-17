using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;

namespace E2.Graph
{
    public enum StoryType { STORYGRAPH, STORYGRAPHWFORCE, STORYLINES, STORYLINESWU, TIMELINE, MAP }

    public class StoryToolkit
    {
        private readonly DataTable _dt;
        private Dictionary<string, object> _meta;

        private double _height, _width;

        private DateTime _tminDate, _tmaxDate;
        private double _tmaxLat, _tminLat, _tminLng, _tmaxLng, _coeffLat, _coeffLng, _coeffDate;


        public StoryToolkit(DataTable _dt, Dictionary<string, object> _meta, double width, double height)
        {
            this._dt = _dt;
            this._meta = _meta;

            if (null != _meta["minDate"] && null != _meta["maxDate"])
            {
                this._tminDate = (DateTime)_meta["minDate"];
                this._tmaxDate = (DateTime)_meta["maxDate"];
            }
            if (null != _meta["minLat"] && null != _meta["maxLat"] && null != _meta["minLng"] && null != _meta["maxLng"])
            {
                this._tminLat = (double)_meta["minLat"];
                this._tmaxLat = (double)_meta["maxLat"];

                this._tminLng = (double)_meta["minLng"];
                this._tmaxLng = (double)_meta["maxLng"];
            }

            this._height = height;
            this._width = width;

            if ((null != _meta["minDate"] && null != _meta["maxDate"]) || (null != _meta["minLat"] && null != _meta["maxLat"] && null != _meta["minLng"] && null != _meta["maxLng"]))
            {
                this._coeffLat = _height / (_tmaxLat - _tminLat);
                this._coeffLng = _height / (_tmaxLng - _tminLng);
                this._coeffDate = _width / _tmaxDate.Subtract(_tminDate).TotalDays;
            }
        }

        public double[][] GetLocationTable() {
            double[][] f= new double[_dt.Rows.Count][];

            Parallel.For(0, _dt.Rows.Count, i =>
            {
                f[i] = new double[2];
                f[i][0] = GetYFromLatitude((double)_dt.Rows[i][Properties.Settings.Default.StorygraphLatitudeName]);
                f[i][1] = GetYFromLongitude((double)_dt.Rows[i][Properties.Settings.Default.StorygraphLongitudeName]);
            });

            return f;
        }

        public LatLngColor[] GetLatLngColorTable()
        {
            LatLngColor[] f = new LatLngColor[_dt.Rows.Count];
            Parallel.For(0, _dt.Rows.Count, i =>
            {
                f[i] = new LatLngColor((double)_dt.Rows[i][Properties.Settings.Default.StorygraphLatitudeName],
                    (double)_dt.Rows[i][Properties.Settings.Default.StorygraphLongitudeName],
                    (string)_dt.Rows[i][Properties.Settings.Default.StorygraphColorName]);
            });

            return f;
        }

        public double[][] GetEventTable()
        {
            double x1 = 0;
            double y1 = _height;

            double x2 = _width;
            double y2 = _height;

            double[][] f = new double[_dt.Rows.Count][];

            Parallel.For(0, _dt.Rows.Count, i =>
            {
                f[i] = new double[2];

                var ptx1 = x1;
                var pty1 = GetYFromLatitude((double)_dt.Rows[i][Properties.Settings.Default.StorygraphLatitudeName]);
   
                var ptx2 = x2;
                var pty2 = GetYFromLongitude((double)_dt.Rows[i][Properties.Settings.Default.StorygraphLongitudeName]);

                double m = (pty2 - pty1) / _width;

                double tempx = GetXFromEventdate((DateTime)_dt.Rows[i][Properties.Settings.Default.StorygraphDateName]);
                double tempy = m * tempx + pty1;

                f[i][0] = (tempx + ptx1);
                f[i][1] = tempy;
            });

            return f;

        }

        private double GetXFromEventdate(DateTime EventDate)
        {
            return EventDate.Subtract(_tminDate).TotalDays * _coeffDate;
        }

        private double GetYFromLatitude(double Latitude)
        {
            return (_height - ((Latitude - _tminLat) * _coeffLat));
        }

        private double GetYFromLongitude(double Longitude)
        {
            return (_height - ((Longitude - _tminLng) * _coeffLng));
        }

        public StoryTable GetStoryTable()
        {
            StoryTable st = new StoryTable();

            float x1 = 0;
            float y1 = (float)_height;

            float x2 = (float)_width;
            float y2 = (float)_height;

            

            for (int i = 0; i < _dt.Rows.Count; i++)
            {
                float[] fLocation = new float[2];
                float[] fStory = new float[2];
                double[] oloc = new double[2];

                var ptx1 = x1;
                oloc[0] = (double)_dt.Rows[i][Properties.Settings.Default.StorygraphLatitudeName];
                var pty1 = GetYFromLatitude(oloc[0]);
                fLocation[0] = (float)pty1;

                var ptx2 = x2;
                oloc[1] = (double)_dt.Rows[i][Properties.Settings.Default.StorygraphLongitudeName];
                var pty2 = GetYFromLongitude(oloc[1]);
                fLocation[1] = (float)pty2;

                double m = (pty2 - pty1) / _width;

                DateTime odt = (DateTime)_dt.Rows[i][Properties.Settings.Default.StorygraphDateName];
                double tempx = GetXFromEventdate(odt);
                double tempy = m * tempx + pty1;

                fStory[0] = (float)(tempx + ptx1);
                fStory[1] = (float)tempy;

                st.Add(new Story(oloc, odt, fLocation, fStory, _dt.Rows[i][Properties.Settings.Default.StorygraphColorName].ToString(), _dt.Rows[i][Properties.Settings.Default.StorygraphLabelName].ToString()));
            };

            return st;
        }

        public TimeFreq GetTimeFrequencyTable()
        {
            var tbl = from x in _dt.AsEnumerable()
                              group x by x[Properties.Settings.Default.StorygraphDateName] into grp
                              select new
                              {
                                  time = grp.Key,
                                  frequency = grp.Count()
                              };

            var maxd = tbl.Max(r => r.frequency);
            var mind = tbl.Min(r => r.frequency);
            var diffd = maxd - mind;
                       
            
            double[][] d = new double[tbl.Count()][];
            int i=0;

            foreach(var v in tbl)
            {
                d[i] = new double[2];
                d[i][0] = GetXFromEventdate((DateTime)v.time);
                d[i][1] = _height*(v.frequency-mind)/diffd;
                i++;
            }

            return new TimeFreq(d, maxd, mind);
        }

        
    }
}
