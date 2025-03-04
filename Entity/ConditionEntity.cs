using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    public class ConditionEntity
    {
        public string CNum { get; set; }
        public string Operator { get; set; }
        public string CompareValue { get; set; }
        public string AlarmContent { get; set; }
        public List<UDevuceEntity> UnionDevices { get; set; }

    }

    public class UDevuceEntity
    {
        public string UNum { get; set; }
        public string DNum { get; set; }
        public string VAddr { get; set; }
        public string Value { get; set; }
        public string VType { get; set; }
    }
}
