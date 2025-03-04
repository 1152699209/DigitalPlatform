using System.Net.Sockets;
using System.Net;
using System;
using System.Text;
using DigitalPlatform.DeviceAccess;
using DigitalPlatform.Entities;
using DigitalPlatform.DeviceAccess.Base;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DigitalService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        //通过构造函数 注入一个日志输出
        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        //执行服务
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //if (_logger.IsEnabled(LogLevel.Information))
                //{
                //    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                //}
                await Task.Delay(1000, stoppingToken);
            }
        }

        //开启服务
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() => 
            {
                this.StartListen(cancellationToken);

            },cancellationToken);
        }

        //停止服务---释放资源
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }

        //监听逻辑
        #region 启动TCP服务监听 
        Socket server;
        Random random = new Random();
        List<ClientModel> Clients = new List<ClientModel>();
        private void StartListen(CancellationToken cancellationToken)
        {
            try
            {
                server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                server.Bind(new IPEndPoint(IPAddress.Any, 8899));
                server.Listen(10);
                _logger.LogInformation("TCP远程服务已启动监听，等待客户端接入....");

                //接收客户端
                AcceptClient(server, cancellationToken);

                CheckAlive(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("TCP远程服务启动失败。" + ex.Message);
            }
        }
        #endregion

        #region 检查客户端活跃性，心跳保活
        private void CheckAlive(CancellationToken cancellationToken)
        {
            var t = Task.Factory.StartNew(async () =>
            {
                int index = 0;
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(1000);

                    if (Clients.Count == 0) continue;

                    if (Clients[index].Lifetime < DateTime.Now)
                    {
                        Clients[index].Client.Shutdown(SocketShutdown.Both);
                        Clients[index].Client.Close();
                        Clients[index].Client.Dispose();

                        Clients.RemoveAt(index);
                    }
                    else
                        index++;
                    index %= Clients.Count;
                }
            }, cancellationToken);
        }
        #endregion

        #region 接受客户端接入
        private void AcceptClient(Socket server, CancellationToken cancellationToken)
        {
            Task.Factory.StartNew(() =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    //接受一个客户端
                    var client = server.Accept();
                    try
                    {
                        // 创建客户端ID
                        //在客户端订阅很长时间后 可能才会对其发布消息
                        //要记录对应的客户端
                        //给客户端一个ID，后续客户端请求都要带着自己的ID
                        ushort id = (ushort)random.Next(0, ushort.MaxValue);
                        while (Clients.Exists(c => c.ID == id))
                        {
                            id = (ushort)random.Next(0, ushort.MaxValue);
                        }
                        byte[] regBytes = new byte[]
                        {
                            0x00,0x00,0x00,0x00,0x01,0x00,0x02,(byte)(id/256),(byte)(id%256)
                        };
                        //向客户端发送消息
                        //服务把这边生成ID发送给客户端
                        client.Send(regBytes, 0, regBytes.Length, SocketFlags.None);

                        ClientModel clientModel = new ClientModel { ID = id, Client = client, Lifetime = DateTime.Now.AddSeconds(20) };
                        Clients.Add(clientModel);
                        _logger.LogInformation("客户端接入 - " + id);

                        // 开始接收客户端消息
                        Receive(client, cancellationToken);

                        // start subscribe monitor
                        this.MonitorData(clientModel, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogInformation("AcceptClient - " + ex.Message);
                        client.Shutdown(SocketShutdown.Both);
                        client.Close();
                        client.Dispose();
                    }
                }
            }
            , cancellationToken);
        }
        #endregion


        #region 数据接收
        private void Receive(Socket client, CancellationToken cancellationToken)
        {
            Task.Factory.StartNew(() =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        List<byte> totalBytes = new List<byte>();
                        byte[] respBytes = new byte[7];
                        client.Receive(respBytes, 0, 7, SocketFlags.None);
                        totalBytes.AddRange(respBytes);

                        ushort len = BitConverter.ToUInt16(new byte[] { respBytes[6], respBytes[5] });
                        if (len > 0)
                        {
                            byte[] dataBytes = new byte[len];
                            client.Receive(dataBytes, 0, len, SocketFlags.None);
                            totalBytes.AddRange((byte[])dataBytes);
                        }


                        // 客户端编号
                        ushort id = BitConverter.ToUInt16(new byte[] { totalBytes[3], totalBytes[2] });
                        var citem = Clients.FirstOrDefault(c => c.ID == id);
                        if (citem == null) continue;
                        //只要收到对应客户端的任何消息 就延长20s
                        citem.Lifetime = DateTime.Now.AddSeconds(20);

                        List<byte> byteList = new List<byte>();
                        //先取前5个字节
                        byteList.AddRange(totalBytes.GetRange(0, 5));
                        switch (byteList[4])
                        {
                            case 0xCF:
                                break;
                            case 0x03:
                                // 处理订阅信息
                                _logger.LogInformation("接收到订阅消息");
                                // 订阅信息   解析设备通信参数与变量
                                var infoBytes = totalBytes.GetRange(7, totalBytes.Count - 7);// 数据部分
                                ushort plen = BitConverter.ToUInt16(new byte[] { infoBytes[1], infoBytes[0] });
                                var pBytes = infoBytes.GetRange(2, plen);
                                string pstr = Encoding.Default.GetString(pBytes.ToArray(), 0, plen);
                                _logger.LogInformation(pstr);
                                citem.Properties.Add(pstr.Split(','));// 后续编辑
                                int i = 2 + plen;
                                ushort vlen = BitConverter.ToUInt16(new byte[] { infoBytes[i + 1], infoBytes[i] });
                                var vBytes = infoBytes.GetRange(i + 2, vlen);
                                string vstr = Encoding.Default.GetString(vBytes.ToArray(), 0, vlen);
                                _logger.LogInformation(vstr);
                                citem.Variables.Add(vstr.Split(','));
                                break;
                            case 0x06:
                                //这里不需要特殊处理
                                break;
                            case 0x07:
                                citem.Lifetime=DateTime.Now;
                                break;
                            default:
                                break;
                        }
                        //返回一个通知信息
                        byteList.Add(0x00);
                        byteList.Add(0x00);
                        client.Send(byteList.ToArray(), 0, byteList.Count, SocketFlags.None);

                    }
                    catch (Exception)
                    {
                        
                        throw;
                    }
                }
            }, cancellationToken);
        }

        private void MonitorData(ClientModel clientModel, CancellationToken cancellationToken)
        {
            Task.Factory.StartNew(() =>
            {
                _logger.LogInformation("开始订阅监听 " + clientModel.ID);

                Communication communication = Communication.Create();

                int dindex = 0;
                while (!cancellationToken.IsCancellationRequested)
                {
                    var prop = clientModel.Properties[dindex];
                    var result_eo = communication.GetExecuteObject(
                        prop.Select(p =>
                        new DevicePropItemEntity { PropName = p.Split(":")[0], PropValue = p.Split(":")[1] }
                        ).ToList());
                    if (!result_eo.Status)
                    {
                        // 发送异常报文
                        _logger.LogInformation("获取通信对象异常");
                        continue;
                    }
                    //获取当前设备 注册的所有参数
                    var variable = clientModel.Variables[dindex];

                    dindex++;
                    dindex %= clientModel.Variables.Count;

                    var vs = variable.Select(v => new VariableProperty
                    {
                        //VarId=DeviceNum-VarNum
                        VarId = v.Split(":")[0],
                        VarAddr = v.Split(":")[1],
                        ValueType = Type.GetType("System." + v.Split(":")[2])
                    }).ToList();

                    // 这里没有做分组处理，如果需要分组的话，
                    // 1、对接分组方法
                    // 2、返回的结果一定是请求的结果
                    foreach (var v in vs)
                    {
                        var ra = result_eo.Data.AnalysisAddress(v);
                        if (!ra.Status)
                        {
                            // 发送异常报文
                            _logger.LogInformation("解析地址异常");
                            continue;
                        }
                        ra.Data.Variables.Add(ra.Data);

                        var result_value = result_eo.Data.Read(new List<CommAddress> { ra.Data });
                        if (!result_value.Status)
                        {
                            // 发送异常报文
                            _logger.LogInformation("读取数据异常");
                            continue;
                        }
                        // 数据返回，
                        if (!clientModel.Values.ContainsKey(v.VarId))
                            clientModel.Values.Add(v.VarId, new byte[] { });
                        //如果当前数据跟 之前读取到的不相同 就返回给客户端
                        if (!clientModel.Values[v.VarId].SequenceEqual(ra.Data.ValueBytes))
                        {
                            _logger.LogInformation("读取到新数据，并发送");
                            // 当数据与历史数据不一致时，发布
                            // 通知新值变化
                            clientModel.Values[v.VarId] = ra.Data.ValueBytes;
                            byte[] idBytes = Encoding.Default.GetBytes(v.VarId);
                            List<byte> sendBytes = new List<byte> {
                                    0x00,0x00,
                                    (byte)(clientModel.ID/256),(byte)(clientModel.ID%256),
                                    0x04,
                                    (byte)((idBytes.Length+ra.Data.ValueBytes.Length+4)/256),
                                    (byte)((idBytes.Length+ra.Data.ValueBytes.Length+4)%256),
                                    (byte)(idBytes.Length/256),
                                    (byte)(idBytes.Length%256),
                                };
                            sendBytes.AddRange(idBytes);
                            sendBytes.Add((byte)(ra.Data.ValueBytes.Length / 256));
                            sendBytes.Add((byte)(ra.Data.ValueBytes.Length % 256));
                            sendBytes.AddRange(ra.Data.ValueBytes);
                            clientModel.Client.Send(sendBytes.ToArray(), 0, sendBytes.Count, SocketFlags.None);
                        }
                    }
                }
            }, cancellationToken);
        }
        #endregion
    }
}
