using DigitalPlatform.Common.Base;
using DigitalPlatform.DeviceAccess.Base;
using DigitalPlatform.DeviceAccess.Transfer;
using DigitalPlatform.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.DeviceAccess.Execute
{
    public class ModbusASCII:ModbusBase
    {
        
        internal override Result Match(List<DevicePropItemEntity> props, List<TransferObject> tos)
        {
            var ps = props.Where(p => p.PropName == "PortName").Select(p => p.PropValue).ToList();
            return this.Match(props, tos, ps, "SerialUnit");
        }

        protected override List<byte> Read(byte slaveNum, byte funcCode, ushort startAddr, ushort count, ushort respLen)
        {
            // 一、组建请求报文
            List<byte> dataBytes = this.CreateReadPDU(slaveNum, funcCode, startAddr, count);
            // 二、计算关拼接LRC校验码
            LRC(dataBytes);
            // 三、转ASCII
            dataBytes = this.ByteArrayToAsciiArray(dataBytes);

            // 四、打开/检查通信组件的状态
            var prop = this.Props.FirstOrDefault(p => p.PropName == "TryCount");
            int tryCount = 30;
            if (prop != null)
                int.TryParse(prop.PropValue, out tryCount);
            Result connectState = this.TransferObject.Connect(tryCount);
            if (!connectState.Status)
                throw new Exception(connectState.Message);
            // 五、发送请求报文
            prop = this.Props.FirstOrDefault(p => p.PropName == "Timeout");
            int timeout = 5000;
            if (prop != null)
                int.TryParse(prop.PropValue, out timeout);

            Result<List<byte>> resp = this.TransferObject.SendAndReceived(
                dataBytes, // 发送的请求报文 
                (respLen + 4) * 2 + 3,
                11, // 异常响应报文长度
                timeout);
            if (!resp.Status)
                throw new Exception(resp.Message);

            // 六、 解析 Ascii报文 -》 byte集合
            List<byte> respBytes = resp.Data;
            respBytes.RemoveAt(0);
            respBytes.RemoveRange(respBytes.Count - 2, 2);
            // 将Ascii码转换成ByteList
            respBytes = AsciiArrayToByteArray(respBytes);

            // 五、校验检查
            List<byte> lrcValidation = respBytes.GetRange(0, respBytes.Count - 1);
            this.LRC(lrcValidation);
            if (!lrcValidation.SequenceEqual(respBytes))
            {
                throw new Exception("LRC校验检查不匹配");
                // CRC 校验失败
            }

            // 六、检查异常报文
            if (resp.Data[1] > 0x80)
            {
                byte errorCode = resp.Data[2];
                throw new Exception(Errors.ContainsKey(errorCode) ? Errors[errorCode] : "自定义异常");
            }
            // 七、截取数据字节
            List<byte> datas = respBytes.GetRange(3, respBytes.Count - 4);
            return datas;
        }

        private void LRC(List<byte> value)
        {
            if (value == null || value.Count == 0) return;

            int sum = 0;
            for (int i = 0; i < value.Count; i++)
            {
                sum += value[i];
            }

            sum = sum % 256;
            sum = 256 - sum;

            value.Add((byte)sum);
        }



        //这两个方法 frame下也能用。 方法只能dotnet
        private List<byte> ByteArrayToAsciiArray(List<byte> bytes)
        {
            // 转换成Ascii码字符16进制
            var hex = bytes.Select(b => b.ToString("X2"));
            // 转换成一个完整的字符串
            string hexStr = string.Join("", hex.ToArray());
            hexStr = ":" + hexStr + "\r\n";
            // 转换成AsciiList
            List<byte> asciiList = new List<byte>(Encoding.ASCII.GetBytes(hexStr));
            
            return asciiList;
        }

        private List<byte> AsciiArrayToByteArray(List<byte> value)
        {
            List<string> asciiStrList = new List<string>();
            //foreach (var item in value)
            //{
            //    asciiStrList.Add(((char)item).ToString());
            //}
            asciiStrList = value.Select(v => ((char)v).ToString()).ToList();

            // 将每两个Ascii字符组成一个16进制 转换成字节  ---  对应：3
            List<byte> resultBytes = new List<byte>();
            for (int i = 0; i < asciiStrList.Count; i++)
            {
                var stringHex = asciiStrList[i].ToString() + asciiStrList[++i].ToString();
                resultBytes.Add(Convert.ToByte(stringHex, 16));
            }
            return resultBytes;
        }
    }
}
