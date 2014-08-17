using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace E2Charts
{
    public class StoryTable: IList<Story>
    {
        private List<Story> _list;
        private Dictionary<string, float[]> _ulocations;
        private Dictionary<string, float[]> _colstr;


        private const float COL = (float)255.0;
        
        public StoryTable()
        {
            _colstr = new Dictionary<string, float[]>();
            _ulocations = new Dictionary<string, float[]>();
            _list = new List<Story>();
        }

        public List<float[]> GetUniqueLocations()
        {
            return _ulocations.Values.ToList<float[]>();
        }

        public float GetMaxFrequency()
        {
            float f = 0;
            foreach (KeyValuePair<string, float[]> kvp in _ulocations)
            {
                f = kvp.Value[2] > f ? kvp.Value[2] : f;
            }
            return f;
        }

        public void Add(Story s)
        {
            float[] t;
            if (!_colstr.ContainsKey(s.GetLabel()))
            {
                _colstr.Add(s.GetLabel(), StrToHexColor(s.GetEventColor()));
            }
            _list.Add(s);

            t = new float[] { s.GetLocationLines()[0], s.GetLocationLines()[1], 1f };
            if (!_ulocations.Keys.Contains(s.GetLocationLineHash()))
            {
                _ulocations.Add(s.GetLocationLineHash(), t);
            }
            else
            {
                _ulocations[s.GetLocationLineHash()][2]++;
            }
        }

        public Dictionary<string, float[]> GetColorDictionary()
        {
            return _colstr;
        }

        public int IndexOf(Story item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(int index, Story item)
        {
            _list.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }

        public Story this[int index]
        {
            get
            {
                return _list[index];
            }
            set
            {
                _list[index] = value;
            }
        }


        public void Clear()
        {
            _list.Clear();
        }

        public bool Contains(Story item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(Story[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _list.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(Story item)
        {
            return _list.Remove(item);
        }

        public IEnumerator<Story> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        private float[] StrToHexColor(string xcolor)
        {
            float[] t = new float[4];
            byte[] floatvals = BitConverter.GetBytes(uint.Parse(xcolor, System.Globalization.NumberStyles.HexNumber));

            t[0] = Int32.Parse(xcolor.Substring(0, 2), System.Globalization.NumberStyles.HexNumber) / COL;
            t[1] = Int32.Parse(xcolor.Substring(2, 2), System.Globalization.NumberStyles.HexNumber) / COL;
            t[2] = Int32.Parse(xcolor.Substring(4, 2), System.Globalization.NumberStyles.HexNumber) / COL;
            t[3] = Int32.Parse(xcolor.Substring(6, 2), System.Globalization.NumberStyles.HexNumber) / COL;
            return t;
        }

        public float GetLocationCount(Story s)
        {
            return _ulocations[s.GetLocationLineHash()][2];
        }
    }

    public struct Story
    {
        float[] xlocation;
        float[] xevent;
        string xcolor;
        string xlabel;
        double[] olocation;
        DateTime odate;

        // float[] in locationLine means ready-to-plot ylat and ylng variables
        // float[] in eventLocation means ready-to-plot x,y coordinates
        public Story(double[] oloc, DateTime oDate, float[] locationLine, float[] eventLocation, string eventColor, string label) { odate = oDate;  olocation = oloc; xlocation = locationLine; xevent = eventLocation; xcolor = eventColor; xlabel = label; }

        // This returns the {ylat, ylng} values of a location line
        public float[] GetLocationLines() { return xlocation; }

        public string GetLocationLineHash() { return xlocation[0].ToString() + "," + xlocation[1].ToString(); }

        // This returns {x, y} values of event
        public float[] GetEventLocation() { return xevent; }

        // This returns the original location
        public double[] GetOriginalLocation() { return olocation; }
        // This returns the original time
        public DateTime GetDate() { return odate; }

        public string GetEventColor() { return xcolor; }
        public string GetLabel() { return xlabel; }

        
    }

    public struct LatLngColor
    {
        double lat, lng;
        string color;

        public double Latitude { get{ return lat;} }
        public double Longitude { get{return lng;} }
        public string Color { get{return color;} }

        public LatLngColor(double lat, double lng, string color) { this.lat = lat; this.lng = lng; this.color = color; }
    }

    public struct TimeFreq
    {
        double[][] tf;
        double maxf;
        double minf;

        public double MaxFrequency { get { return maxf; } }
        public double MinFrequency { get { return minf; } }
        public double[][] FrequencyTable { get { return tf; } }

        public TimeFreq(double[][] tf, double maxf, double minf) { this.tf = tf; this.maxf = maxf; this.minf = minf; }
    }

}
