using System;
using D2D = Microsoft.WindowsAPICodePack.DirectX.Direct2D1;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAPICodePack.DirectX.Direct2D1;

namespace E2.Graph
{
    internal sealed class D2DStorylinesScene: Direct2D.Scene
    {
        private const int RECTSIZE = 3;
        private Dictionary<string, D2D.SolidColorBrush> brushes;
        private D2D.SolidColorBrush neutralBrush;
        private StoryTable _st;
        private D2D.StrokeStyle sst;
        private D2D.StrokeStyle ust;
        private bool _showLocationLines = false;

        private float div = 360.0f / (2 * 3.1416f);
        private float WINGLENGTHX = 20f;
        private float WINGLENGTHY = 10f;

        private Random rnd;

        public D2DStorylinesScene(StoryTable st, bool showLocationLines)
        {
            this._st = st;
            this._showLocationLines = showLocationLines;
            brushes = new Dictionary<string, D2D.SolidColorBrush>();
            rnd = new Random();
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
            sst = Factory.CreateStrokeStyle(new D2D.StrokeStyleProperties() { DashStyle = D2D.DashStyle.DashDot });
            ust = Factory.CreateStrokeStyle(new D2D.StrokeStyleProperties() { DashStyle = D2D.DashStyle.Dash });
            foreach (string key in _st.GetColorDictionary().Keys)
            {
                t = _st.GetColorDictionary()[key];
                brushes.Add(key, this.RenderTarget.CreateSolidColorBrush(new D2D.ColorF(t[0], t[1], t[2], t[3])));
            }

            neutralBrush = this.RenderTarget.CreateSolidColorBrush(new D2D.ColorF(0.8f, 0.8f, 0.8f,0.5f));
            
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
            float[] pt, ct;
            
            
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
                
                drawSpatialUncertaintyGlyph(WINGLENGTHX, pt[0], pt[1], prev.GetLocationLines()[0], prev.GetLocationLines()[1], width, brushes[prev.GetLabel()]);
                Story s;
                
                int random;

                for (int i = 1; i < storyset.Count(); i++)
                {
                    storyenumerator.MoveNext();
                    s = storyenumerator.Current;
                    ct = s.GetEventLocation();
                    random = rnd.Next(100);
                    if (random < 25)
                        this.RenderTarget.DrawEllipse(new Ellipse(new Point2F(ct[0], ct[1]), RECTSIZE, RECTSIZE), brushes[s.GetLabel()], 2f);
                    else if (random < 50)
                        drawTemporalUncertaintyGlyph(rnd.Next(20), ct[0], ct[1], s.GetLocationLines()[0], s.GetLocationLines()[1], width, brushes[prev.GetLabel()]);
                    else if (random < 75)
                        drawSpatialUncertaintyGlyph(rnd.Next(20), ct[0], ct[1], s.GetLocationLines()[0], s.GetLocationLines()[1], width, brushes[prev.GetLabel()]);
                    else
                        drawSpatioTemporalUncertaintyGlyph(rnd.Next(20), ct[0], ct[1], s.GetLocationLines()[0], s.GetLocationLines()[1], width, brushes[prev.GetLabel()]);
                    
                    
                    //drawUGlyph(WINGLENGTH, ct[0], ct[1], s.GetLocationLines()[0], s.GetLocationLines()[1], width, brushes[prev.GetLabel()]);
                    //drawTemporalUncertaintyGlyph(WINGLENGTHX, ct[0], ct[1], s.GetLocationLines()[0], s.GetLocationLines()[1], width, brushes[prev.GetLabel()]);
                    //drawSpatialUncertaintyGlyph(WINGLENGTHX, ct[0], ct[1], s.GetLocationLines()[0], s.GetLocationLines()[1], width, brushes[prev.GetLabel()]);
                    //

                    brushes[s.GetLabel()].Opacity = 1.0f;
                    this.RenderTarget.DrawLine(new D2D.Point2F(pt[0], pt[1]), new D2D.Point2F(ct[0], ct[1]), brushes[s.GetLabel()], 1.0f, sst);

                    pt = ct;
                }
            }
        }

        private void drawTemporalUncertaintyGlyph(float wingLength, float x, float y, float lat, float lng, float width, Brush brush)
        {
            float rotationAngle = (float)Math.Atan((lng - lat) / width) * div;
            this.RenderTarget.Transform = Matrix3x2F.Rotation(rotationAngle, new D2D.Point2F(x, y));
            this.RenderTarget.DrawLine(new D2D.Point2F(x - wingLength - 5, y - 2), new D2D.Point2F(x + wingLength + 5, y - 2), brush, 1.5f);
            this.RenderTarget.DrawLine(new D2D.Point2F(x - wingLength - 5, y + 2), new D2D.Point2F(x + wingLength + 5, y + 2), brush, 1.5f);
            this.RenderTarget.Transform = Matrix3x2F.Identity;
        }

        private void drawSpatialUncertaintyGlyph(float wingLength, float x, float y, float lat, float lng, float width, Brush brush)
        {
            float DASHSIZE = 5f;

            float y1 = lat + (lng - lat) * (x - DASHSIZE) / width;
            float y2 = lat + (lng - lat) * (x + DASHSIZE) / width;


            this.RenderTarget.DrawLine(new D2D.Point2F(x - DASHSIZE, y1 + wingLength), new D2D.Point2F(x + DASHSIZE, y2 + wingLength), brush, 1.0f);
            this.RenderTarget.DrawLine(new D2D.Point2F(x, y - wingLength), new D2D.Point2F(x, y + wingLength), brush, 2.0f, ust);
            this.RenderTarget.DrawLine(new D2D.Point2F(x - DASHSIZE, y1 - wingLength), new D2D.Point2F(x + DASHSIZE, y2 - wingLength), brush, 1.0f);
        }

        private void drawSpatioTemporalUncertaintyGlyph(float wingLength, float x, float y, float lat, float lng, float width, Brush brush)
        {
            float y1 = lat + (lng - lat) * (x - wingLength) / width;
            float y2 = lat + (lng - lat) * (x + wingLength) / width;

            PathGeometry pLinesGeometry = this.Factory.CreatePathGeometry();
            GeometrySink gs = pLinesGeometry.Open();
            gs.SetFillMode(FillMode.Winding);
            gs.BeginFigure(new D2D.Point2F(x - wingLength, y1 + WINGLENGTHY), FigureBegin.Filled);
            gs.AddLine(new D2D.Point2F(x + wingLength, y2 + WINGLENGTHY));
            gs.AddLine(new D2D.Point2F(x + wingLength, y2 - WINGLENGTHY));
            gs.AddLine(new D2D.Point2F(x - wingLength, y1 - WINGLENGTHY));
            gs.EndFigure(FigureEnd.Closed);

            gs.Close();

            brush.Opacity = 0.5f;
            this.RenderTarget.FillGeometry(pLinesGeometry, brush);
            brush.Opacity = 1f;

            this.RenderTarget.DrawGeometry(pLinesGeometry, brush, 1.0f, ust);
        }

        private void drawUGlyph(float wingLength, float x, float y, float lat, float lng, float width, Brush brush)
        {
            float rotationAngle = (float)Math.Atan((lng - lat) / width) * div;
            //rotationAngle += rotationAngle < 0 ? 90f : 45f;

            this.RenderTarget.Transform = Matrix3x2F.Rotation(rotationAngle, new D2D.Point2F(x,y));

            float hw = WINGLENGTHX / 2;

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

            brush.Opacity = 0.5f;
            this.RenderTarget.FillGeometry(pLinesGeometry, brush);


            //brush.Opacity = 0.2f;
            //this.RenderTarget.FillGeometry(pLinesGeometry,brush);

            //brush.Opacity = 0.8f;
            //this.RenderTarget.DrawGeometry(pLinesGeometry, brush, 0.8f);
            //this.RenderTarget.DrawRectangle(new D2D.RectF(x - RECTSIZE, y - RECTSIZE, x + RECTSIZE, y + RECTSIZE), brush, 0.8f);
            
            this.RenderTarget.Transform = Matrix3x2F.Identity;
        }
    }
}
