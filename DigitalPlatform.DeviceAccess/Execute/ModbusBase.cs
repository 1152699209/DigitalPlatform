using DigitalPlatform.DeviceAccess.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.DeviceAccess.Execute
{
    public abstract class ModbusBase : ExecuteObject
    {
        protected static Dictionary<int, string> Errors = new Dictionary<int, string>
        {
            { 0x01, "非法功能码"},
            { 0x02, "非法数据地址"},
            { 0x03, "非法数据值"},
            { 0x04, "从站设备故障"},
            { 0x05, "确认，从站需要一个耗时操作"},
            { 0x06, "从站忙"},
            { 0x08, "存储奇偶性差错"},
            { 0x0A, "不可用网关路径"},
            { 0x0B, "网关目标设备响应失败"},
        };
        /// <summary>
        /// 地址打包在一起 减少对设备的请求次数
        /// </summary>
        /// <param name="variables"></param>
        /// <returns></returns>
        public override Result<List<CommAddress>> GroupAddress(List<VariableProperty> variables)
        {
            Result<List<CommAddress>> result = new Result<List<CommAddress>>();
            result.Data = new List<CommAddress>();
            try
            {
                //通过 400001 获取功能码 起始地址\
                //N个打包地址 每一个都是一批功能码相同的地址    
                List<ModbusAddress> addresses = new List<ModbusAddress>();
                foreach (var item in variables)
                {
                    //解析当前参数的地址  解析后的地址对象  要根据ID与点位匹配,例如40001   就是保持型寄存器的第2个地址
                    var  res = this.AnalysisAddress(item);
                    if (!res.Status)
                    {
                        result.Status = false;
                        result.Message=res.Message;
                        return result;
                    }

                    var maCurrent = res.Data as ModbusAddress;
                    maCurrent.VariableId = item.VarId;
                    var addr = addresses.FirstOrDefault(a => a.FuncCode == maCurrent.FuncCode);
                    //如果不存在同一个功能码的
                    if (addr == null)
                    {
                        //创建一个大的通讯对象
                        var ma = new ModbusAddress(maCurrent);
                        //把当前这个变量的通讯对象加入
                        ma.Variables.Add(maCurrent);
                        //把大通讯对象放到集合
                        addresses.Add(ma);
                    }
                    ///如果存在相同功能码的元素，这里的逻辑能简化。直接找出相同功能码下 最大和最小的即可
                    //这里就是找 最大范围。  一次性全部取出来 然后根据变量对应的点位 信息  把对应的值赋给变量
                    else
                    {
                        if (maCurrent.StartAddress > addr.StartAddress)
                        {
                            if (maCurrent.StartAddress + maCurrent.Length > addr.StartAddress + addr.Length)
                            {
                                addr.Length = maCurrent.StartAddress - addr.StartAddress + maCurrent.Length;
                            }
                        }
                        else if (maCurrent.StartAddress < addr.StartAddress)
                        {
                            addr.Length += addr.StartAddress - maCurrent.StartAddress;
                            addr.StartAddress = maCurrent.StartAddress;
                        }
                        addr.Variables.Add(maCurrent);
                    }
                }
                //把打包好的数据 全都放给result的Data准备返回
                addresses.ForEach(a => result.Data.Add(a));
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public override Result<CommAddress> AnalysisAddress(VariableProperty item, bool is_write = false)
        {
            Result<CommAddress> result = new Result<CommAddress>();
            try
            {
                ModbusAddress ma = new ModbusAddress();
                ma.VariableId = item.VarId;
                // 根据数据类型计算所需的寄存器数量。int 2寄存器 short1  
                int typeLen = Marshal.SizeOf(item.ValueType);
                ma.Length = typeLen / 2;    // 这里没特殊处理 使用字符串的情况。应该也用不到
                if (item.VarAddr.StartsWith("0"))
                {
                    //00 就是输入线圈
                    ma.FuncCode = 01;
                    ma.Length = 1;
                    if (is_write)
                        ma.FuncCode = 15;
                }
                else if (item.VarAddr.StartsWith("1"))
                {
                    //01 就是线圈状态
                    ma.FuncCode = 02;
                    ma.Length = 1;
                }
                else if (item.VarAddr.StartsWith("3"))
                    //03输入寄存器
                    ma.FuncCode = 04;
                else if (item.VarAddr.StartsWith("4"))
                    //04保持型寄存器
                    ma.FuncCode = is_write ? 16 : 03;
                // 起始地址
                ma.StartAddress = int.Parse(item.VarAddr.Substring(1)) - 1;// 关于减1的动作，可以通过配置来确定

                result.Data = ma;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Message = ex.Message;
            }
            return result;
        }
        //public ModbusAddress AnalysisAddress(VariableProperty item)
        //{
        //    ModbusAddress ma = new ModbusAddress();
        //    // 根据数据类型计算所需的寄存器数量
        //    int typeLen = Marshal.SizeOf(item.ValueType);
        //    ma.Length = typeLen / 2;    // 常规   特殊使用：字符串
        //    //读线圈状态
        //    if (item.VarAddr.StartsWith("0"))
        //    {
        //        ma.FuncCode = 01;
        //        ma.Length = 1;
        //    }
        //    //读输入线圈
        //    else if (item.VarAddr.StartsWith("1"))
        //    {
        //        ma.FuncCode = 02;
        //        ma.Length = 1;
        //    }
        //    //读输入输入寄存器
        //    else if (item.VarAddr.StartsWith("3"))
        //        ma.FuncCode = 04;
        //    //读输入保持型寄存器
        //    else if (item.VarAddr.StartsWith("4"))
        //        ma.FuncCode = 03;
        //    // 起始地址     关于减1的动作，可以通过配置来确定。
        //    ma.StartAddress = int.Parse(item.VarAddr.Substring(1)) - 1;
        //    return ma;
        //}
        #region 创建PDU
        protected List<byte> CreateReadPDU(byte slaveNum, byte funcCode, ushort startAddr, ushort count)
        {
            List<byte> datas = new List<byte>();
            datas.Add(slaveNum);
            datas.Add(funcCode);

            datas.Add((byte)(startAddr / 256));
            datas.Add((byte)(startAddr % 256));

            datas.Add((byte)(count / 256));
            datas.Add((byte)(count % 256));

            return datas;
        }

        protected List<byte> CreateWritePDU(byte slaveNum, byte funcCode, ushort startAddr, byte[] data)
        {
            //ModbusAddress ma = address as ModbusAddress;
            List<byte> command = new List<byte>();
            command.Add(slaveNum);
            command.Add(funcCode);
            command.Add(BitConverter.GetBytes(startAddr)[1]);
            command.Add(BitConverter.GetBytes(startAddr)[0]);

            if (funcCode == 0x10)// 写多寄存器
            {
                // 写寄存器数量
                command.Add(BitConverter.GetBytes(data.Length / 2)[1]);
                command.Add(BitConverter.GetBytes(data.Length / 2)[0]);
                // 要写入寄存器的字节数
                command.Add((byte)data.Length);
            }
            command.AddRange(data);

            return command;
        }

        #endregion
        public override Result Read(List<CommAddress> mas)
        {
            //1.变量离散不连续
            //2.一个一个的变量去读取（速度慢）。打包：在相同存储区中，且地址间隔不大。一次性全部读出来
            // 40001、40010、40100、40200
            //3.进行同类 存储区的 地址整合 40001->请求200个
            //4.把5个地址对应的数据 挑出来 然后返回
            //返回一个根据地址打包好的数据
            Result result = new Result();
            try
            {
                //获取从站地址，这里获取的从站地址  可能不是当前的通讯对象的。
                //实验这种情况  两台设备  从站地址不同 端口号相同。如果这里分别获取不到数据，还要再修改逻辑
                //这里的SlabeId
                var prop = this.Props.FirstOrDefault(p => p.PropName == "SlaveId");
                if (prop == null)
                {
                    throw new Exception("未配置从站地址");
                }
                byte slaveId = 0x01;
                byte.TryParse(prop.PropValue, out slaveId);
                foreach (ModbusAddress ma in mas)
                {
                    // 问题：打包的地址并没有做长度检查
                    ushort max = 120;// 默认情况  每次请求最多支持120个寄存器
                    int reqTotalCount = ma.Length;// 一共需要读多少个寄存器，ma.Length-》寄存器数据   一个寄存器两个字节
                    if (ma.FuncCode == 0x01 || ma.FuncCode == 0x02)
                    {
                        // 如果是线圈，每次请求最多支持120*8*2个状态     8个状态使用一个字节
                        max = 240 * 8;
                    }
                    List<byte> bytes = new List<byte>();
                    ushort startAddr = (ushort)ma.StartAddress;

                    for (ushort i = 0; i < reqTotalCount; i += max)
                    {
                        startAddr += i;
                        var perCount = (ushort)Math.Min(reqTotalCount - i, max);
                        var dataBytelen = perCount * 2;
                        if (ma.FuncCode == 0x01 || ma.FuncCode == 0x02)
                        {
                            dataBytelen = (int)Math.Ceiling(perCount * 1.0 / 8);
                        }
                        bytes.AddRange(this.Read(slaveId, (byte)ma.FuncCode, startAddr, perCount, (ushort)dataBytelen));
                    }
                    //如果是寄存器
                    if (new int[] { 04, 03 }.Contains(ma.FuncCode))
                    {
                        for (int i = 0; i < ma.Variables.Count; i++)
                        {
                            var addr = ma.Variables[i] as ModbusAddress;
                            var start = addr.StartAddress - ma.StartAddress;
                            var len = addr.Length * 2;//获取字节数量
                            byte[] dataBytes = bytes.GetRange(start * 2, len).ToArray();
                            //根据字节序调整结果
                            addr.ValueBytes = this.SwitchEndianType(dataBytes.ToList()).ToArray();
                        }
                    }
                    //线圈 一个byte对应8个状态
                    else if (new int[] { 01, 02 }.Contains(ma.FuncCode))
                    {
                        //状态全部转出来
                        //根据位置结果获取 0x00 0x01
                        List<byte> resultState = new List<byte>();
                        bytes.ForEach(b =>
                        {
                            for (int i = 0; i < 8; i++)
                            {
                                //这里resultState只存放0x00 和 0x01 
                                resultState.Add((byte)((b & (1 << i)) >> i));
                            }
                        });
                        for (int i = 0; i < ma.Variables.Count; i++)
                        {
                            var addr = ma.Variables[i] as ModbusAddress;
                            var start = addr.StartAddress - ma.StartAddress;//这里有个偏移量
                            addr.ValueBytes = resultState.GetRange(start, 1).ToArray();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Message = ex.Message;
            }
            return result;
        }

        public override Result Write(List<CommAddress> addresses)
        {
            Result result = new Result();
            try
            {
                var prop = this.Props.FirstOrDefault(p => p.PropName == "SlaveId");
                if (prop == null)
                    throw new Exception("未配置从站地址");

                byte slaveId = 0x01;
                byte.TryParse(prop.PropValue, out slaveId);


                foreach (ModbusAddress item in addresses)
                {
                    //这里会调用 对应实例化对象重写后的Write方法
                    this.Write(slaveId, (byte)item.FuncCode, (ushort)item.StartAddress, item.ValueBytes, 8);
                }
            }
            catch (Exception ex)
            {
                result = new Result(false, ex.Message);
            }

            return result;
        }
        protected virtual List<byte> Read(byte slaveNum, byte funcCode, ushort startAddr, ushort count, ushort respLen)
        {
            return null;
        }

        protected virtual void Write(byte slaveNum, byte funcCode, ushort startAddr, byte[] data, ushort respLen) { }
    }
}
