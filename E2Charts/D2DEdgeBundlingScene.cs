using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.WindowsAPICodePack.DirectX.Direct2D1;
using D2D = Microsoft.WindowsAPICodePack.DirectX.Direct2D1;

namespace E2Charts
{
    internal sealed class D2DEdgeBundlingScene: Direct2D.Scene
    {
        private const float PI = 3.1416f;
        private const float RECTSIZE = 1.2f;
        private const float div = 360.0f / (2 * PI);

        private Dictionary<string, D2D.SolidColorBrush> brushes;
        private StoryTable _st;
        private bool _showLocationLines = false;
        private ForceParameters _f;

        public D2D.SolidColorBrush neutralBrush { get; set; }
        public D2D.SolidColorBrush alternativeBrush { get; set; }

        float[] _kp; // Array of stiffness constant for each location line
        float[,] _compatiblityMatrix;

        public D2DEdgeBundlingScene(StoryTable st, ForceParameters f, bool showLocationLines)
        {
            this._st = st;
            this._f = f;
            this._showLocationLines = showLocationLines;
            brushes = new Dictionary<string, D2D.SolidColorBrush>();
        }

        #region D2D Scene specifics
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        protected override void OnCreateResources()
        {
            float[] t;
            foreach (string key in _st.GetColorDictionary().Keys)
            {
                t = _st.GetColorDictionary()[key];
                brushes.Add(key, this.RenderTarget.CreateSolidColorBrush(new D2D.ColorF(t[0], t[1], t[2], t[3])));
            }

            neutralBrush = this.RenderTarget.CreateSolidColorBrush(new D2D.ColorF(0.8f, 0.0f, 0.0f, 0.3f));
            alternativeBrush = this.RenderTarget.CreateSolidColorBrush(new D2D.ColorF(0.8f, 0.8f, 0.8f, 0.3f));
            base.OnCreateResources();
        }

        protected override void OnFreeResources()
        {
            base.OnFreeResources();

            foreach (string key in brushes.Keys)
            {
                brushes[key].Dispose();
            }
            brushes.Clear();
        }

        protected override void OnRender()
        {
            var size = this.RenderTarget.Size;
            float width = size.Width;
            float height = size.Height;
#if DEBUG
            DateTime t1 = DateTime.Now;
            Console.WriteLine("Rendering started at: " + t1.ToString());
#endif
            float[, ,] pt = segmentLines(width, _f.Segments - _f.Segments%3);
            computeCompatibility(width);
#if DEBUG
            Console.WriteLine("Starting force calculation at: " + DateTime.Now.ToString());
#endif
            for (int i = 0; i < _f.Iterations; i++)
                calcForce(width, pt);
#if DEBUG
            Console.WriteLine("Completing force calculation at: " + DateTime.Now.ToString());
#endif
            this.RenderTarget.BeginDraw();
            this.RenderTarget.Clear(new D2D.ColorF(1, 1, 1));

            DrawFDEBBeziers2(width, height, pt);

            this.RenderTarget.EndDraw();
#if DEBUG
            Console.WriteLine("Rendering ended at: " + DateTime.Now.ToString());
            Console.WriteLine("Total Time taken: " + DateTime.Now.Subtract(t1).ToString());
#endif
        }
        #endregion

        private float[,,] segmentLines(float width, int segments)
        {
#if DEBUG
            Console.WriteLine("Starting Segmentation at: " + DateTime.Now.ToString());
#endif
            /// 3D array of floats containing - Locations, their segments, x,y coordinate of the segments
            float[, ,] r = new float[_st.GetUniqueLocations().Count, segments + 1, 2];
            /// maxFrequency is used later to compute the stiffness of a line i.e, more points it has, more stiff it is
            float maxFrequency = _st.GetMaxFrequency();
            /// Stiffness constant, kp
            _kp = new float[_st.GetUniqueLocations().Count];

            float ylat, ylng, slope, intercept, incx;
            
            for (int i = 0; i < _st.GetUniqueLocations().Count; i++)
            {
                ylat = _st.GetUniqueLocations()[i][0];
                ylng = _st.GetUniqueLocations()[i][1];

                slope = (ylng - ylat) / width;
                intercept = ylat;
                incx = width / segments;

                for (int j = 0; j <= segments; j++)
                {
                    r[i, j, 0] = incx * j;
                    r[i, j, 1] = (slope * r[i, j, 0]) + intercept;
                }
                //_kp[i] = (_st.GetUniqueLocations()[i][2] / maxFrequency);// < 0.5f ? 0.3f : (t / maxFrequency);
                _kp[i] = _st.GetUniqueLocations()[i][2];
            }
#if DEBUG
            Console.WriteLine("Completing Segmentation at: " + DateTime.Now.ToString());
#endif
            return r;
        }

        private void computeCompatibility(float width)
        {
#if DEBUG
            Console.WriteLine("Computing Compatibility Matrix at: " + DateTime.Now.ToString());
#endif
            /// GetUniqueLocations() [ylat, ylng, frequency]
            _compatiblityMatrix = new float[_st.GetUniqueLocations().Count, _st.GetUniqueLocations().Count];
            float compatiblityAngle = PI/30;
            float compatibilityDistance = 50f/width;

            for (int i = 0; i < _st.GetUniqueLocations().Count; i++)
            {
                float m1 = (_st.GetUniqueLocations()[i][1] - _st.GetUniqueLocations()[i][0]) / width;
                for (int j = 0; j < _st.GetUniqueLocations().Count; j++)
                {
                    if (i != j)
                    {
                        float m2 = (_st.GetUniqueLocations()[j][1] - _st.GetUniqueLocations()[j][0]) / width;
                        float angle = (float)Math.Atan((m1-m2)/(1+m1*m2));
                        float distance = Math.Abs(_st.GetUniqueLocations()[i][0] - _st.GetUniqueLocations()[j][0]) + Math.Abs(_st.GetUniqueLocations()[i][1] - _st.GetUniqueLocations()[j][1]);
                        distance /= width;
                        if (angle < compatiblityAngle && distance < compatibilityDistance) _compatiblityMatrix[i, j] = 1;
                        else _compatiblityMatrix[i, j] = 0;

                    }
                }
            }
#if DEBUG
            Console.WriteLine("Completing Compatibility Matrix  at: " + DateTime.Now.ToString());
#endif
        }

        private void calcForce(float width, float[,,] _pt)
        {
            /// float[,,] _pt = 3D array of floats containing - Locations, their segments, x,y coordinate of the segments
            /// Algorithm:
            ///     For location i,
            ///         Get the coordinates jth segment
            ///         Compute its interactions with all other k nodes where i != k
            ///         Update position

            float force;
            float invwidth = 1 / width;

            for (int i = 0; i < _pt.GetLength(0); i++)
            {
                for (int j = 1; j < _pt.GetLength(1) - 1; j++)
                {
                    for (int k = 0; k < _pt.GetLength(0); k++)
                    {
                        /// This is where we calculate the VERTICAL pulls
                        if (i != k && _compatiblityMatrix[i,k] == 1)
                        {
                            /// Safety condition since if t2 -> 0 then the 1/t will spike to infinity
                            /// t2 > N is to increase the width of bundle at that point - this needs revision
                            force = (_pt[i, j, 1] - _pt[k, j, 1]);
                            //Console.WriteLine("i:" + i + " ,k:" + k + " ,force:"+force);
                            if (Math.Abs(force) > 1f) 
                            {
                                _pt[i, j, 1] += (-1f / (force * _kp[i]));
                            }
                        }
                    }

                    /// This part is kp . (||pi-1 - pi|| + ||pi - pi+1||) - meaning the HORIZONTAL pulls
                    _pt[i, j, 1] += (_pt[i, j - 1, 1] - 2 * _pt[i, j, 1] + _pt[i, j + 1, 1]) / 1000f;

                    /// So finally we get VERTICAL pulls + HORIZONTAL pulls
                }
            }
        }

        private void DrawFDEBBeziers2(float width, float height, float[,,] _pt)
        {
            List<tempEvents> tes = new List<tempEvents>();
            float[] t;

            //foreach (Story s in _st)
            //{
            //    t = s.GetLocationLines();
            //    //this.RenderTarget.DrawRectangle(new D2D.RectF(0.0f, t[0], width, t[1]), neutralBrush, 1.0f, null);
            //    this.RenderTarget.DrawLine(new D2D.Point2F(0.0f, t[0]), new D2D.Point2F(width, t[1]), alternativeBrush, 0.8f);
            //}
            
            for (int i = 0; i < _pt.GetLength(0); i++)
            {
                PathGeometry pLinesGeometry = this.Factory.CreatePathGeometry();
                GeometrySink gs = pLinesGeometry.Open();
                gs.SetFillMode(FillMode.Winding);

                gs.BeginFigure(new D2D.Point2F(_pt[i, 0, 0], _pt[i, 0, 1]), FigureBegin.Hollow);

                for (int j = 1; j < _pt.GetLength(1); j += 3)
                    gs.AddBezier(
                        new BezierSegment(
                            new Point2F(_pt[i, j, 0], _pt[i, j, 1]),
                            new Point2F(_pt[i, j + 1, 0], _pt[i, j + 1, 1]),
                            new Point2F(_pt[i, j + 2, 0], _pt[i, j + 2, 1])
                        )
                    );


                gs.EndFigure(FigureEnd.Open);
                gs.Close();
                neutralBrush.Opacity = 0.8f + _kp[i];
                if(_showLocationLines)
                    this.RenderTarget.DrawGeometry(pLinesGeometry, neutralBrush, 0.8f);

                foreach (Story s in _st)
                {
                    t = s.GetLocationLines();
                    if ((int)_pt[i, 0, 1] == (int)t[0] && (int)_pt[i, (int)(_pt.GetLength(1) - 1), 1] == (int)t[1])
                    {
                        PointAndTangent pt = pLinesGeometry.ComputePointAtLength((pLinesGeometry.ComputeLength() * s.GetEventLocation()[0]) / width);
                        RectangleGeometry pEventsGeometry = this.Factory.CreateRectangleGeometry(
                            new D2D.RectF(pt.Point.X - RECTSIZE, pt.Point.Y - RECTSIZE, pt.Point.X + RECTSIZE, pt.Point.Y + RECTSIZE)
                            );
                        tes.Add(new tempEvents(pEventsGeometry, brushes[s.GetLabel()], 0.8f));
                        
                    }
                }
            }
            //foreach (tempEvents te in tes)
            //    this.RenderTarget.DrawGeometry(te.pEventsGeometry, te.solidColorBrush, te.p);

        }
    }
}
