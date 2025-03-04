using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.DeviceAccess.Base
{
    public class CommAddress
    {
        public string VariableId { get; set; }
        public Type ValueType { get; set; }
        public byte[] ValueBytes { get; set; }
        public List<CommAddress> Variables { get; set; } = new List<CommAddress>();
    }
}
