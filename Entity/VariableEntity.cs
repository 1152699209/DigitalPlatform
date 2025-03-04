using Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.Entities
{
    public class VariableEntity
    {
        // 后面做报警
        public string VarNum { get; set; }
        public string Header { get; set; }
        public string Address { get; set; }
        public double Offset { get; set; }
        public double Modulus { get; set; }

        
        public string VarType { get; set; }

        public List<ConditionEntity> AlarmConditions { get; set; }
        public List<ConditionEntity> UnionConditions { get; set; }
    }
}
