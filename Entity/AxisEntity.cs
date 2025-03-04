using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.Entities
{
    public class AxisEntity
    {
        public string ANum { get; set; }
        public string Title { get; set; }
        public bool IsShowTitle { get; set; }
        public string Minimum { get; set; }
        public string Maximum { get; set; }
        public bool IsShowSeperator { get; set; }
        public string LabelFormatter { get; set; }
        public string Position { get; set; }

        public List<SectionEntity> Sections { get; set; }
    }
}
