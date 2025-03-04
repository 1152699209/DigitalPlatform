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
    public class ModbusTCP : ModbusBase
    {
        internal override Result Match(List<DevicePropItemEntity> props, List<TransferObject> tos)
        {
            var ps = props.Where(p => p.PropName == "IP" || p.PropName == "Socket").Select(p => p.PropValue).ToList();
            return this.Match(props, tos, ps, "SocketTcpUnit");
        }

        protected override List<byte> Read(byte slaveNum, byte funcCode, ushort startAddr, ushort count, ushort respLen)
        {
            // 一、组建请求报文
            List<byte> dataBytes = this.CreateReadPDU(slaveNum, funcCode, startAddr, count);
            // 二、拼接TCP报文头
            dataBytes = this.JointHeader(dataBytes);

            // 三、打开/检查通信组件的状态
            var prop = this.Props.FirstOrDefault(p => p.PropName == "TryCount");
            int tryCount = 30;
            if (prop != null)
                int.TryParse(prop.PropValue, out tryCount);
            Result connectState = this.TransferObject.Connect(tryCount);
            if (!connectState.Status)
                throw new Exception(connectState.Message);


            // 四、准备请求
            prop = this.Props.FirstOrDefault(p => p.PropName == "Timeout");
            int timeout = 5000;
            if (prop != null)
                int.TryParse(prop.PropValue, out timeout);

            // 获取数据
            Result<List<byte>> read_result = this.TransferObject.SendAndReceived( dataBytes,6,timeout,CalcDataLength);
            if (!read_result.Status || read_result.Data.Count == 0)
                throw new Exception("响应报文接收异常！" + read_result.Message);


            List<byte> respBytes = read_result.Data;
            respBytes.RemoveRange(0, 6);

            // 五、检查异常报文
            if (respBytes[1] > 0x80)
            {
                byte errorCode = respBytes[2];
                throw new Exception(Errors.ContainsKey(errorCode) ? Errors[errorCode] : "自定义异常");
            }
            // 六、截取数据字节
            List<byte> datas = respBytes.GetRange(3, respBytes.Count - 3);
            return datas;
        }


        private int CalcDataLength(byte[] bytes)
        {
            // 1、知道哪部分字节表示长度
            // 2、知道协议的字节序
            int length = 0;
            if (bytes != null && bytes.Length == 6)
            {
                length = BitConverter.ToUInt16(this.SwitchEndianType(new List<byte> { bytes[4], bytes[5] }).ToArray());
            }
            return length;
        }

        private List<byte> JointHeader(List<byte> bytes)
        {
            // 添加MBAP
            List<byte> byteList = new List<byte>();
            byteList.Add(0x00);// TransactionID 
            byteList.Add(0x00);// TransactionID   
            byteList.Add(0x00);
            byteList.Add(0x00); // Modbus标记
            byteList.Add((byte)(bytes.Count / 256));
            byteList.Add((byte)(bytes.Count % 256)); // 后续字节数
            byteList.AddRange(bytes);
            return byteList;
        }
    }
}
