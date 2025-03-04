using DigitalPlatform.Common.Base;
using DigitalPlatform.DeviceAccess.Base;
using DigitalPlatform.DeviceAccess.Transfer;
using DigitalPlatform.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace DigitalPlatform.DeviceAccess.Execute
{
    public abstract class ExecuteObject
    {
        //动态配置字节序
        public EndianType EndianType { get; set; } = EndianType.ABCD;

        
        internal List<DevicePropItemEntity> Props { get; set; }
        internal TransferObject TransferObject { get; set; }
        public virtual Result Write(List<CommAddress> addresses) => new Result();

        //两个Match方法 涉及到方法重载
        //tos是当前存在的通讯对象列表
        //当我们创建通讯对象的时候一定会涉及到  匹配。为了后续方便通讯  还要把设备通讯参数的列表  保存下来。
        internal Result Match(List<DevicePropItemEntity> props, List<TransferObject> tos, List<string> conditions, string protocol)
        {
            Result result = new Result();
            try
            {
                this.Props = props;
                var pe = props.FirstOrDefault(p => p.PropName == "Endian");
                EndianType et = EndianType.ABCD;
                //数据属性  获取字节序
                if (pe != null)
                {
                    this.EndianType = (EndianType)Enum.Parse(typeof(EndianType), pe.PropValue);
                }
                //判断依据是 找出是否存在一个 满足我们所需要portname的串口通讯对象
                //All和Any的含义：ps中所有的子项 都要存在于对应TransferObject的condition中，即两集合元素相同
                this.TransferObject = tos.FirstOrDefault(e =>
                e.GetType().Name == "protocol" &&
                conditions.All(s => e.Conditions.Any(c => c == s))
                );
                //等于null说明没找到  要创建一个新的
                if (this.TransferObject == null)
                {
                    Type type = this.GetType().Assembly.GetType("DigitalPlatform.DeviceAccess.Transfer." + protocol);
                    this.TransferObject = (TransferObject)Activator.CreateInstance(type);
                    this.TransferObject.Conditions = conditions;
                    tos.Add(this.TransferObject);
                    //初始化相关属性
                    Result result_config = this.TransferObject.Config(props);
                    if (!result.Status)
                    {
                        return result_config;
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


        internal virtual Result Match(List<DevicePropItemEntity> props, List<TransferObject> tos) => new Result();
        public virtual Result<CommAddress> AnalysisAddress(VariableProperty item,bool isWrite=false)=>new Result<CommAddress>(false,"");
        public virtual void DisConnect() { }
        public virtual void Connect() { }
        public virtual Result Read(List<CommAddress> mas) => new Result();
        public virtual void ReadAsync() { }
       
        public virtual void WriteAsync() { }
        public virtual Result Dispose()
        {
            if (TransferObject == null)
            {
                return new Result();
            }
            try
            {
                this.TransferObject?.Close();
                return new Result();
            }
            catch (Exception e)
            {
                return new Result(false, e.Message);
            }
        }

        /// <summary>
        /// 对地址打包，把所有同类型功能码的地址整合在一起，不考虑数量
        /// </summary>
        public virtual Result<List<CommAddress>> GroupAddress(List<VariableProperty> variables) => null;

        /// <summary>
        /// 表示将一个数据字节进行指定字节序的调整
        /// </summary>
        /// <param name="bytes">接收待转换的设备中返回的字节数组</param>
        /// <returns>返回调整完成的字节数组</returns>
        public List<byte> SwitchEndianType(List<byte> bytes)
        {
            // 不管是什么字节序，这个Switch里返回的是ABCD这个顺序
            List<byte> temp = new List<byte>();
            switch (EndianType)  // alt+enter
            {
                case EndianType.ABCD:
                    temp = bytes;
                    break;
                case EndianType.DCBA:
                    for (int i = bytes.Count - 1; i >= 0; i--)
                    {
                        temp.Add(bytes[i]);
                    }
                    break;
                case EndianType.CDAB:
                    temp = new List<byte> { bytes[2], bytes[3], bytes[0], bytes[1] };
                    break;
                case EndianType.BADC:
                    temp = new List<byte> { bytes[1], bytes[0], bytes[3], bytes[2] };
                    break;
            }
            if (BitConverter.IsLittleEndian)
                temp.Reverse();

            return temp;
        }
    }

}
