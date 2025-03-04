using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.Models
{
    public class ThumbModel
    {
        public string Header {  get; set; }
        public List<ThumbItemModel> Children { get; set; } = new List<ThumbItemModel>();
    }

    public class ThumbItemModel
    {
        public string Icon { get; set; }
        //根据名称 创建对象
        public string TargetType { get; set; }

        //默认大小 
        public int Width { get; set; }
        public int Height { get; set; }
        public string Header { get; set; }
    }
}
