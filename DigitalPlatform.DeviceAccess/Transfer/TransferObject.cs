using DigitalPlatform.DeviceAccess.Base;
using DigitalPlatform.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.DeviceAccess.Transfer
{
    internal abstract class TransferObject
    {
        public object TUnit { get; set; }

        internal bool ConnectState {  get; set; }
        internal List<string> Conditions = new List<string>();

        internal virtual Result Config(List<DevicePropItemEntity> props) => new Result();
        
        internal virtual Result Close() { this.ConnectState = false; return new Result(); }
        internal virtual Result Connect(int trycount = 30) => new Result();
        internal virtual Result<List<byte>> SendAndReceived(List<byte> req, int len1, int len2, int timeout) => null;

        // 参数：calcLen
        // 委托，作用是
        internal virtual Result<List<byte>> SendAndReceived(List<byte> req, int len1, int timeout, Func<byte[], int> calcLen)
            => new Result<List<byte>>(false, "NULL");
    }
}
