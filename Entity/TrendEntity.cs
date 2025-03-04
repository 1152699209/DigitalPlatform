using DigitalPlatform.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    public class TrendEntity
    {
        public string TNum { get; set; }
        public string THeader { get; set; }
        public bool ShowLegend { get; set; }

        public List<AxisEntity> Axes { get; set; }
        public List<SeriesEntity> Series { get; set; }
    }
}
