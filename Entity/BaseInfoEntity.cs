using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    public class BaseInfoEntity
    {
        [Column("b_num")]
        public string BaseNum { get; set; }
        [Column("header")]
        public string Header { get; set; }
        [Column("content")]
        public string Description { get; set; }
        [Column("value")]
        public string Value { get; set; }
        [Column("d_num")]
        public string DeviceNum { get; set; }
        [Column("v_num")]
        public string VariableNum { get; set; }
        public string type { get; set; }
    }
}
