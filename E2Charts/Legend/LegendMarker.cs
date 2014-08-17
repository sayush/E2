using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace E2Charts.Legend
{
    class LegendMarker : Shape
    {
        private Brush fillColor;

        public double MarkerHeight, MarkerWidth;

        public LegendMarker(double height, double width, Brush b)
        {
            MarkerHeight = height; MarkerWidth = width;
            fillColor = b;
            this.Stroke = Brushes.Black;
            this.Fill = fillColor;
        }

        public LegendMarker() { }

        protected override Geometry DefiningGeometry
        {
            get
            {
                // Create a StreamGeometry for describing the shape
                StreamGeometry geometry = new StreamGeometry();
                geometry.FillRule = FillRule.EvenOdd;

                using (StreamGeometryContext context = geometry.Open())
                {
                    InternalTriMarkerGeometry(context);
                }

                // Freeze the geometry for performance benefits
                geometry.Freeze();

                return geometry;
            }
        }

        private void InternalTriMarkerGeometry(StreamGeometryContext context)
        {
            Point p1 = new Point(0, 0);
            Point p2 = new Point(MarkerWidth, 0);
            Point p3 = new Point(MarkerWidth, MarkerHeight);
            Point p4 = new Point(0, MarkerHeight);

            context.BeginFigure(p1, true, true);
            context.LineTo(p2, true, true);
            context.LineTo(p3, true, true);
            context.LineTo(p4, true, true);
        }

    }
}
