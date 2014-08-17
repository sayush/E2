using System;
using D2D = Microsoft.WindowsAPICodePack.DirectX.Direct2D1;
using System.Data;
using System.Collections.Generic;
using Microsoft.WindowsAPICodePack.DirectX.Direct2D1;

namespace E2Charts
{
    internal sealed class D2DStorygraphScene: Direct2D.Scene
    {
        
        private const float RECTSIZE = 1.2f;
        private const float WINGLENGTH = 3f;
        private float div = 360.0f / (2 * 3.1416f);
        private Dictionary<string, D2D.SolidColorBrush> brushes;
        private StoryTable _st;
        private bool _showLocationLines = false;
        public D2D.SolidColorBrush neutralBrush { get; set; }

        public D2DStorygraphScene(StoryTable st, bool showLocationLines)
        {
            this._st = st;
            this._showLocationLines = showLocationLines;
            brushes = new Dictionary<string, D2D.SolidColorBrush>();
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
                //brushes.Add(key, this.RenderTarget.CreateSolidColorBrush(new D2D.ColorF(t[0], t[1], t[2], 0.6f)));
            }

            //neutralBrush = this.RenderTarget.CreateSolidColorBrush(new D2D.ColorF(1.0f, 0.95f, 0.95f, 0.3f));
            neutralBrush = this.RenderTarget.CreateSolidColorBrush(new D2D.ColorF(0.8f, 0.8f, 0.8f, 0.3f));
            base.OnCreateResources(); // Call this last to start the animation

            //this.RenderTarget.Dpi = new DpiF(640.0f*this.Factory.DesktopDpi.X/96.0f, 480.0f*this.Factory.DesktopDpi.Y/96.0f);
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
#if DEBUG
            DateTime t1 = DateTime.Now;
            Console.WriteLine("Rendering started at: " + t1.ToString());
#endif
            //this.RenderTarget.AntiAliasMode = D2D.AntiAliasMode.PerPrimitive;
            var size = this.RenderTarget.Size;
            float width = size.Width;
            float height = size.Height;

            this.RenderTarget.BeginDraw();
            this.RenderTarget.Clear(new D2D.ColorF(0, 0, 0, 0));

            DrawStories(width, height, _showLocationLines);
            
            this.RenderTarget.EndDraw();
#if DEBUG
            Console.WriteLine("Rendering ended at: " + DateTime.Now.ToString());
            Console.WriteLine("Total Time taken: " + DateTime.Now.Subtract(t1).ToString());
#endif
        }

        private void DrawStories(float width, float height, bool showLines){
            float[] t;

            if (showLines)
            {
                foreach (Story s in _st)
                {
                    t = s.GetLocationLines();
                    //this.RenderTarget.DrawRectangle(new D2D.RectF(0.0f, t[0], width, t[1]), neutralBrush, 1.0f, null);
                    this.RenderTarget.DrawLine(new D2D.Point2F(0.0f, t[0]), new D2D.Point2F(width, t[1]), neutralBrush, 0.8f);
                }
            }

            //D2D.SolidColorBrush xBrush = this.RenderTarget.CreateSolidColorBrush(new ColorF(0.8f,0.8f,0.8f,0.8f));

            foreach (Story s in _st)
            {
                t = s.GetEventLocation();
                D2D.RectF temprect  = new D2D.RectF(t[0] - RECTSIZE, t[1] - RECTSIZE, t[0] + RECTSIZE, t[1] + RECTSIZE);
                
                this.RenderTarget.FillRectangle(temprect, brushes[s.GetLabel()]);
                this.RenderTarget.DrawRectangle(temprect, brushes[s.GetLabel()], 0.5f);
                //drawUGlyph(WINGLENGTH, t[0], t[1], s.GetLocationLines()[0], s.GetLocationLines()[1], width, brushes[s.GetLabel()]);
            }
        }

        private void drawUGlyph(float wingLength, float x, float y, float lat, float lng, float width, Brush brush)
        {
            float rotationAngle = (float)Math.Atan((lng - lat) / width) * div;
            //rotationAngle += rotationAngle < 0 ? 90f : 45f;

            this.RenderTarget.Transform = Matrix3x2F.Rotation(rotationAngle, new D2D.Point2F(x, y));

            float hw = WINGLENGTH / 2;

            PathGeometry pLinesGeometry = this.Factory.CreatePathGeometry();
            GeometrySink gs = pLinesGeometry.Open();
            gs.SetFillMode(FillMode.Winding);

            gs.BeginFigure(new D2D.Point2F(x, y), FigureBegin.Filled);
            gs.AddLine(new D2D.Point2F(x + hw, y + hw));
            gs.AddLine(new D2D.Point2F(x + hw, y - wingLength));
            gs.AddLine(new D2D.Point2F(x, y));
            gs.AddLine(new D2D.Point2F(x - hw, y + hw));
            gs.AddLine(new D2D.Point2F(x - hw, y - wingLength));
            gs.EndFigure(FigureEnd.Closed);

            gs.Close();

            brush.Opacity = 0.2f;
            this.RenderTarget.FillGeometry(pLinesGeometry, brush);

            brush.Opacity = 0.8f;
            this.RenderTarget.DrawGeometry(pLinesGeometry, brush, 0.8f);
            //this.RenderTarget.DrawRectangle(new D2D.RectF(x - RECTSIZE, y - RECTSIZE, x + RECTSIZE, y + RECTSIZE), brush, 0.8f);

            this.RenderTarget.Transform = Matrix3x2F.Identity;
        }

        
    }
}
