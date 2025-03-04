using DigitalPlatform.Assets;
using DigitalPlatform.Common.Base;
using DigitalPlatform.Entities;
using DigitalPlatform.IDAL;
using DigitalPlatform.Models;
using Entity;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;

namespace DigitalPlatform.ViewModels
{
    /// <summary>
    /// 对应窗口的数据逻辑 
    /// </summary>
    public class ConfigViewModel : ViewModelBase
    {
        public RelayCommand<object> ThumbCommand { get; set; }
        public RelayCommand<object> ItemDropCommand { get; set; }
        private string _saveFailedMessage;
        public string SaveFailedMessage
        {
            get { return _saveFailedMessage; }
            set { Set(ref _saveFailedMessage, value); }
        }
        public RelayCommand<DevicePropModel> DeletePropCommand { get; set; }
        public RelayCommand AddVariableCommand { get; set; }
        public RelayCommand<VariableModel> DeleteVarCommand { get; set; }
        public List<ThumbModel> ThumbList { get; set; }
        public RelayCommand AlarmConditionCommand { get; set; }
        public RelayCommand UnionConditionCommand { get; set; }

        private bool _windowState = false;
        public RelayCommand<object> CloseCommand { get; set; }
        public RelayCommand<object> MouseMoveCommand { get; set; }
        private DeviceItemModel _currentDevice;
        public DeviceItemModel CurrentDevice
        {
            get { return _currentDevice; }
            set { Set(ref _currentDevice, value); }
        }
        public List<PropOptionModel> PropOptions { get; set; }
        public RelayCommand<object> SaveCommand { get; set; }
        public RelayCommand<object> CloseSaveFailedCommand { get; set; }
        public RelayCommand<DeviceItemModel> DeviceSeletcedCommand { get; set; }
        public RelayCommand<object> DeleteCommand { get; set; }
        public RelayCommand<DeviceItemModel> AddPropCommand { get; set; }
        public ObservableCollection<DeviceItemModel> DeviceList { get; set; } = new ObservableCollection<DeviceItemModel>();
        private ILocalDataAccess _localDataAccess;
        public ConfigViewModel(ILocalDataAccess localDataAccess)
        {
            _localDataAccess = localDataAccess;
            if (!IsInDesignMode)
            {
                ///组件获取方式
                ///1. 维护数据库
                ///2. 使用配置文件
                ///3. 扫描组件库（特定类型）可以通过添加特性的方式 进行筛选
                ThumbList = new List<ThumbModel>();
                // 通过数据库维护
                var ts = localDataAccess.GetThumbList();
                //按照种类 Groupby
                ThumbList = ts.GroupBy(t => t.Category).Select(g => new ThumbModel
                {
                    Header = g.Key,
                    Children = g.Select(b => new ThumbItemModel
                    {
                        Icon = "pack://application:,,,/DigitalPlatform.Assets;component/Images/Thumbs/" + b.Icon,
                        Header = b.Header,
                        TargetType = b.TargetType,
                        Width = b.Width,
                        Height = b.Height
                    }).ToList()
                }).ToList();
                //初始化组件通信参数
                PropOptions = new List<PropOptionModel>();
                var pos = localDataAccess.GetPropertyOption();
                PropOptions = pos.Select(p =>
                {
                    var pom = new PropOptionModel()
                    {
                        Header = p.PropHeader,
                        PropName = p.PropName,
                        PropType = p.PropType,


                    };
                    //初始化当前属性的 属性值 并且当前属性还要有一个默认的选择项
                    var list = InitOptions(p.PropName, out int DefacultIndex);
                    pom.ValueOptions = list;
                    pom.DefaultIndex = DefacultIndex;
                    return pom;

                }).ToList();


                ItemDropCommand = new RelayCommand<object>(DoItemDropCommand);
                ThumbCommand = new RelayCommand<object>(DoThumbCommand);
                SaveCommand = new RelayCommand<object>(DoSaveCommand);
                DeleteCommand = new RelayCommand<object>(DoDeleteCommand);
                AddPropCommand = new RelayCommand<DeviceItemModel>(DoAddPropCommand);
                DeletePropCommand = new RelayCommand<DevicePropModel>(DoDeletePropComman);
                DeleteVarCommand = new RelayCommand<VariableModel>(DoDeleteVarCommand);
                AddVariableCommand = new RelayCommand(() =>
                {
                    if (CurrentDevice != null)
                        CurrentDevice.VariableList.Add(new VariableModel()
                        {
                            VarNum = "V" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                            Modulus = 1,
                            Offset = 0
                        });
                });
                CloseSaveFailedCommand = new RelayCommand<object>((obj) =>
                {
                    VisualStateManager.GoToElementState(obj as Window, "SaveFailClose", true);
                });
                DeviceSeletcedCommand = new RelayCommand<DeviceItemModel>(model =>
                {
                    //这里的点击 会触发一个冒泡事件
                    //记录一个当前选中组件
                    //对当前组件进行选中
                    //如果当前已经选中  就隐藏这个已经选中的
                    if (CurrentDevice != null)
                    {
                        CurrentDevice.IsSelected = false;
                    }
                    if (model != null)
                    {
                        model.IsSelected = true;
                    }

                    CurrentDevice = model;
                });
                AlarmConditionCommand = new RelayCommand(() =>
                {
                    if (CurrentDevice != null)
                        ActionManager.Execute("AlarmCondition", CurrentDevice);
                });

                UnionConditionCommand = new RelayCommand(() =>
                {
                    if (CurrentDevice != null)
                        ActionManager.Execute("UnionCondition", CurrentDevice);
                });

                //初始化组态组件
                ComponentsInit();

                //通过一个三重委托  使得连控条件对象能获得到当前的设备列表
                ActionManager.Register<Action<Func<List<DeviceDropModel>>>>(
                "GetDevice",
                ResponseGet);
            }
        }
        //这个方法接收Doget方法作为参数,data相当于Doget
        private void ResponseGet(Action<Func<List<DeviceDropModel>>> data)
        {
            data.Invoke(FuncMethod);
        }
        //相当于Doget(FuncMethod)---> Doget执行相当于 this.dropdeviceList=FuncMethod();
        private List<DeviceDropModel> FuncMethod()
        {
            return new List<DeviceDropModel>(
                    this.DeviceList
                        .Where(d => !new string[] { "HL", "VL", "WidthRule", "HeightRule" }.Contains(d.DeviceType) && d.PropList.Count > 0)
                        .Select(d => new DeviceDropModel
                        {
                            DNum = d.DeviceNum,
                            DHeader = d.Header
                        }));
        }
        private void DoDeleteVarCommand(VariableModel model)
        {
            CurrentDevice.VariableList.Remove(model);
        }
        private void DoDeletePropComman(DevicePropModel model)
        {
            if (CurrentDevice != null)
            {
                CurrentDevice.PropList.Remove(model);

            }
        }
        private void DoAddPropCommand(DeviceItemModel obj)
        {
            if (CurrentDevice != null)
            {
                CurrentDevice.PropList.Add(new DevicePropModel()
                {
                    PropName = "Protocol",
                    PropValue = "ModBusRtu"
                });
            }
        }
        //移除对象
        private void DoDeleteCommand(object obj)
        {

            DeviceList.Remove(obj as DeviceItemModel);
        }
        private void ComponentsInit()
        {
            var ds = _localDataAccess.GetDevices().Select(d =>
            {
                var dim = new DeviceItemModel(_localDataAccess)
                {
                    IsMonitor = false,
                    Header = d.Header,
                    X = double.Parse(d.X),
                    Y = double.Parse(d.Y),
                    Z = int.Parse(d.Z),
                    Width = double.Parse(d.W),
                    Height = double.Parse(d.H),
                    DeviceType = d.DeviceTypeName,
                    DeviceNum = d.DeviceNum,
                    FlowDirection = int.Parse(d.FlowDirection),
                    Rotate = int.Parse(d.Rotate),
                    //通过委托  从vm中获取所有的设备
                    Devices = () => DeviceList.ToList(),
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
                dim.InitContextMenu();
                return dim;
            });
            DeviceList = new ObservableCollection<DeviceItemModel>(ds);

            //添加水平和垂直对齐线
            // 水平/垂直对齐线
            DeviceList.Add(new DeviceItemModel(_localDataAccess) { X = 0, Y = 0, Width = 2000, Height = 0.5, Z = 9999, DeviceType = "HL", IsVisible = false });
            DeviceList.Add(new DeviceItemModel(_localDataAccess) { X = 0, Y = 0, Width = 0.5, Height = 2000, Z = 9999, DeviceType = "VL", IsVisible = false });
            //宽高对齐线                       
            DeviceList.Add(new DeviceItemModel(_localDataAccess) { X = 0, Y = 0, Width = 0, Height = 15, Z = 9999, DeviceType = "WidthRule", IsVisible = false });
            DeviceList.Add(new DeviceItemModel(_localDataAccess) { X = 0, Y = 0, Width = 15, Height = 0, Z = 9999, DeviceType = "HeightRule", IsVisible = false });
        }
        private void DoSaveCommand(object obj)
        {

            //保存的时候 把对齐的横竖线给移除掉

            var ds = DeviceList.Where(d => !new string[] { "HL", "VL", "WidthRule", "HeightRule" }.Contains(d.DeviceType)).
                Select(dev => new DeviceEntity
                {
                    Header = dev.Header,
                    DeviceNum = dev.DeviceNum,
                    X = dev.X.ToString(),
                    Y = dev.Y.ToString(),
                    Z = dev.Z.ToString(),
                    W = dev.Width.ToString(),
                    H = dev.Height.ToString(),
                    FlowDirection = dev.FlowDirection.ToString(),
                    Rotate = dev.Rotate.ToString(),
                    DeviceTypeName = dev.DeviceType,
                    //转换属性集合
                    Props = dev.PropList.Select(dp => new DevicePropItemEntity
                    {
                        PropName = dp.PropName,
                        PropValue = dp.PropValue,
                    }).ToList(),
                    Vars = dev.VariableList.Select(dv => new VariableEntity
                    {
                        VarNum = dv.VarNum,
                        Header = dv.VarName,
                        Address = dv.VarAddress,
                        Offset = dv.Offset,
                        VarType = dv.VarType,
                        Modulus = dv.Modulus,
                        AlarmConditions = dv.AlarmConditions.Select(dva => new ConditionEntity
                        {
                            CNum = dva.CNum,
                            Operator = dva.Operator,
                            AlarmContent = dva.AlarmContent,
                            CompareValue = dva.CompareValue,
                        }).ToList(),
                        UnionConditions = dv.UnionConditions.Select(dva => new ConditionEntity
                        {
                            CNum = dva.CNum,
                            Operator = dva.Operator,
                            CompareValue = dva.CompareValue,
                            AlarmContent = dva.AlarmContent,
                            UnionDevices = dva.UnionDeviceList.Select(ud => new UDevuceEntity
                            {
                                UNum = ud.UNum,
                                DNum = ud.DNum,
                                VAddr = ud.Address,
                                Value = ud.Value,
                                VType = ud.ValueType
                            }).ToList()
                        }).ToList(),

                    }).ToList(),
                    ManualControls = dev.ManualControlList.Select(dm => new ManualEntity
                    {
                        Header = dm.ControlHeader,
                        Address = dm.ControlAddress,
                        Value = dm.Value,
                    }).ToList(),
                });
            try
            {
                _localDataAccess.SaveDevice(ds.ToList());
                VisualStateManager.GoToElementState(obj as Window, "NormalSucess", true);
                VisualStateManager.GoToElementState(obj as Window, "SaveSuccess", true);

                ActionManager.Execute("refresh");

            }
            catch (Exception ex)
            {
                SaveFailedMessage = ex.Message;
                VisualStateManager.GoToElementState(obj as Window, "SaveFailedNormal", true);
                VisualStateManager.GoToElementState(obj as Window, "SaveFailedShow", true);

            }

        }
        private List<string> InitOptions(string propName, out int OptionsDefaultIndex)
        {
            List<string> values = new List<string>();
            OptionsDefaultIndex = 0;
            switch (propName)
            {
                case "Protocol":
                    values.Add("ModbusRTU");
                    values.Add("ModbusASCII");
                    values.Add("ModbusTCP");
                    values.Add("S7COMM");
                    values.Add("FINSTcp");
                    values.Add("MC3E");
                    break;
                case "PortName":
                    values = new List<string>(SerialPort.GetPortNames());

                    break;
                case "BaudRate":
                    values.Add("2400");
                    values.Add("4800");
                    values.Add("9600");
                    values.Add("19200");
                    values.Add("38400");
                    values.Add("57600");
                    values.Add("115200");

                    OptionsDefaultIndex = 2;
                    break;
                case "DataBit":
                    values.Add("5");
                    values.Add("7");
                    values.Add("8");

                    OptionsDefaultIndex = 2;
                    break;
                case "Parity":
                    values = new List<string>(Enum.GetNames<Parity>());
                    break;
                case "StopBit":
                    values = new List<string>(Enum.GetNames<StopBits>());

                    OptionsDefaultIndex = 1;
                    break;

                case "Endian":
                    values = new List<string>(Enum.GetNames<EndianType>());
                    break;
                default: break;
            }

            return values;
        }

        #region 拖拽实现

        private void DoThumbCommand(object obj)
        {
            object[] values = (object[])obj;
            //DragSource 从什么里拖拽出来的
            //data 拖拽的对象实例
            //拖拽方式
            DragDrop.DoDragDrop(values[1] as Border, values[0] as ThumbItemModel, DragDropEffects.Copy);
        }
        private void DoItemDropCommand(object obj)
        {

            DragEventArgs e = obj as DragEventArgs;

            Point point = e.GetPosition((IInputElement)e.Source);
            var data = (ThumbItemModel)e.Data.GetData(typeof(ThumbItemModel));
            var dim = new DeviceItemModel(_localDataAccess)
            {

                DeviceNum = DateTime.Now.ToString("yyyyMMddHHmmss"),
                DeviceType = data.TargetType,
                Height = data.Height,
                Width = data.Width,
                X = point.X - data.Width / 2,
                Y = point.Y - data.Height / 2,
                Devices = () => DeviceList.ToList(),

            };
            dim.InitContextMenu();
            DeviceList.Add(dim);
        }

        #endregion


    }
}
