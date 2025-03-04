using DigitalPlatform.DeviceAccess.Base;
using DigitalPlatform.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.DeviceAccess.Transfer
{
    internal class SerialUnit : TransferObject
    {
        private static readonly object trans_lock = new object();

        SerialPort serialPort;
        public SerialUnit()
        {
            serialPort = new SerialPort();
            this.TUnit = serialPort;
        }
        internal override Result Config(List<DevicePropItemEntity> props)
        {
            // 端口名称、波特率、数据位、校验位、停止位
            Result result = new Result();
            try
            {
                foreach (var item in props)
                {
                    object v = null;
                    PropertyInfo pi = serialPort.GetType().GetProperty(item.PropName.Trim(), BindingFlags.Public | BindingFlags.Instance);
                    if (pi == null) continue;
                    Type propType = pi.PropertyType;
                    //如果是枚举类型  就按照值转换成对应的枚举类型
                    if (propType.IsEnum)
                    {
                        v = Enum.Parse(propType, item.PropValue.Trim() as string);
                    }
                    //其他类型 按照其类型转换
                    else
                    {
                        v = Convert.ChangeType(item.PropValue.Trim(), propType);
                    }
                    pi.SetValue(serialPort, v);
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Message = ex.Message;
            }
            return result;
        }
        internal override Result Connect(int trycount = 30)
        {
            Result result = new Result();
            try
            {
                //根据重试次数 判断循环
                int count = 0;
                while (count < trycount)
                {
                    if (serialPort.IsOpen)
                        break;
                    try
                    {
                        serialPort.Open();
                        break;
                    }
                    catch (System.IO.IOException)
                    {
                        Task.Delay(1).GetAwaiter().GetResult();
                        count++;
                    }
                }
                if (serialPort == null || !serialPort.IsOpen)
                    throw new Exception("串口打开失败");
                this.ConnectState = false;
            }
            catch (Exception e)
            {
                result.Status = false;
                result.Message = e.Message;
            }
            return result;
        }
        internal override Result<List<byte>> SendAndReceived(List<byte> req, int receiveLen, int errorLen, int timeout = 5000)
        {
            lock (trans_lock)
            {
                Result<List<byte>> result = new Result<List<byte>>();
                // 发送
                serialPort.Write(req.ToArray(), 0, req.Count);
                List<byte> respBytes = new List<byte>();
                try
                {
                    serialPort.ReadTimeout = timeout;
                    while (respBytes.Count < receiveLen)
                    {
                        respBytes.Add((byte)serialPort.ReadByte());
                        // 临时处理   需要兼容其他协议  目前只处理了ModbusRTU的需求
                        if (respBytes.Count > 1 && respBytes[1] > 0x80)
                            receiveLen = errorLen;
                    }
                }
                //这个Catch可能还会因为 收到的是带有返回错误信息的报文
                //如何收到错误返回报文  就不报错  ，设备仍然正常联通
                catch (TimeoutException)
                {
                    if (respBytes.Count != errorLen && respBytes.Count != receiveLen)
                    {
                        result.Status = false;
                        result.Message = "接收报文超时";
                    }
                }
                catch (Exception e)
                {
                    // 异常：一定时间内没有拿到字节数据
                    result.Status = false;
                    result.Message = e.Message;
                }
                finally
                {
                    result.Data = respBytes;
                }
                return result;
            }
        }
        internal override Result Close()
        {
            if (serialPort != null)
                serialPort.Close();
            this.ConnectState = false;
            return new Result();
        }

    }
}
