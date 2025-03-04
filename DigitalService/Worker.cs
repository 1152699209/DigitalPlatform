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

        //ͨ�����캯�� ע��һ����־���
        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        //ִ�з���
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

        //��������
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() => 
            {
                this.StartListen(cancellationToken);

            },cancellationToken);
        }

        //ֹͣ����---�ͷ���Դ
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }

        //�����߼�
        #region ����TCP������� 
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
                _logger.LogInformation("TCPԶ�̷����������������ȴ��ͻ��˽���....");

                //���տͻ���
                AcceptClient(server, cancellationToken);

                CheckAlive(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("TCPԶ�̷�������ʧ�ܡ�" + ex.Message);
            }
        }
        #endregion

        #region ���ͻ��˻�Ծ�ԣ���������
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

        #region ���ܿͻ��˽���
        private void AcceptClient(Socket server, CancellationToken cancellationToken)
        {
            Task.Factory.StartNew(() =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    //����һ���ͻ���
                    var client = server.Accept();
                    try
                    {
                        // �����ͻ���ID
                        //�ڿͻ��˶��ĺܳ�ʱ��� ���ܲŻ���䷢����Ϣ
                        //Ҫ��¼��Ӧ�Ŀͻ���
                        //���ͻ���һ��ID�������ͻ�������Ҫ�����Լ���ID
                        ushort id = (ushort)random.Next(0, ushort.MaxValue);
                        while (Clients.Exists(c => c.ID == id))
                        {
                            id = (ushort)random.Next(0, ushort.MaxValue);
                        }
                        byte[] regBytes = new byte[]
                        {
                            0x00,0x00,0x00,0x00,0x01,0x00,0x02,(byte)(id/256),(byte)(id%256)
                        };
                        //��ͻ��˷�����Ϣ
                        //������������ID���͸��ͻ���
                        client.Send(regBytes, 0, regBytes.Length, SocketFlags.None);

                        ClientModel clientModel = new ClientModel { ID = id, Client = client, Lifetime = DateTime.Now.AddSeconds(20) };
                        Clients.Add(clientModel);
                        _logger.LogInformation("�ͻ��˽��� - " + id);

                        // ��ʼ���տͻ�����Ϣ
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


        #region ���ݽ���
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


                        // �ͻ��˱��
                        ushort id = BitConverter.ToUInt16(new byte[] { totalBytes[3], totalBytes[2] });
                        var citem = Clients.FirstOrDefault(c => c.ID == id);
                        if (citem == null) continue;
                        //ֻҪ�յ���Ӧ�ͻ��˵��κ���Ϣ ���ӳ�20s
                        citem.Lifetime = DateTime.Now.AddSeconds(20);

                        List<byte> byteList = new List<byte>();
                        //��ȡǰ5���ֽ�
                        byteList.AddRange(totalBytes.GetRange(0, 5));
                        switch (byteList[4])
                        {
                            case 0xCF:
                                break;
                            case 0x03:
                                // ��������Ϣ
                                _logger.LogInformation("���յ�������Ϣ");
                                // ������Ϣ   �����豸ͨ�Ų��������
                                var infoBytes = totalBytes.GetRange(7, totalBytes.Count - 7);// ���ݲ���
                                ushort plen = BitConverter.ToUInt16(new byte[] { infoBytes[1], infoBytes[0] });
                                var pBytes = infoBytes.GetRange(2, plen);
                                string pstr = Encoding.Default.GetString(pBytes.ToArray(), 0, plen);
                                _logger.LogInformation(pstr);
                                citem.Properties.Add(pstr.Split(','));// �����༭
                                int i = 2 + plen;
                                ushort vlen = BitConverter.ToUInt16(new byte[] { infoBytes[i + 1], infoBytes[i] });
                                var vBytes = infoBytes.GetRange(i + 2, vlen);
                                string vstr = Encoding.Default.GetString(vBytes.ToArray(), 0, vlen);
                                _logger.LogInformation(vstr);
                                citem.Variables.Add(vstr.Split(','));
                                break;
                            case 0x06:
                                //���ﲻ��Ҫ���⴦��
                                break;
                            case 0x07:
                                citem.Lifetime=DateTime.Now;
                                break;
                            default:
                                break;
                        }
                        //����һ��֪ͨ��Ϣ
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
                _logger.LogInformation("��ʼ���ļ��� " + clientModel.ID);

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
                        // �����쳣����
                        _logger.LogInformation("��ȡͨ�Ŷ����쳣");
                        continue;
                    }
                    //��ȡ��ǰ�豸 ע������в���
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

                    // ����û�������鴦�������Ҫ����Ļ���
                    // 1���Խӷ��鷽��
                    // 2�����صĽ��һ��������Ľ��
                    foreach (var v in vs)
                    {
                        var ra = result_eo.Data.AnalysisAddress(v);
                        if (!ra.Status)
                        {
                            // �����쳣����
                            _logger.LogInformation("������ַ�쳣");
                            continue;
                        }
                        ra.Data.Variables.Add(ra.Data);

                        var result_value = result_eo.Data.Read(new List<CommAddress> { ra.Data });
                        if (!result_value.Status)
                        {
                            // �����쳣����
                            _logger.LogInformation("��ȡ�����쳣");
                            continue;
                        }
                        // ���ݷ��أ�
                        if (!clientModel.Values.ContainsKey(v.VarId))
                            clientModel.Values.Add(v.VarId, new byte[] { });
                        //�����ǰ���ݸ� ֮ǰ��ȡ���Ĳ���ͬ �ͷ��ظ��ͻ���
                        if (!clientModel.Values[v.VarId].SequenceEqual(ra.Data.ValueBytes))
                        {
                            _logger.LogInformation("��ȡ�������ݣ�������");
                            // ����������ʷ���ݲ�һ��ʱ������
                            // ֪ͨ��ֵ�仯
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
