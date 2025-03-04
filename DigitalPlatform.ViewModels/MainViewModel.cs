using DigitalPlatform.Assets;
using DigitalPlatform.DeviceAccess;
using DigitalPlatform.DeviceAccess.Base;
using DigitalPlatform.Entities;
using DigitalPlatform.IDAL;
using DigitalPlatform.Models;
using Entity;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Shapes;
using System.Windows.Threading;
using Zhaoxi.DigitaPlatform.Entities;

namespace DigitalPlatform.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private string _systemTitle;

        public string SystemTile
        {
            get { return _systemTitle; }
            set { Set(ref _systemTitle, value); }
        }
        private bool _isWindowClose;

        public bool IsWindowClose
        {
            get { return _isWindowClose; }
            set { Set(ref _isWindowClose, value); }
        }
        private int _viewBlur = 0;

        public int ViewBlur
        {
            get { return _viewBlur; }
            set { Set(ref _viewBlur, value); }
        }
        //LoginViewModel获取到的用户信息
        public UserModel GlobalUserInfo { get; set; } = new UserModel();
        public RelayCommand<object> SwitchPageCommand { get; set; }
        public List<DeviceItemModel> DeviceList { get; set; }
        public RelayCommand ComponentsConfigCommand { get; set; }
        private DateTime _date;
        public DateTime Date
        {

            get { return _date; }
            set { Set(ref _date, value); }
        }


        DispatcherTimer timer;
        public List<MonitorWarnningModel> WarnningList { get; set; }
        public List<RankingItemModel> RankingList { get; set; }

        private object _viewContent;
        public RelayCommand LogoutCommand { get; set; }
        public object ViewContent
        {
            get { return _viewContent; }
            set { Set(ref _viewContent, value); }
        }
        public RelayCommand AlarmDetailCommand { get; set; }
        public List<MenuModel> Menus { get; set; }
        private ILocalDataAccess _localDataAccess;

        // Monitor
        public VariableModel Temperature { get; set; }
        public VariableModel Humidity { get; set; }
        public VariableModel PM { get; set; }
        public VariableModel Pressure { get; set; }
        public VariableModel FlowRate { get; set; }

        //这里由App中定义的IOC容器 注入
        public MainViewModel(ILocalDataAccess localDataAccess)
        {
            _localDataAccess = localDataAccess;
            timer = new DispatcherTimer();

            timer.Interval = new TimeSpan(1000);
            timer.Tick += Timer_Tick;
            timer.Start();
            // 主窗口数据
            Menus = new List<MenuModel>();
            Menus.Add(new MenuModel
            {
                IsSelected = true,
                MenuHeader = "监控",
                MenuIcon = "\ue639",
                TargetView = "MonitorPage"
            });
            Menus.Add(new MenuModel
            {
                MenuHeader = "趋势",
                MenuIcon = "\ue61a",
                TargetView = "TrendPage"
            });
            Menus.Add(new MenuModel
            {
                MenuHeader = "报警",
                MenuIcon = "\ue60b",
                TargetView = "AlarmPage"
            });
            Menus.Add(new MenuModel
            {
                MenuHeader = "报表",
                MenuIcon = "\ue703",
                TargetView = "ReportPage"
            });
            Menus.Add(new MenuModel
            {
                MenuHeader = "配置",
                MenuIcon = "\ue60f",
                TargetView = "SettingsPage"
            });

            AlarmDetailCommand = new RelayCommand(() =>
            {
                Menus[2].IsSelected = true;
                ShowPage(Menus[2]);
            });

            ComponentsConfigCommand = new RelayCommand(ShowConfig);
            // Monitor
            Random random = new Random();
            // 用气排行
            string[] quality = new string[] { "车间-1", "车间-2", "车间-3", "车间-4",
                "车间-5" };
            RankingList = new List<RankingItemModel>();
            foreach (var q in quality)
            {
                RankingList.Add(new RankingItemModel()
                {
                    Header = q,
                    PlanValue = random.Next(100, 200),
                    FinishedValue = random.Next(10, 150),
                });
            }


            WarnningList = new List<MonitorWarnningModel>()
                {
                  new MonitorWarnningModel{Message= "PLT-01：保养到期",
                      DateTime=DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                  new MonitorWarnningModel{Message= "PLT-01：故障",
                      DateTime=DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                  new MonitorWarnningModel{Message= "PLT-01：保养到期",
                      DateTime=DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                  new MonitorWarnningModel{Message= "PLT-01：保养到期",
                      DateTime=DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                  new MonitorWarnningModel{Message= "PLT-01：保养到期",
                      DateTime=DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                  new MonitorWarnningModel{Message= "PLT-01：保养到期",
                      DateTime=DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                  new MonitorWarnningModel{Message= "PLT-01：保养到期",
                      DateTime=DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
                };



            ShowPage(Menus[0]);
            SwitchPageCommand = new RelayCommand<object>(ShowPage);
            ActionManager.Register<object>("refresh", ComponentsInit);
            //初始化组件信息
            ComponentsInit();
            //监控数据
            this.Monitor();
            //通过服务监听设备
            //this.MonitorByServer();
            LogoutCommand = new RelayCommand(DoLogout);

            ActionManager.Register<Action<Func<MainViewModel>>>(
                "GetMainViewModel",
                GetMainViewModel);

            Messenger.Default.Register<Action<Func<string[], List<DeviceItemModel>>>>(this, "DeviceInfo",
                   new Action<Action<Func<string[], List<DeviceItemModel>>>>(a => a.Invoke(ds =>
                   {
                       return this.DeviceList.Where(d => ds.Any(dd => dd == d.DeviceNum)).ToList();
                   })));

            Messenger.Default.Register<DeviceAlarmModel>(this, "Alarm", ReceiveAlarm);
            Messenger.Default.Register<string>(this, "CancelAlarm", num =>
            {
                var device = DeviceList.FirstOrDefault(d => d.DeviceNum == num);
                if (device == null) return;
                device.IsWarning = false;
            });


            // 初始化固定监测参数
            List<BaseInfoEntity> baseInfos = new List<BaseInfoEntity>();
            localDataAccess.GetBaseInfo(baseInfos, null);
            SystemTile = baseInfos.FirstOrDefault(b => b.BaseNum == "B001").Value;

            var var = baseInfos.FirstOrDefault(v => v.BaseNum == "B005");
            // 注意检查空对象
            // 温度的对象指定
            Temperature = DeviceList.FirstOrDefault(d => d.DeviceNum == var.DeviceNum)?
                .VariableList.FirstOrDefault(v => v.VarNum == var.VariableNum);
        }

        private void ReceiveAlarm(DeviceAlarmModel model)
        {
            var device = DeviceList.FirstOrDefault(d => d.DeviceNum == model.DNum);
            if (device != null && !device.IsWarning)
            {
                device.WarningMessage = model;
                this.SetWarning(model.ANum, model.AlarmContent, device, model.AlarmValue, model.VNum);
            }
        }

        private void GetMainViewModel(Action<Func<MainViewModel>> data)
        {
            data.Invoke(FuncMethod);
        }

        private MainViewModel FuncMethod()
        {
            return this;
        }

        //用于管理所有线程执行状态的信号量
        CancellationTokenSource cts = new CancellationTokenSource();
        List<Task> tasks = new List<Task>();
        private void Monitor()
        {
            //通过统一的接口 获取通信对象（提供对应的属性）
            //获取对象后，进行方法的调用
            //多设备请求：
            //1 多个设备依次轮询(一个设备处理完 关闭连接 再开下个连接--短连接)
            //2 每个设备单独监视（一个设备 一个线程 长连接）
            //场景：设备 分离，从一个设备中转  不允许多个连接
            // 需要解决传输对象的共用问题
            //这里并非所有设备都需要监听
            foreach (var item in DeviceList)
            {
                //没有相关的属性或属性值的配置 不执行监听
                if (item.PropList.Count == 0 || item.VariableList.Count == 0) continue;
                Communication comm = Communication.Create();
                //根据设备创建对应的通讯对象
                var result_eo = comm.GetExecuteObject(item.PropList.Select(p => new DevicePropItemEntity
                {
                    PropName = p.PropName,
                    PropValue = p.PropValue,
                }).ToList());
                if (!result_eo.Status)
                {
                    //异常信息
                    //item.IsWarning = true;
                    //item.WarningMessage = result_eo.Message;
                    //这里是业务逻辑上的异常  给个特定的编码
                    SetWarning("BE0001", result_eo.Message, item, vnum: "01");
                    continue;
                }
                //创建循环监视线程
                var task = Task.Run(async () =>
                {
                    //处理刷新频率
                    int delay = 500;
                    var dv = item.PropList.FirstOrDefault(p => p.PropName == "RefreshRate");
                    if (dv != null)
                    {
                        int.TryParse(dv.PropValue, out delay);
                    }

                    List<VariableProperty> variables = item.VariableList.Select(v =>
                            new VariableProperty
                            {
                                VarId = v.VarNum,
                                VarAddr = v.VarAddress,
                                ValueType = Type.GetType("System." + v.VarType)
                            }).ToList();
                    //进行Group,获取到指定设备后 就可以进行打包操作

                    Result<List<CommAddress>> result_addr = result_eo.Data.GroupAddress(variables);
                    if (!result_addr.Status)
                    {
                        // 异常提示信息

                        SetWarning("BE0002", result_addr.Message, item, vnum: "02");
                        return;
                    }
                    //循环监听
                    while (!cts.IsCancellationRequested)
                    {
                        await Task.Delay(delay);
                        //处理数据
                        //在里面处理read时 产生的异常
                        //将当前设备中所有点位信息 提供到Read方法中
                        //Read只会返回执行状态
                        //传递的是引用类型  在read方法中的修改 会作用到mas本身
                        var result_v = result_eo.Data.Read(result_addr.Data);//目前在Read方法内进行打包
                        if (!result_v.Status)
                        {
                            // 异常提示信息
                            //item.IsWarning = true;
                            //item.WarningMessage = result_v.Message;
                            SetWarning("BE0003", result_v.Message, item, vnum: "03");
                            continue;
                        }
                        //解析数据
                        //目前通信平台中返回相关数据的字节
                        //获取read方法修改后mas中的数据
                        foreach (var ma in result_addr.Data)
                        {
                            foreach (var vv in ma.Variables)
                            {
                                //根据vp.ValueBytes获取值
                                var dataBytes = vv.ValueBytes;
                                var id = vv.VariableId;
                                //根据vp.VarId找出对应的变量 并把新的值赋给它
                                var v = item.VariableList.FirstOrDefault(v => v.VarNum == id);
                                //字节序
                                Result<object> result_data = comm.ConvertType(dataBytes, Type.GetType("System." + v.VarType));
                                if (!result_data.Status)
                                {
                                    //如果数据获取过程发生异常 就报警
                                    SetWarning("BE0004", result_data.Message, item, vnum: v.VarNum);
                                    continue;
                                }
                                try
                                {
                                    //设备数据 200 偏移量-20  系数0.1
                                    // 200*0.1-20
                                    //计算对象
                                    if (v.VarType != "Boolean")
                                    {
                                        DataTable dataTable = new DataTable();
                                        string e = result_data.Data + "*" + v.Modulus + "+" + v.Offset;
                                        var result = dataTable.Compute(e, null);
                                        v.Value = result;
                                    }
                                    else
                                    {
                                        v.Value = result_data.Data;
                                    }

                                    string alarm_num = "";
                                    string union_num = "";

                                    //// 报警条件检查
                                    if (v.AlarmConditions.Count > 0)
                                    {
                                        ConditionModel cm = null;
                                        foreach (var dc in v.AlarmConditions)
                                        {
                                            string condition = v.Value + dc.Operator + dc.CompareValue;
                                            if (Boolean.TryParse(new DataTable().Compute(condition, "").ToString(), out bool result) && result)
                                            {
                                                if (!new string[] { "==", "!=" }.Contains(condition))
                                                {
                                                    cm = this.CompareValues(v.AlarmConditions.ToList(), dc.Operator, v.Value);
                                                }
                                            }
                                        }
                                        if (cm != null)
                                        {
                                            alarm_num = cm.CNum;

                                            DeviceAlarmModel model = new DeviceAlarmModel
                                            {
                                                ANum = "A-" + DateTime.Now.ToString("yyyyMMddHHmmssFFF"),
                                                CNum = cm.CNum,
                                                DNum = v.DeviceNum,
                                                VNum = v.VarNum,
                                                AlarmContent = cm.AlarmContent,
                                                AlarmValue = v.Value.ToString(),
                                                DateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                                Level = 1,
                                                State = 0
                                            };
                                            //Messenger.Default.Send<DeviceAlarmModel>(model, "Alarm");

                                            ReceiveAlarm(model);
                                        }
                                    }
                                    //根据维护的条件 进行比对，这里写相对麻烦
                                    //1.要重新获取condition
                                    //2.进行自动联动时（一个设备影响另一个设备的值时），触发警报的逻辑 可能不会从这里走。
                                    //但最终都要设置value，因此在Set方法中判断最合理
                                    //报表数据，如果每次都去提交 性能跟不上。
                                    //提交策略：
                                    //1。定量   2.定时
                                    //提交位置 多处接收


                                    ViewModelLocator.AddRecord(new RecordWriteEntity
                                    {
                                        DeviceNum = item.DeviceNum,
                                        DeviceName = DeviceList.FirstOrDefault(dd => dd.DeviceNum == v.DeviceNum)?.Header,
                                        VarNum = v.VarNum,
                                        VarName = v.VarName,
                                        RecordValue = v.Value.ToString(),
                                        AlarmNum = alarm_num,
                                        UnionNum = union_num,
                                        RecordTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                        UserName = GlobalUserInfo.UserName
                                    });

                                }
                                catch (Exception ex)
                                {
                                    //item.IsWarning = true;
                                    //item.WarningMessage = ex.Message;
                                    SetWarning("BE0005", ex.Message, item, vnum: v.VarNum);
                                }
                            }
                        }
                        //手动解除报警状态
                        // item.IsWarning = false;
                    }
                    //循环结束后 销毁通讯对象
                    result_eo.Data.Dispose();
                }, cts.Token);
                tasks.Add(task);
            }
        }

        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        private ushort clientId = 0;
        private ushort tid = 0;
        bool alive = false;
        AutoResetEvent PingResetEvent= new AutoResetEvent(false);
        Communication communication = Communication.Create();
        private void MonitorByServer()
        {
            try
            {
                // 1、TCP连接到服务器
                socket.Connect("127.0.0.1", 8899);
                socket.ReceiveTimeout = 5000;

                // 2、获取服务器颁发的ID
                byte[] idbytes = this.ReceiveBytes(socket);
                if (idbytes[4] == 0xFF)
                    return;
                // 获取ID
                clientId = BitConverter.ToUInt16(new byte[] { idbytes[8], idbytes[7] }, 0);

                // 3、提交订阅信息：所有设备的通信属性、变量信息
                foreach (var item in DeviceList)
                {
                    if (item.PropList.Count == 0 || item.VariableList.Count == 0) continue;

                    // "\"Protocl:ModbusRTU\",\"SlaveId:1\""    可以直接序列化Model，但要传输更多内容
                    var ps = string.Join(",", item.PropList.Select(p => p.PropName + ":" + p.PropValue).ToArray());
                    //把字符串 转成 字节
                    byte[] pb = Encoding.Default.GetBytes(ps);
                    var vs = string.Join(",", item.VariableList.Select(v => v.DeviceNum + "-" + v.VarNum + ":" + v.VarAddress + ":" + v.VarType).ToArray());
                    byte[] vb = Encoding.Default.GetBytes(vs);

                    // 订阅信息发送
                    // 报文ID
                    tid++;
                    tid %= 0xFFFF;
                    List<byte> subBytes = new List<byte>
                    {
                        (byte)(tid / 256),
                        (byte)(tid % 256),
                        (byte)(clientId / 256),
                        (byte)(clientId % 256),
                        0x03,
                        (byte)((pb.Length + vb.Length + 4) / 256),
                        (byte)((pb.Length + vb.Length + 4) % 256),
                        (byte)((pb.Length) / 256),
                        (byte)((pb.Length) % 256)
                    };
                    subBytes.AddRange(pb);
                    subBytes.Add((byte)((vb.Length) / 256));
                    subBytes.Add((byte)((vb.Length) % 256));
                    subBytes.AddRange(vb);

                    // 发送订阅请求
                    socket.Send(subBytes.ToArray());

                    // 接收订阅反馈（自行处理）
                    // 
                }

                // 4、开启数据接收
                var t1=Task.Factory.StartNew(async () =>
                {
                    while (!cts.IsCancellationRequested)
                    {
                        await Task.Delay(100);
                        try
                        {
                            byte[] respBytes = this.ReceiveBytes(socket);

                            switch (respBytes[4])
                            {
                                case 0x04:
                                    // 接收到订阅后，服务端所发布的最新数据
                                    var dataBytes = respBytes.ToList().GetRange(7, respBytes.Length - 7);

                                    // VarId
                                    // id
                                    ushort idlen = BitConverter.ToUInt16(new byte[] { dataBytes[1], dataBytes[0] });
                                    var idBytes = dataBytes.GetRange(2, idlen);
                                    string id_str = Encoding.Default.GetString(idBytes.ToArray());
                                    string[] ids = id_str.Split("-");

                                    //获取设备
                                    var d = this.DeviceList.FirstOrDefault(d => d.DeviceNum == ids[0]);
                                    if (d == null) continue;
                                    //获取设备对应的变量
                                    var v = d.VariableList.FirstOrDefault(v => v.VarNum == ids[1]);
                                    if (v == null) continue;


                                    int i = 2 + idlen;
                                    ushort vlen = BitConverter.ToUInt16(new byte[] { dataBytes[i + 1], dataBytes[i] });
                                    var vBytes = dataBytes.GetRange(i + 2, vlen);

                                    Result<object> result_data = communication.ConvertType(vBytes.ToArray(), Type.GetType("System." + v.VarType));
                                    // 检查结果状态 

                                    // 赋值
                                    var newValue = result_data.Data;
                                    if (v.VarType != "Boolean")
                                        newValue = new DataTable().Compute(result_data.Data.ToString() + "*" + v.Modulus.ToString() + "+" + v.Offset.ToString(), "");

                                    if (v.Value == newValue) continue;
                                    v.Value = newValue;

                                    break;
                                case 0x06:
                                    alive = true;
                                    //线程放行
                                    PingResetEvent.Set();
                                    break;

                                default: break;
                            }
                        }
                        catch
                        {
                        }
                    }
                }, cts.Token);
                tasks.Add(t1);

                // 5、开启心跳服务
                var t2=Task.Factory.StartNew(async() =>

                {
                    Socket s = socket;
                    while (!cts.IsCancellationRequested)
                    {
                        // 每隔一秒发一个心跳
                        await Task.Delay(1000);
                        tid++;
                        tid %= 0xFFFF;
                        List<byte> subBytes = new List<byte> {
                                (byte)(tid / 256), (byte)(tid % 256),
                                (byte)(clientId/256),(byte)(clientId%256),
                                0x06 ,
                                0x00,0x00
                            };
                        s.Send(subBytes.ToArray(), 0, subBytes.Count, SocketFlags.None);
                        // 接收心跳响应

                        alive = false;
                        PingResetEvent.WaitOne(3000);
                        // 扩展：尝试多次心跳（加一个计数器），都不能正常，判断断开 ，执行重连
                        if (!alive)
                            //超时后直接弹出循环  不再进行心跳检查
                            break;
                    }
                }, cts.Token);
                tasks.Add(t2);

            }
            catch (Exception)
            {

                throw;
            }
        }

        private byte[] ReceiveBytes(Socket socket)
        {
            byte[] subResp = new byte[7];
            //根据自定义的协议内容 先取7个字节
            //
            socket.Receive(subResp, 0, 7, SocketFlags.None);
            ushort len = BitConverter.ToUInt16(new byte[] { subResp[6], subResp[5] });
            if (len == 0)
                return subResp;

            byte[] pyload = new byte[len];
            socket.Receive(pyload, 0, len, SocketFlags.None);

            List<byte> bytes = new List<byte>();
            bytes.AddRange(subResp);
            bytes.AddRange(pyload);

            return bytes.ToArray();
        }
        private void SetWarning(string key, string message, DeviceItemModel device, string value = "", string vnum = "", int level = 1)
        {

            if (device.IsWarning) return;
            device.IsWarning = true;
            device.WarningMessage = new DeviceAlarmModel
            {
                ANum = "A" + DateTime.Now.ToString("yyyyMMddHHmmssFFF"),
                CNum = key,
                AlarmContent = message,
                DateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Level = level,
                State = 1
            };
            // 保存到数据库
            _localDataAccess.SaveAlarmMessage(new AlarmEntity
            {
                AlarmNum = device.WarningMessage.ANum,
                //key是对应的条件编号
                CNum = key,
                DeviceNum = device.DeviceNum,
                VariableNum = vnum,
                AlarmContent = message,
                RecordTime = device.WarningMessage.DateTime,
                AlarmLevel = level.ToString(),
                RecordValue = value,
                State = "0",
                UserId = GlobalUserInfo.UserName
            });
        }
        private void ShowConfig()
        {
            if (GlobalUserInfo.UserType > 0)
            {
                this.ViewBlur = 5;
                ActionManager.ExecuteAndResult<ConfigViewModel>("AAA", null);
                //if (ActionManager.ExecuteAndResult<ConfigViewModel>("AAA", null))
                //{
                //当从组态编辑界面推出后，先暂停所有线程。然后重新初始化设备信息
                //再开启监听
                //有耗时操作 添加一个等待页面
                //界面关闭后 通过设置信号量 关闭线程
                cts.Cancel();
                Task.WaitAll(tasks.ToArray());
                cts = new CancellationTokenSource();
                tasks.Clear();
                ComponentsInit();
                Monitor();
                //}
                this.ViewBlur = 0;
            }
            else
            {
                //提示没有权限操作  
                if (ActionManager.ExecuteAndResult<object>("ShowRight", null))
                {
                    // 执行重新登录
                    DoLogout();
                }
            }
        }
        private void DoLogout()
        {
            //重新启动
            Process.Start("DigitalPlatform.exe");
            //关闭当前窗口,怎么重VM中关闭View的窗口
            this.IsWindowClose = true;
        }
        private void ComponentsInit()
        {
            var ds = _localDataAccess.GetDevices().Select(d =>
            {
                return new DeviceItemModel(_localDataAccess)
                {
                    Header = d.Header,
                    IsMonitor = true,
                    X = double.Parse(d.X),
                    Y = double.Parse(d.Y),
                    Z = int.Parse(d.Z),
                    Width = double.Parse(d.W),
                    Height = double.Parse(d.H),
                    DeviceType = d.DeviceTypeName,
                    DeviceNum = d.DeviceNum,
                    FlowDirection = int.Parse(d.FlowDirection),
                    Rotate = int.Parse(d.Rotate),
                    PropList = new ObservableCollection<DevicePropModel>(d.Props.Select(dp => new DevicePropModel
                    {
                        PropName = dp.PropName,
                        PropValue = dp.PropValue,
                    })),

                    VariableList = new ObservableCollection<VariableModel>(d.Vars.Select(dv => new VariableModel
                    {
                        VarNum = dv.VarNum,
                        VarName = dv.Header,
                        VarAddress = dv.Address,
                        Offset = dv.Offset,
                        Modulus = dv.Modulus,
                        VarType = dv.VarType,
                        //初始化的时候 为所有变量 添加其所属的设备ID，用于后面发送警报
                        DeviceNum = d.DeviceNum,
                        AlarmConditions = new ObservableCollection<ConditionModel>(dv.AlarmConditions.Select(dc => new ConditionModel
                        {
                            CNum = dc.CNum,
                            CompareValue = dc.CompareValue,
                            AlarmContent = dc.AlarmContent,
                            Operator = dc.Operator,
                        })),
                        UnionConditions = new ObservableCollection<ConditionModel>(dv.UnionConditions.Select(du => new ConditionModel
                        {
                            CNum = du.CNum,
                            Operator = du.Operator,
                            CompareValue = du.CompareValue,

                            UnionDeviceList = new ObservableCollection<UnionDeivceModel>(du.UnionDevices.Select(dd => new UnionDeivceModel
                            {
                                UNum = dd.UNum,
                                DNum = dd.DNum,
                                Address = dd.VAddr,
                                Value = dd.Value,
                                ValueType = dd.VType
                            }))
                        })),
                    })),
                    ManualControlList = new ObservableCollection<ManualControlModel>(d.ManualControls.Select(dm => new ManualControlModel
                    {
                        ControlAddress = dm.Address,
                        Value = dm.Value,
                        ControlHeader = dm.Header,
                    }))
                };
            });
            DeviceList = ds.ToList();
            this.RaisePropertyChanged(nameof(DeviceList));
            //初始化固定检测参数

        }



        private ConditionModel CompareValues(List<ConditionModel> conditionList, string operatorStr, object currentValue)
        {
            var query = (from q in conditionList
                         where q.Operator == operatorStr &&
                         Boolean.TryParse(new DataTable().Compute(currentValue + q.Operator + q.CompareValue.ToString(), "").ToString(),
                         out bool state) && state
                         select q).ToList();
            if (query.Count > 1)
            {
                if (operatorStr == "<" || operatorStr == "<=")
                    currentValue = conditionList.Min(v => double.Parse(v.CompareValue));
                else if (operatorStr == ">" || operatorStr == ">=")
                    currentValue = conditionList.Max(v => double.Parse(v.CompareValue));

                return query.FirstOrDefault(v => v.CompareValue == currentValue);
            }
            return query[0];
        }
        private void Timer_Tick(object? sender, EventArgs e)
        {
            Date = DateTime.Now;
        }

        private void ShowPage(object obj)
        {

            // Bug：对象会重复创建，需要处理
            // 第80行解决

            var model = obj as MenuModel;
            if (model != null)
            {
                if (GlobalUserInfo.UserType == 0 && model.TargetView != "MonitorPage")
                {
                    //提示权限
                    this.Menus[0].IsSelected = true;
                    if (ActionManager.ExecuteAndResult<object>("ShowRight", null))
                    {
                        DoLogout();
                    }
                }
                else
                {
                    //判断当前类型名称 与目标界面名称是否相同
                    if (ViewContent != null && ViewContent.GetType().Name == model.TargetView) return;
                    //反射加载 或者 IOC容器
                    //Assembly.GetExecutingAssembly 加载正在运行的程序集
                    //加载View的程序集
                    Type type = Assembly.Load("DigitalPlatform.Views")
                        .GetType("DigitalPlatform.Views.Pages." + model.TargetView)!;
                    ViewContent = Activator.CreateInstance(type)!;
                }
            }
        }
        public override void Cleanup()
        {
            //关闭页面时 取消所有线程
            //
            base.Cleanup();
        }


    }
}
