using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DigitalPlatform.Common.Converter
{
    public class UserTypeValueConverter : IValueConverter
    {
        //转过来的
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "(null)";
            else if (value.ToString() == "0") return "操作员";
            else if (value.ToString() == "1") return "技术员";
            else if (value.ToString() == "10") return "信息管理员";

            return "";
        }


        //转过去的

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
