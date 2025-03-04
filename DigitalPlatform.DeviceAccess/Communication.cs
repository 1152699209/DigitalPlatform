using DigitalPlatform.DeviceAccess;
using DigitalPlatform.DeviceAccess.Base;
using DigitalPlatform.DeviceAccess.Execute;
using DigitalPlatform.DeviceAccess.Transfer;
using DigitalPlatform.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.DeviceAccess
{
    public class Communication
    {
        private static Communication _instace;
        private static object _lock = new object();
        private Communication() { }
        /// <summary>
        /// 单例模式 创建对象
        /// </summary>
        /// <returns></returns>
        public static Communication Create()
        {
            if (_instace == null)
            {
                lock (_lock)
                {
                    if (_instace == null)
                        _instace = new Communication();
                }
            }
            return _instace;
        }
        //维护当前所有创建的与实际设备的通讯对象,防止不同设备因为通讯参数相同 重复创建 产生异常
        private List<TransferObject> transferList { get; set; } = new List<TransferObject>();
        /// <summary>
        /// 该方法用于获取 对应设备的通讯执行对象
        /// </summary>
        /// <param name="props">实际设备的通讯参数列表</param>
        /// <returns></returns>
        public Result<ExecuteObject> GetExecuteObject(List<DevicePropItemEntity> props)
        {
            Result<ExecuteObject> result = new Result<ExecuteObject>();
            try
            {
                //获取协议
                var protocol = props.FirstOrDefault(p => p.PropName == "Protocol");
                if (protocol == null)
                {
                    throw new Exception("协议信息未知");
                }
                //根据协议名称动态创建类型,加载文件所在的这个程序集(传命名空)
                Type type = Assembly.Load("DigitalPlatform.DeviceAccess")
                    .GetType("DigitalPlatform.DeviceAccess.Execute." + protocol.PropValue);
                if (type == null)
                {
                    throw new Exception("执行对象类型无效");
                }
                //动态创建对象 直接转换成父类,方便后面统一的返回实际对象
                ExecuteObject eo = Activator.CreateInstance(type) as ExecuteObject;
                if (eo == null)
                {
                    throw new Exception("执行对象创建失败");
                }
                //根据参数列表 匹配对应的通讯对象，这里会根据对应的子类实例 调用对应的重写后的Match方法
                var r1 = eo.Match(props, transferList);
                if (!r1.Status)
                {
                    result.Status = false;
                    result.Message = r1.Message;
                }
                else
                {
                    result.Data = eo;
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Message = ex.Message;
            }
            return result;
        }
        public Result<object> ConvertType(byte[] valueBytes, Type type)
        {
            Result<object> result = new Result<object>();
            try
            {
                if (type == typeof(bool))
                {
                    result.Data = valueBytes[0] == 0x01;
                }
                else if (type == typeof(string))
                {
                    result.Data = Encoding.UTF8.GetString(valueBytes);
                }
                else
                {
                    // 这里不需要字节调整
                    Type tBitConverter = typeof(BitConverter);
                    MethodInfo method = tBitConverter.GetMethods(BindingFlags.Public | BindingFlags.Static)
                        .FirstOrDefault(mi => mi.ReturnType == type && mi.GetParameters().Length == 2) as MethodInfo;
                    if (method == null)
                        throw new Exception("未找到匹配的数据类型转换方法");
                    result.Data = method?.Invoke(tBitConverter, new object[] { valueBytes.ToArray(), 0 });
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Message = ex.Message;
            }
            return result;
        }
    }
}
