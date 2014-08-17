using System;
using D2D = Microsoft.WindowsAPICodePack.DirectX.Direct2D1;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAPICodePack.DirectX.Direct2D1;

namespace E2.Graph
{
    internal sealed class D2DStorylinesUScene: Direct2D.Scene
    {
        private const int RECTSIZE = 4;
        private Dictionary<string, D2D.SolidColorBrush> brushes;
        private Dictionary<string, D2D.SolidColorBrush> halobrushes;
        private D2D.SolidColorBrush neutralBrush, regionBrush;
        private StoryTable _st;
        private D2D.StrokeStyle sst;
        private bool _showLocationLines = false;
        
        private const float VELOCITY = 2;
        private const float CUTOFF = 200;

        private float div = 360.0f / (2 * 3.1416f);
        private float WINGLENGTH = 32f;

        public D2DStorylinesUScene(StoryTable st, bool showLocationLines)
        {
            this._st = st;
            this._showLocationLines = showLocationLines;
            brushes = new Dictionary<string, D2D.SolidColorBrush>();
            halobrushes = new Dictionary<string, D2D.SolidColorBrush>();
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
            sst = Factory.CreateStrokeStyle(new D2D.StrokeStyleProperties() { DashStyle = D2D.DashStyle.Dash });
            foreach (string key in _st.GetColorDictionary().Keys)
            {
                t = _st.GetColorDictionary()[key];
                brushes.Add(key, this.RenderTarget.CreateSolidColorBrush(new D2D.ColorF(t[0], t[1], t[2], t[3])));
                if(!halobrushes.ContainsKey(key))
                    halobrushes.Add(key, this.RenderTarget.CreateSolidColorBrush(new D2D.ColorF(t[0], t[1], t[2], 0.2f)));
            }

            neutralBrush = this.RenderTarget.CreateSolidColorBrush(new D2D.ColorF(0.8f, 0.8f, 0.8f));
            regionBrush = this.RenderTarget.CreateSolidColorBrush(new D2D.ColorF(1.0f, 0.8f, 0.8f,0.3f));
            
            //base.OnCreateResources(); // Call this last to start the animation
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

            this.RenderTarget.BeginDraw();
            this.RenderTarget.Clear(new D2D.ColorF(0, 0, 0, 0));

            DrawStories(width, height, _showLocationLines);

            this.RenderTarget.EndDraw();
        }


        private void DrawStories(float width, float height, bool showLines){
            float[] pt, ct, ptl, ctl;
            float w = this.RenderTarget.Size.Width;

            if (showLines)
            {
                PathGeometry pLinesGeometry = this.Factory.CreatePathGeometry();
                GeometrySink gs = pLinesGeometry.Open();
                gs.SetFillMode(FillMode.Winding);

                foreach (Story s in _st)
                {
                    ct = s.GetLocationLines();
                    gs.BeginFigure(new D2D.Point2F(0.0f, ct[0]), FigureBegin.Hollow);
                    gs.AddLine(new D2D.Point2F(width, ct[1]));
                    gs.EndFigure(FigureEnd.Open);
                    //this.RenderTarget.DrawLine(new D2D.Point2F(0.0f, ct[0]), new D2D.Point2F(width, ct[1]), neutralBrush, 0.8f);
                }

                gs.Close();
                this.RenderTarget.DrawGeometry(pLinesGeometry, neutralBrush, 0.8f);
            }

            foreach (KeyValuePair<string, D2D.SolidColorBrush> kvp in brushes)
            {
                var storyset = from r in _st where r.GetLabel().Equals(kvp.Key) orderby r.GetEventLocation()[0] ascending select r;
                var storyenumerator = storyset.GetEnumerator();

                storyenumerator.MoveNext();
                Story prev = storyenumerator.Current;
                pt = prev.GetEventLocation();
                ptl = prev.GetLocationLines();

                this.RenderTarget.DrawRectangle(new D2D.RectF(pt[0] - RECTSIZE, pt[1] - RECTSIZE, pt[0] + RECTSIZE, pt[1] + RECTSIZE), brushes[prev.GetLabel()], 0.8f);
                //drawUGlyph(WINGLENGTH, pt[0], pt[1], prev.GetLocationLines()[0], prev.GetLocationLines()[1], width, brushes[prev.GetLabel()]);

                Story s;

                for (int i = 1; i < storyset.Count(); i++)
                {
                    storyenumerator.MoveNext();
                    s = storyenumerator.Current;
                    ct = s.GetEventLocation();
                    ctl = s.GetLocationLines();


                    if (Math.Abs(ct[0] - pt[0]) + Math.Abs(ct[1] - pt[1]) < CUTOFF)
                    {

                        // vt is symmetrical
                        float vt = VELOCITY * (ct[0] - pt[0]);

                        GeometrySink gs1, gs2, gsr;
                        PathGeometry pg1, pg2, pgr;

                        pg1 = pg2 = null;
                        gsr = gs1 = gs2 = null;

                        float yr1 = getYForUncertaintyBound(ptl[0] + vt, ptl[1] + vt, ct[0], w);
                        float yr2 = getYForUncertaintyBound(ptl[0] - vt, ptl[1] - vt, ct[0], w);

                        pg1 = Factory.CreatePathGeometry();
                        gs1 = pg1.Open();
                        gs1.BeginFigure(new D2D.Point2F(pt[0], pt[1]), FigureBegin.Filled);
                        gs1.AddLine(new D2D.Point2F(ct[0], yr1));
                        gs1.AddLine(new D2D.Point2F(ct[0], yr2));
                        gs1.EndFigure(FigureEnd.Closed);
                        gs1.Close();

                        yr1 = getYForUncertaintyBound(ctl[0] + vt, ctl[1] + vt, pt[0], w);
                        yr2 = getYForUncertaintyBound(ctl[0] - vt, ctl[1] - vt, pt[0], w);

                        pg2 = Factory.CreatePathGeometry();
                        gs2 = pg2.Open();
                        gs2.BeginFigure(new D2D.Point2F(ct[0], ct[1]), FigureBegin.Filled);
                        gs2.AddLine(new D2D.Point2F(pt[0], yr1));
                        gs2.AddLine(new D2D.Point2F(pt[0], yr2));
                        gs2.EndFigure(FigureEnd.Closed);
                        gs2.Close();

                        pgr = Factory.CreatePathGeometry();
                        gsr = pgr.Open();
                        pg1.CombineWithGeometry(pg2, CombineMode.Intersect, gsr);
                        gsr.Close();

                        this.RenderTarget.DrawGeometry(pgr, halobrushes[s.GetLabel()], 1.0f);
                        this.RenderTarget.FillGeometry(pgr, halobrushes[s.GetLabel()]);
                    }

                    //// for the forward cones
                    //float yr1 = getYForUncertaintyBound(ptl[0] + vt, ptl[1] + vt, ct[0], w);
                    //float yr2 = getYForUncertaintyBound(ptl[0] - vt, ptl[1] - vt, ct[0], w);
                    //this.RenderTarget.DrawLine(new D2D.Point2F(pt[0], pt[1]), new D2D.Point2F(ct[0], yr1), regionBrush, 2.0f);
                    //this.RenderTarget.DrawLine(new D2D.Point2F(pt[0], pt[1]), new D2D.Point2F(ct[0], yr2), regionBrush, 2.0f);

                    //// for the backward cones
                    //yr1 = getYForUncertaintyBound(ctl[0] + vt, ctl[1] + vt, pt[0], w);
                    //yr2 = getYForUncertaintyBound(ctl[0] - vt, ctl[1] - vt, pt[0], w);
                    //this.RenderTarget.DrawLine(new D2D.Point2F(ct[0], ct[1]), new D2D.Point2F(pt[0], yr1), regionBrush, 2.0f);
                    //this.RenderTarget.DrawLine(new D2D.Point2F(ct[0], ct[1]), new D2D.Point2F(pt[0], yr2), regionBrush, 2.0f);
                    
                    
                    
                    D2D.RectF temprect = new D2D.RectF(ct[0] - RECTSIZE, ct[1] - RECTSIZE, ct[0] + RECTSIZE, ct[1] + RECTSIZE);
                    this.RenderTarget.FillRectangle(temprect, brushes[s.GetLabel()]);
                    this.RenderTarget.DrawRectangle(temprect, brushes[s.GetLabel()], 0.5f);

                    //this.RenderTarget.DrawRectangle(new D2D.RectF(ct[0] - RECTSIZE, ct[1] - RECTSIZE, ct[0] + RECTSIZE, ct[1] + RECTSIZE), brushes[s.GetLabel()], 0.8f);
                    //drawUGlyph(WINGLENGTH, ct[0], ct[1], s.GetLocationLines()[0], s.GetLocationLines()[1], width, brushes[prev.GetLabel()]);
                    this.RenderTarget.DrawLine(new D2D.Point2F(pt[0], pt[1]), new D2D.Point2F(ct[0], ct[1]), brushes[s.GetLabel()], 2.0f, sst);

                    pt = ct;
                    ptl = ctl;
                }
            }
        }

        // lat, lng of the location line, time of the subsequent point, width of the drawing
        private float getYForUncertaintyBound(float a, float b, float x, float w)
        {
            return (x/w) * (b - a) + a;
        }

        private void drawUGlyph(float wingLength, float x, float y, float lat, float lng, float width, Brush brush)
        {
            float rotationAngle = (float)Math.Atan((lng - lat) / width) * div;
            //rotationAngle += rotationAngle < 0 ? 90f : 45f;

            this.RenderTarget.Transform = Matrix3x2F.Rotation(rotationAngle, new D2D.Point2F(x,y));

            float hw = WINGLENGTH / 2;

            PathGeometry pLinesGeometry = this.Factory.CreatePathGeometry();
            GeometrySink gs = pLinesGeometry.Open();
            gs.SetFillMode(FillMode.Winding);

            gs.BeginFigure(new D2D.Point2F(x,y), FigureBegin.Filled);
            gs.AddLine(new D2D.Point2F(x + hw, y + hw));
            gs.AddLine(new D2D.Point2F(x + hw, y - wingLength));
                gs.AddLine(new D2D.Point2F(x, y));
                gs.AddLine(new D2D.Point2F(x - hw, y + hw));
                gs.AddLine(new D2D.Point2F(x - hw, y - wingLength));
            gs.EndFigure(FigureEnd.Closed);

            gs.Close();

            brush.Opacity = 0.2f;
            this.RenderTarget.FillGeometry(pLinesGeometry,brush);

            brush.Opacity = 0.8f;
            this.RenderTarget.DrawGeometry(pLinesGeometry, brush, 0.8f);
            //this.RenderTarget.DrawRectangle(new D2D.RectF(x - RECTSIZE, y - RECTSIZE, x + RECTSIZE, y + RECTSIZE), brush, 0.8f);
            
            this.RenderTarget.Transform = Matrix3x2F.Identity;
        }
    }
}
