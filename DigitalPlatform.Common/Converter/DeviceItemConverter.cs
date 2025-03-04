using DigitalPlatform.Components;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DigitalPlatform.Common.Converter
{
    public class DeviceItemConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            #region 创建水平和垂直的对齐线
            if (value.ToString() == "HL")
            {
                return new Line
                {
                    X1 = 0,
                    Y1 = 0,
                    X2 = 2000,
                    Y2 = 0,
                    Height = 0.5,
                    Stroke = Brushes.Red,
                    StrokeThickness = 1,
                    StrokeDashArray = new DoubleCollection { 3.0, 3.0 },
                    ClipToBounds = true,
                };
            }
            else if (value.ToString() == "VL")
            {
                return new Line
                {
                    X1 = 0,
                    Y1 = 0,
                    X2 = 0,
                    Y2 = 2000,
                    Width = 0.5,
                    Stroke = Brushes.Red,
                    StrokeThickness = 1,
                    StrokeDashArray = new DoubleCollection { 3.0, 3.0 },
                    ClipToBounds = true,
                };
            }
            #endregion
            //根据类型 反射创建对象
            var assembly = Assembly.Load("DigitalPlatform.Components");
            Type t = assembly.GetType("DigitalPlatform.Components." + value.ToString());
            //这里创建的组件不同，每个组件都要进行绑定
            //解决方法 ：创建父类组件 避免了对不同类型的组件进行类型转换  统一设置绑定关系
            var obj = Activator.CreateInstance(t)!;
            if(!new string[] { "WidthRule", "HeightRule" }.Contains(value.ToString()))
            {
                var c = (ComponentBase)obj!;
                //这里要使用relativeSource去找对应的VM才行，不然绑不上。因为他的默认DC是 集合
                RelativeSource relativeSource = new RelativeSource();
                relativeSource.AncestorType = typeof(ItemsControl);
                relativeSource.Mode = RelativeSourceMode.FindAncestor;
                Binding binding = new Binding();
                binding.RelativeSource = relativeSource;
                binding.Path = new System.Windows.PropertyPath("DataContext.DeleteCommand");
                //传的是父类的依赖属性
                c.SetBinding(ComponentBase.DeleteCommandProperty, binding);
                binding = new Binding();
                binding.Path = new System.Windows.PropertyPath(".");
                c.SetBinding(ComponentBase.DeleteParameterProperty, binding);
                binding = new Binding();
                binding.Path = new System.Windows.PropertyPath("IsSelected");
                c.SetBinding(ComponentBase.IsSelectedProperty, binding);
                //处理组件尺寸缩放的绑定
                binding = new Binding();
                binding.Path = new System.Windows.PropertyPath("ResizeMoveCommand");
                c.SetBinding(ComponentBase.ResizeMoveCommandProperty, binding);
                binding = new Binding();
                binding.Path = new System.Windows.PropertyPath("ResizeDownCommand");
                c.SetBinding(ComponentBase.ResizeDownCommandProperty, binding);
                binding = new Binding();
                binding.Path = new System.Windows.PropertyPath("ResizeUpCommand");
                c.SetBinding(ComponentBase.ResizeUpCommandProperty, binding);


                binding = new Binding();
                binding.Path = new System.Windows.PropertyPath("Rotate");
                c.SetBinding(ComponentBase.RotateAngleProperty, binding);
                //绑定管道流动方向
                binding = new Binding();
                binding.Path = new System.Windows.PropertyPath("FlowDirection");
                c.SetBinding(ComponentBase.FlowDirectionProperty, binding);
                //绑定报警状态
                binding = new Binding();
                binding.Path = new System.Windows.PropertyPath("IsWarning");
                c.SetBinding(ComponentBase.IsWarningProperty, binding);
 
                binding = new Binding();
                binding.Path = new System.Windows.PropertyPath("WarningMessage.AlarmContent");
                c.SetBinding(ComponentBase.WarningMessageProperty, binding);

                binding = new Binding();
                binding.Path = new System.Windows.PropertyPath("IsMonitor");// Model中的属性
                c.SetBinding(ComponentBase.IsMonitorProperty, binding);// 组件中的依赖属性
                binding = new Binding();
                binding.Path = new System.Windows.PropertyPath("VariableList");// Model中的属性
                c.SetBinding(ComponentBase.VarListProperty, binding);// 组件中的依赖属性


                binding = new Binding();
                binding.Path = new System.Windows.PropertyPath("ManualControlList");// Model中的属性
                c.SetBinding(ComponentBase.ControlListProperty, binding);// 组件中的依赖属性
                binding = new Binding();
                binding.Path = new System.Windows.PropertyPath("ManualControlCommand");// Model中的属性
                c.SetBinding(ComponentBase.ManualControlCommandProperty, binding);// 组件中的依赖属性

                // xaml  C#
                // "{Binding RelativeSource={RelativeSource Type=Window},Path=DataContext.AlarmDetailCommand}"
                binding = new Binding();
                binding.Path = new System.Windows.PropertyPath("DataContext.AlarmDetailCommand");// Model中的属性
                binding.RelativeSource = new RelativeSource { AncestorType = typeof(Window) };
                c.SetBinding(ComponentBase.AlarmDetailCommandProperty, binding);// 组件中的依赖属性

                return c;
            }
            return obj;   
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
