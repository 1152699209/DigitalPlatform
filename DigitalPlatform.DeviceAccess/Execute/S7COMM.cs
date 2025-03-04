using DigitalPlatform.DeviceAccess.Base;
using DigitalPlatform.DeviceAccess.Transfer;
using DigitalPlatform.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.DeviceAccess.Execute
{
    internal class S7COMM:ExecuteObject
    {
        internal override Result Match(List<DevicePropItemEntity> props, List<TransferObject> tos)
        {
            var ps = props.Where(p => p.PropName == "IP" || p.PropName == "Port").Select(p => p.PropValue).ToList();
            return this.Match(props, tos, ps, "SocketTcpUnit");
        }
    }
}
