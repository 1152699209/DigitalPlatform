using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DigitalService
{
    public class ClientModel
    {
        //客户端ID
        public ushort ID { get; set; }
        //客户端通讯对象
        public Socket Client { get; set; }

        public List<string[]> Properties { get; set; } = new List<string[]>();
        public List<string[]> Variables { get; set; } = new List<string[]>();

        //客户端最大生命周期
        public DateTime Lifetime { get; set; }// 服务接收客户   Receive()
        public Dictionary<string, byte[]> Values { get; set; } = new Dictionary<string, byte[]>();
    }
}
