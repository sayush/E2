#define DEBUG

using System;
using D2D = Microsoft.WindowsAPICodePack.DirectX.Direct2D1;
using System.Data;
using System.Collections.Generic;
using Microsoft.WindowsAPICodePack.DirectX.Direct2D1;
using Microsoft.WindowsAPICodePack.DirectX.Graphics;
using System.Threading.Tasks;


namespace E2.Graph
{

    class tempEvents
    {
        public RectangleGeometry pEventsGeometry;
        public SolidColorBrush solidColorBrush;
        public float p;

        public tempEvents(RectangleGeometry pEventsGeometry, SolidColorBrush solidColorBrush, float p)
        {
            // TODO: Complete member initialization
            this.pEventsGeometry = pEventsGeometry;
            this.solidColorBrush = solidColorBrush;
            this.p = p;
        }
    }

    internal sealed class D2DStorygraphFDEBScene: Direct2D.Scene
    {
        private const float K = 100f;
        private float[] _kp;

        private float[] _normalizedFrequency;

        private const float RECTSIZE = 0.5f;
        private Dictionary<string, D2D.SolidColorBrush> brushes;
        private StoryTable _st;

        private float[,,] _pt; // for [locations, segments, coordinates]
        private bool _showLocationLines = false;
        public D2D.SolidColorBrush neutralBrush { get; set; }
        ForceParameters _f;

        float maxFrequency;

        List<tempEvents> te;

        public D2DStorygraphFDEBScene(StoryTable st, ForceParameters f, bool showLocationLines)
        {
            this._st = st;
            this._showLocationLines = showLocationLines;
            brushes = new Dictionary<string, D2D.SolidColorBrush>();
            this._f = f;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        protected override void OnCreateResources()
        {
            // We don't need to free any resources because the base class will
            // call OnFreeResources if necessary before calling this method.
            //this.redBrush = this.RenderTarget.CreateSolidColorBrush(new D2D.ColorF(1, 0, 0));
            float[] t;
            foreach (string key in _st.GetColorDictionary().Keys)
            {
                t = _st.GetColorDictionary()[key];
                brushes.Add(key, this.RenderTarget.CreateSolidColorBrush(new D2D.ColorF(t[0], t[1], t[2], t[3])));
            }

            neutralBrush = this.RenderTarget.CreateSolidColorBrush(new D2D.ColorF(0.8f, 0.8f, 0.8f, 0.5f));
            //neutralBrush = this.RenderTarget.CreateSolidColorBrush(new D2D.ColorF(0.95f, 0.95f, 0.95f, 0.5f));
            //neutralBrush = this.RenderTarget.CreateSolidColorBrush(new D2D.ColorF(0.7f, 0.7f, 0.7f));
            //neutralBrush = this.RenderTarget.CreateSolidColorBrush(new D2D.ColorF(1.0f, 0.2f, 0.2f, 0.5f));
            base.OnCreateResources(); // Call this last to start the animation
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
            preprocess(width, _f.Segments, _f.Iterations);

            this.RenderTarget.BeginDraw();
            this.RenderTarget.Clear(new D2D.ColorF(1, 1, 1));

            DrawFDEBBeziers2(width, height);
            DrawEvents();
            
            this.RenderTarget.EndDraw();
#if DEBUG
            Console.WriteLine("Rendering ended at: " + DateTime.Now.ToString());
            Console.WriteLine("Total Time taken: " + DateTime.Now.Subtract(t1).ToString());
#endif
        }

        private PathGeometry DrawLines(float width, float height)
        {
            PathGeometry pLinesGeometry = this.Factory.CreatePathGeometry();

            GeometrySink gs = pLinesGeometry.Open();
            gs.SetFillMode(FillMode.Winding);

            for (int i = 0, j = _pt.GetLength(1); i < _pt.GetLength(0); i++)
            {
                gs.BeginFigure(new D2D.Point2F(_pt[i, 0, 0], _pt[i, 0, 1]), FigureBegin.Hollow);
                gs.AddLine(new Point2F(_pt[i, (int)(j - 1), 0], _pt[i, (int)(j - 1), 1]));
                gs.EndFigure(FigureEnd.Open);
            }
            gs.Close();

            return pLinesGeometry;
        }

        private void DrawFDEBLines(float width, float height)
        {
            PathGeometry pLinesGeometry = this.Factory.CreatePathGeometry();

            GeometrySink gs = pLinesGeometry.Open();
            gs.SetFillMode(FillMode.Winding);

            for (int i = 0; i < _pt.GetLength(0); i++)
            {
                gs.BeginFigure(new D2D.Point2F(_pt[i, 0, 0], _pt[i, 0, 1]), FigureBegin.Hollow);
                for (int j = 1; j < _pt.GetLength(1); j++)
                    gs.AddLine(new Point2F(_pt[i, j, 0], _pt[i, j, 1]));
                gs.EndFigure(FigureEnd.Open);
            }
            gs.Close();

            this.RenderTarget.DrawGeometry(pLinesGeometry, neutralBrush, 1f);
        }

        private void DrawFDEBBeziers(float width, float height)
        {
            float[] t;
            for (int i = 0, j = _pt.GetLength(1); i < _pt.GetLength(0); i++)
            {
                PathGeometry pLinesGeometry = this.Factory.CreatePathGeometry();
                GeometrySink gs = pLinesGeometry.Open();
                gs.SetFillMode(FillMode.Winding);

                gs.BeginFigure(new D2D.Point2F(_pt[i, 0, 0], _pt[i, 0, 1]), FigureBegin.Hollow);
                gs.AddBezier(
                    new BezierSegment(
                        new Point2F(_pt[i, (int)((j - 1) * 0.33), 0], _pt[i, (int)((j - 1) * .33), 1]),
                        new Point2F(_pt[i, (int)((j - 1) * 0.66), 0], _pt[i, (int)((j - 1) * .66), 1]),
                        new Point2F(_pt[i, (int)(j - 1), 0], _pt[i, (int)(j - 1), 1])
                    )
                );
                gs.EndFigure(FigureEnd.Open);
                gs.Close();
                this.RenderTarget.DrawGeometry(pLinesGeometry, neutralBrush, 1f);

                foreach(Story s in _st){
                    t = s.GetLocationLines();
                    if ((int)_pt[i, 0, 1] == (int)t[0] && (int)_pt[i, (int)(j - 1), 1] == (int)t[1])
                    {
                        PointAndTangent pt = pLinesGeometry.ComputePointAtLength(s.GetEventLocation()[0]);
                        RectangleGeometry pEventsGeometry = this.Factory.CreateRectangleGeometry(
                            new D2D.RectF(pt.Point.X - RECTSIZE, pt.Point.Y - RECTSIZE, pt.Point.X + RECTSIZE, pt.Point.Y + RECTSIZE)
                            );
                        this.RenderTarget.DrawGeometry(pEventsGeometry, brushes[s.GetLabel()], 0.8f);
                    }
                }
            }
        }

        private void DrawFDEBBeziers2(float width, float height)
        {
            te = new List<tempEvents>();
            float[] t;
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
                            new Point2F(_pt[i, j+1, 0], _pt[i, j+1, 1]),
                            new Point2F(_pt[i, j+2, 0],_pt[i, j+2, 1])
                        )
                    );


                gs.EndFigure(FigureEnd.Open);
                gs.Close();
                neutralBrush.Opacity = 0.8f+_normalizedFrequency[i];
                this.RenderTarget.DrawGeometry(pLinesGeometry, neutralBrush, 0.8f );

                foreach (Story s in _st)
                {
                    t = s.GetLocationLines();
                    if ((int)_pt[i, 0, 1] == (int)t[0] && (int)_pt[i, (int)(_pt.GetLength(1) - 1), 1] == (int)t[1])
                    {
                        PointAndTangent pt = pLinesGeometry.ComputePointAtLength((pLinesGeometry.ComputeLength() * s.GetEventLocation()[0]) / width);
                        RectangleGeometry pEventsGeometry = this.Factory.CreateRectangleGeometry(
                            new D2D.RectF(pt.Point.X - RECTSIZE, pt.Point.Y - RECTSIZE, pt.Point.X + RECTSIZE, pt.Point.Y + RECTSIZE)
                            );
                        te.Add(new tempEvents(pEventsGeometry, brushes[s.GetLabel()], 0.8f));
                    }
                }
            }
        }

        private void DrawEvents()
        {
            foreach(tempEvents t in te)
                this.RenderTarget.DrawGeometry(t.pEventsGeometry, t.solidColorBrush, t.p);
        }


        private void preprocess(float width, int segments, int iterations)
        {
#if DEBUG
            Console.WriteLine("Preprocess stage reached at: " + DateTime.Now.ToString());
#endif
            _pt = segmentLine(width, segments - segments%3);
#if DEBUG
            Console.WriteLine("Segmentation completed at: " + DateTime.Now.ToString());
#endif
            for(int i=0;i<iterations;i++)
                calcForce(width);
#if DEBUG
            Console.WriteLine("Force calculation completed at: " + DateTime.Now.ToString());
#endif
        }

        private float[,,] segmentLine(float width, int segments)
        {
#if DEBUG
            Console.WriteLine("Unique Locations: " + _st.GetUniqueLocations().Count);
#endif
            float[,,] r = new float[_st.GetUniqueLocations().Count, segments+1, 2];
            _normalizedFrequency = new float[_st.GetUniqueLocations().Count];
            float maxFrequency = _st.GetMaxFrequency();

            _kp = new float[_st.GetUniqueLocations().Count];
            
            float ylat, ylng, slope, intercept, incx;

            //float kp = (float)(K / (Math.Pow(Math.Pow(ylng - ylat, 2) + width * width, 0.5)));

            Parallel.For(0, _st.GetUniqueLocations().Count, (i) =>
            //for (int i = 0; i < _st.GetUniqueLocations().Count; i++)
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

                float t = _st.GetUniqueLocations()[i][2];
                _normalizedFrequency[i] = (t / maxFrequency);// < 0.5f ? 0.3f : (t / maxFrequency);
                // Using rectilinear metric
                _kp[i] = t;// / Math.Abs(ylng - ylat+ width); //(float)( t / (Math.Abs(ylng - ylat) ));
                //_kp[i] = (float)(_st.GetUniqueLocations()[i][2] / ((ylng - ylat) + width));
            }
            );

            

            return r;
        }

        float calcCa(float ylat1, float ylng1, float ylat2, float ylng2, float width)
        {
            float v1y = ylng1 - ylat1;
            float v2y = ylng2 - ylat2;
            float w2 = width * width;
            
            float nominator = (w2 + v1y * v2y);
            float denominator1 = (float)(Math.Pow(w2 + Math.Pow(v1y, 2), 0.5) * Math.Pow(w2 + Math.Pow(v2y, 2), 0.5));
            float denominator2 = (float)(w2 + Math.Abs(v1y)) * (w2 + Math.Abs(v2y));

            float squared = nominator/denominator1;
            float rectilinear = nominator / denominator2;

            //return squared;
            return .99f;
        }
            // Using rectilinear metric
            //


        // The main function to calculate the force - the one contributing to O(n^3)
        void calcForce(float width)
        {
            float t;
            
            //Parallel.For(0, _pt.GetLength(0), i =>
            for (int i=0; i < _pt.GetLength(0); i++)
            {
                for (int j = 1; j < _pt.GetLength(1) - 1; j++)
                {
                    //Parallel.For(0, _pt.GetLength(0), k =>
                    for (int k = 0; k < _pt.GetLength(0); k++)
                    {
                        if (i != k)
                        {
                            t = -1f / (_pt[i, j, 1] - _pt[k, j, 1]);
                            if (Math.Abs(t) < 1.0)
                            {
                                _pt[i, j, 1] += t;// * calcCa(_pt[i, j, 1], _pt[i, 0, 1], _pt[k, j, 1], _pt[k, 0, 1], width));
                            }
                        }
                    }
                    //);

                    // This part is kp . (||pi-1 - pi|| + ||pi - pi+1||)
                    _pt[i, j, 1] += (_pt[i, j - 1, 1] - 2 * _pt[i, j, 1] + _pt[i, j + 1, 1]) /1000f;
                }
            }
            //);
        }
    }
}
