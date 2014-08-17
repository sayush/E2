using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace E2Charts.Legend
{
    class LegendContent : Grid
    {
        private const int MARKERDIM = 15;
        private const double FONTPADDING = 5;
        private const int MARKERPADDING = 5;
        private const double FONTSIZEY = 12;

        private readonly Dictionary<string, Brush> _brushes;
        

        public LegendContent(StoryTable st)
        {
            this._brushes = new Dictionary<string, Brush>();
            foreach (Story s in st)
            {
                if (!_brushes.ContainsKey(s.GetLabel()))
                {
                    _brushes.Add(s.GetLabel(), new SolidColorBrush((Color)ColorConverter.ConvertFromString("#" + s.GetEventColor().Substring(0, 6))));
                }
            }
        }

        public void Draw()
        {
            int i=0;
            foreach (KeyValuePair<string, Brush> kv in _brushes)
            {
                this.RowDefinitions.Add(new RowDefinition() { Height = new System.Windows.GridLength(30) });
            }
            this.ColumnDefinitions.Add(new ColumnDefinition() { Width = new System.Windows.GridLength(30) });
            this.ColumnDefinitions.Add(new ColumnDefinition() { Width = System.Windows.GridLength.Auto });



            foreach (KeyValuePair<string, Brush> kv in _brushes)
            {
                LegendMarker lm = new LegendMarker(MARKERDIM, MARKERDIM, kv.Value) { Margin  = new System.Windows.Thickness(7.5, 5, 7.5, 5) };
                Grid.SetRow(lm, i);
                Grid.SetColumn(lm, 0);
                this.Children.Add(lm);

                Label lb = new Label() { Content = kv.Key, FontSize=FONTSIZEY };
                Grid.SetRow(lb, i);
                Grid.SetColumn(lb, 1);
                this.Children.Add(lb);

                i++;
            }
        }
    }
}
