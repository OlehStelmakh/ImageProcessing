using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DocumentFormat.OpenXml.Drawing;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace WpfApp1
{
    class HistogramConstructor
    {
        public static Bitmap bitmap { set; get; }
        public HistogramConstructor()
        {
            if (bitmap!=null)
            {
                createPlot();
            }
            else
            {
                MessageBox.Show("Please select an image!");
                
            }
        }

        public void createPlot()
        {
            int[] brightness = new int[256];
            for (int i = 0; i < bitmap.Height; i++)
            {
                for (int j = 0; j < bitmap.Width; j++)
                {
                    Color pixel = bitmap.GetPixel(j, i);
                    int bright = Convert.ToInt32(pixel.R * 0.3 + pixel.G * 0.59 + pixel.B * 0.11);
                    brightness[bright] += 1;
                }
            }

            this.Items = new Collection<Item>();
            for (int i = 0; i < 256; i++)
            {
                if (Math.Abs(i % 8) < 0.01)
                {
                    Items.Add(new Item { Label = $"{i}", Value = brightness[i] });
                }
                else
                {
                    Items.Add(new Item { Label = String.Empty, Value = brightness[i] });
                }

            }

            var model = new PlotModel { Title = "Column Series", LegendPlacement = LegendPlacement.Outside, LegendPosition = LegendPosition.RightTop, LegendOrientation = LegendOrientation.Vertical };

            model.Axes.Add(new CategoryAxis { Position = AxisPosition.Bottom, ItemsSource = this.Items, LabelField = "Label" });
            model.Axes.Add(new LinearAxis { Position = AxisPosition.Left, MinimumPadding = 0, AbsoluteMinimum = 0 });
            model.Series.Add(new ColumnSeries { Title = "Brightness", ItemsSource = this.Items, ValueField = "Value", FillColor= OxyColor.FromArgb(255,64,64,64) });

            this.MyModel = model;
        }
        public Collection<Item> Items { get; set; }
        public PlotModel MyModel { get; set; }
    }

    
    public class Item
    {
        public string Label { get; set; }
        public int Value { get; set; }
    }
}


