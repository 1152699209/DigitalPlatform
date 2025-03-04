using DigitalPlatform.DeviceAccess.Base;
using DigitalPlatform.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.DeviceAccess.Transfer
{
    internal class SocketTcpUnit : TransferObject
    {
        string ip;
        int port = 0;
        public Socket socket;
        public SocketTcpUnit()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.TUnit = socket;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="props"></param>
        /// <returns></returns>
        internal override Result Config(List<DevicePropItemEntity> props)
        {
            try
            {
                ip = props.FirstOrDefault(p => p.PropName == "IP")?.PropValue;
                int.TryParse(props.FirstOrDefault(p => p.PropName == "Port")?.PropValue, out port);

                return new Result();
            }
            catch (Exception ex)
            {
                return new Result(false, ex.Message);
            }
        }
        //相当于一个信号量
        ManualResetEvent TimeoutObject = new ManualResetEvent(false);
        readonly object socketLock = new object();
        internal override Result Connect(int trycount = 30)
        {
            lock (socketLock)
            {
                Result result = new Result();
                try
                {
                    if (socket == null)
                        // ProtocolType 可支持配置
                        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    int count = 0;
                    bool connectState = false;
                    //一个开始这个是阻塞状态
                    TimeoutObject.Reset();
                    while (count < trycount)
                    {
                        //判断当前端口下 是否已经连接
                        if (!(!socket.Connected || (socket.Poll(200, SelectMode.SelectRead) && (socket.Available == 0))))
                        {
                            return result;
                        }
                        try
                        {
                            socket?.Close();
                            socket.Dispose();
                            socket = null;
                            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                            socket.BeginConnect(ip, port, callback =>
                            {
                                connectState = false;
                                var cbSocket = callback.AsyncState as Socket;
                                if (cbSocket != null)
                                {
                                    connectState = cbSocket.Connected;
                                    if (cbSocket.Connected)
                                        cbSocket.EndConnect(callback);
                                }
                                //这个方法执行完后 释放一个信号量
                                TimeoutObject.Set();
                            }, socket);
                            //由于是异步方法，这里会等待对应回调释放信号量。如果超时 就说明连接失败。主要是阻塞主线程2s
                            TimeoutObject.WaitOne(2000, false);
                            if (!connectState) throw new Exception();
                            else break;
                        }
                        catch (SocketException ex)
                        {
                            if (ex.SocketErrorCode == SocketError.TimedOut)// 连接超时
                                throw new Exception(ex.Message);
                        }
                        catch (Exception) { }
                    }
                    if (socket == null || !socket.Connected || ((socket.Poll(200, SelectMode.SelectRead) && (socket.Available == 0))))
                    {
                        throw new Exception("网络连接失败");
                    }
                }
                catch (Exception ex)
                {
                    result = new Result(false, ex.Message);
                }
                return result;
            }
        }
        internal override Result<List<byte>> SendAndReceived(List<byte> req, int header_len, int timeout, Func<byte[], int> calLen = null)
        {
            lock (socketLock)
            {
                Result<List<byte>> result = new Result<List<byte>>();
                try
                {
                    socket.ReceiveTimeout = timeout;
                    if (req != null)
                    {
                        socket.Send(req.ToArray(), 0, req.Count, SocketFlags.None);
                    }
                    byte[] data = new byte[header_len];
                    //先把头长度接受出来
                    socket.Receive(data, 0, header_len, SocketFlags.None);
                    result.Data = new List<byte>(data);

                    int dataLen = 0;
                    //根据传入的方法 和  就收到的头数据 计算剩余报文长度
                    if (calLen != null)
                    {
                        //使用调用方传入的计算 报文长度的方法 计算报文
                        dataLen = calLen(data);
                    }
                    if (dataLen == 0)
                    {
                        throw new Exception("获取数据长度失败");
                    }
                    data = new byte[dataLen];
                    socket.Receive(data, 0, dataLen, SocketFlags.None);
                    result.Data.AddRange(data);
                }
                catch (SocketException ex)
                {
                    result.Status = false;
                    if (ex.SocketErrorCode == SocketError.TimedOut)
                    {
                        result.Message = "接收时间超时";
                    }
                }
                catch(Exception ex)
                {
                    result.Status = false;
                    result.Message=ex.Message;
                }
                return result;
            }
        }
        internal override Result Close()
        {
            try
            {
                if (socket?.Connected ?? false)
                    socket?.Shutdown(SocketShutdown.Both);//正常关闭连接
                base.Close();
            }
            catch { }
            try
            {
                socket?.Close();
                return new Result();
            }
            catch (Exception ex) { return new Result(false, ex.Message); }
        }
    }
    internal class SocketUdpUnit : TransferObject
    {
        internal override Result Config(List<DevicePropItemEntity> props)
        {
            return base.Config(props);
        }
    }
}
