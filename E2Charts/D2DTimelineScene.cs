using System;
using D2D = Microsoft.WindowsAPICodePack.DirectX.Direct2D1;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAPICodePack.DirectX.Direct2D1;

namespace E2Charts
{
    internal sealed class D2DTimelineScene: Direct2D.Scene
    {
        private const float RECTSIZE = 0.5f;
        private Dictionary<string, D2D.SolidColorBrush> brushes;
        private D2D.SolidColorBrush neutralBrush;
        private TimeFreq _st;

        public D2DTimelineScene(TimeFreq st, bool showLocationLines)
        {
            this._st = st;
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
            neutralBrush = this.RenderTarget.CreateSolidColorBrush(new D2D.ColorF(0.17f, 0.48f, 0.71f));
            
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

            DrawTimeline(width, height);

            this.RenderTarget.EndDraw();
#if DEBUG
            Console.WriteLine("Rendering ended at: " + DateTime.Now.ToString());
            Console.WriteLine("Total Time taken: " + DateTime.Now.Subtract(t1).ToString());
#endif
        }

        private void DrawTimeline(float width, float height)
        {
            foreach (double[] t in _st.FrequencyTable)
            {
                this.RenderTarget.DrawRectangle(new D2D.RectF((float)t[0] - RECTSIZE, height, (float)t[0] + RECTSIZE, height- (float)t[1]), neutralBrush, 0.8f);
            }

        }

    }
}
