using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DigitalPlatform.Components
{
    /// <summary>
    /// Meter.xaml 的交互逻辑
    /// </summary>
    public partial class Meter : UserControl
    {
        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(Meter), new PropertyMetadata(""));

        public string Unit
        {
            get { return (string)GetValue(UnitProperty); }
            set { SetValue(UnitProperty, value); }
        }
        public static readonly DependencyProperty UnitProperty =
            DependencyProperty.Register("Unit", typeof(string), typeof(Meter), new PropertyMetadata(""));

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(Meter),
                new PropertyMetadata(0.0, new PropertyChangedCallback(OnValueChanged)));

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            string sData = "M0,160A160 160 0 0 1 {0} {1}";
            sData = string.Format(sData,
                160 - 160 * Math.Cos(double.Parse(e.NewValue.ToString()) * 2.7 * Math.PI / 180),
                160 - 160 * Math.Sin(double.Parse(e.NewValue.ToString()) * 2.7 * Math.PI / 180));

            var convter = TypeDescriptor.GetConverter(typeof(Geometry));
            (d as Meter).path.Data = (Geometry)convter.ConvertFrom(sData);
        }


        public Meter()
        {
            InitializeComponent();
        }
    }
}
